using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ECS {

    /// <summary>
    /// Helper class, holding static thread data used by ParallelSystemComponentsProcessor<T>
    /// </summary>
    public static class ParallelProcessorWorkers{
        static readonly object _locker = new object();
        static Thread[] _workers;
        public static int WorkerCount { get; private set; }
        public static ConcurrentQueue<Action> _itemQ = new ConcurrentQueue<Action>();

        public static readonly object _workingCountLocker = new object();
        public static long workingCount = 0;

        /// <summary>
        /// Processors using the workers
        /// </summary>
        public static int processorInstanceCount = 0;

        public static Thread[] GetWorkers() {
            if (_workers == null) {
                WorkerCount = Math.Max(Environment.ProcessorCount, 0); //Math.Max(1, (int)(Environment.ProcessorCount*0.5f));// Math.Max(Environment.ProcessorCount-1, 0);
                _workers = new Thread[WorkerCount];
                // Create and start a separate thread for each worker
                for (int i = 0; i < WorkerCount; i++) {
                    Thread t = new Thread(Consume);
                    t.Name = "ParallelComponentProcessor-" + i.ToString();
                    t.IsBackground = true;
                    t.Start();
                    //Task t = Task.Factory.StartNew(Consume, TaskCreationOptions.LongRunning);
                    _workers[i] = t;
                }
            }

            return _workers;
        }

        /// <summary>
        /// Consume and process a worker item
        /// </summary>
        static void Consume() {
            // Keep consuming until told otherwise.
            while (true) {
                Action item;
                lock (_locker) {
                    while (_itemQ.Count == 0) Monitor.Wait(_locker);
                    if (!_itemQ.TryDequeue(out item)) {
                        continue;
                    }
                }
                // This signals our exit.
                if (item == null) return;

                // Execute item
                try {
                    item();
                }
                catch (Exception e) {
                    //Console.WriteLine(e.Message);
                    UnityEngine.Debug.LogException(e);
                }

            }
        }

        /// <summary>
        /// Add a new item to the worker queue
        /// </summary>
        /// <param name="item"></param>
        public static void EnqueueItem(Action item) {
            lock (_locker) {
                _itemQ.Enqueue(item);
                // We must pulse because we're changing a blocking condition.
                Monitor.Pulse(_locker);
            }
        }
    }

    public class ParallelSystemComponentsProcessor<T> : IDisposable /*where T : ISystemComponents*/ {
        
        
        /// <summary>
        /// Cached action to process for this system
        /// </summary>
        Action[] processActions;
        class ProcessActionData {
            public int startComponentIndex;
            public int endComponentIndex;
            public float deltaTime;
        }
        /// <summary>
        /// Data to process in processActions.
        /// </summary>
        ProcessActionData[] processActionData;


        public ParallelSystemComponentsProcessor(Action<int, float> componentAction) {
            ParallelProcessorWorkers.processorInstanceCount++;

            processActionData = new ProcessActionData[ParallelProcessorWorkers.WorkerCount];
            processActions = new Action[ParallelProcessorWorkers.WorkerCount];
            for (int i = 0; i < ParallelProcessorWorkers.WorkerCount; ++i) {
                processActionData[i] = new ProcessActionData();
                int threadID = i;
                processActions[i] = () => {
                    //Thread.MemoryBarrier();
                    try {
                        ProcessActionData processData = processActionData[threadID];
                        for (int componentIndex = processData.startComponentIndex; componentIndex <= processData.endComponentIndex; ++componentIndex) {
                            componentAction(componentIndex, processData.deltaTime);
                        }
                    }
                    catch (Exception e) {
                        Console.WriteLine("Error in ParallelSystemComponentsProcessor: "+e.Message);
                        UnityEngine.Debug.LogException(e);
                    }
                    finally {
                        //Interlocked.Decrement(ref workingCount);
                        lock (ParallelProcessorWorkers._workingCountLocker) {
                            ParallelProcessorWorkers.workingCount--;
                            Monitor.Pulse(ParallelProcessorWorkers._workingCountLocker);
                        }
                    }
                };
            }


        }



        /// <summary>
        /// Stop processing
        /// </summary>
        /// <param name="waitForWorkers"></param>
        void Shutdown(bool waitForWorkers) {
            // Enqueue one null item per worker to make each exit.
            foreach (Thread worker in ParallelProcessorWorkers.GetWorkers())
                ParallelProcessorWorkers.EnqueueItem(null);

            // Wait for workers to finish
            if (waitForWorkers)
                foreach (Thread worker in ParallelProcessorWorkers.GetWorkers())
                    worker.Join();
        }
        
        public void Process(ICollection componentsToProcess, float deltaTime) {
            Process(componentsToProcess.Count, deltaTime);
        }

        public void Process(int componentsToProcessCount, float deltaTime) {
            //Stop here if there are no components to process
            if (componentsToProcessCount == 0) return;

            //Make sure we are not using more threads than components
            int workersToUse = Math.Min(ParallelProcessorWorkers.WorkerCount, componentsToProcessCount);

            Interlocked.Exchange(ref ParallelProcessorWorkers.workingCount, workersToUse);

            int componentChunk = (int)Math.Floor((float)componentsToProcessCount / (float)workersToUse);
            for (int i = 0; i < workersToUse; ++i) {
                int workerID = i;

                //Setup worker tasks
                int startComponentIndex = workerID * componentChunk;
                int endComponentIndex = 0;
                if (workerID != workersToUse - 1) {
                    endComponentIndex = (workerID * componentChunk) + componentChunk - 1;
                }
                //Work on last chunk. Last chunk has more elements than other chunks if (this.systemComponents.Count % this.maxThreads != 0)
                else {
                    endComponentIndex = componentsToProcessCount - 1;
                }
                
                //Set process action data
                processActionData[workerID].deltaTime = deltaTime;
                processActionData[workerID].startComponentIndex = startComponentIndex;
                processActionData[workerID].endComponentIndex = endComponentIndex;

                //Process action on worker
                ParallelProcessorWorkers.EnqueueItem(processActions[workerID]);
            }

            /*
            //Wait for queue to finish
            long _workingCountCheck = Interlocked.Read(ref workingCount);
            while(_workingCountCheck > 0) {
                _workingCountCheck = Interlocked.Read(ref workingCount);
            }
            */

            //Wait for queue to finish
            lock (ParallelProcessorWorkers._workingCountLocker) {
                while (ParallelProcessorWorkers.workingCount > 0) Monitor.Wait(ParallelProcessorWorkers._workingCountLocker);
            }
        }

        public void Dispose() {
            ParallelProcessorWorkers.processorInstanceCount--;
            if(ParallelProcessorWorkers.processorInstanceCount <= 0) {
                Shutdown(true);
            }
        }
    }
}