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
    public static class ParallelSystemComponentsProcessorWorkers{
        public static readonly object _locker = new object();
        public static Thread[] _workers;
        public static int workerCount;
        public static ConcurrentQueue<Action> _itemQ = new ConcurrentQueue<Action>();

        public static readonly object _workingCountLocker = new object();
        public static long workingCount = 0;

        /// <summary>
        /// Systems using this processor. 
        /// </summary>
        public static int componentsProcessorInstanceCount = 0;
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
            ParallelSystemComponentsProcessorWorkers.componentsProcessorInstanceCount++;

            if (ParallelSystemComponentsProcessorWorkers._workers == null) {
                ParallelSystemComponentsProcessorWorkers.workerCount = Math.Max(Environment.ProcessorCount, 0); //Math.Max(1, (int)(Environment.ProcessorCount*0.5f));// Math.Max(Environment.ProcessorCount-1, 0);
                ParallelSystemComponentsProcessorWorkers._workers = new Thread[ParallelSystemComponentsProcessorWorkers.workerCount];
                // Create and start a separate thread for each worker
                for (int i = 0; i < ParallelSystemComponentsProcessorWorkers.workerCount; i++) {
                    Thread t = new Thread(Consume);
                    t.Name = "ParallelComponentProcessor-" + i.ToString();
                    t.IsBackground = true;
                    t.Start();
                    //Task t = Task.Factory.StartNew(Consume, TaskCreationOptions.LongRunning);
                    ParallelSystemComponentsProcessorWorkers._workers[i] = t;
                }                    
            }

            processActionData = new ProcessActionData[ParallelSystemComponentsProcessorWorkers.workerCount];
            processActions = new Action[ParallelSystemComponentsProcessorWorkers.workerCount];
            for (int i = 0; i < ParallelSystemComponentsProcessorWorkers.workerCount; ++i) {
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
                        lock (ParallelSystemComponentsProcessorWorkers._workingCountLocker) {
                            ParallelSystemComponentsProcessorWorkers.workingCount--;
                            Monitor.Pulse(ParallelSystemComponentsProcessorWorkers._workingCountLocker);
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
            foreach (Thread worker in ParallelSystemComponentsProcessorWorkers._workers)
                EnqueueItem(null);

            // Wait for workers to finish
            if (waitForWorkers)
                foreach (Thread worker in ParallelSystemComponentsProcessorWorkers._workers)
                    worker.Join();
        }

        /// <summary>
        /// Add a new item to the worker queue
        /// </summary>
        /// <param name="item"></param>
        void EnqueueItem(Action item) {
            lock (ParallelSystemComponentsProcessorWorkers._locker) {
                ParallelSystemComponentsProcessorWorkers._itemQ.Enqueue(item);
                // We must pulse because we're changing a blocking condition.
                Monitor.Pulse(ParallelSystemComponentsProcessorWorkers._locker);
            }
        }

        /// <summary>
        /// Consume and process a worker item
        /// </summary>
        void Consume() {
            // Keep consuming until told otherwise.
            while (true) {
                Action item;
                lock (ParallelSystemComponentsProcessorWorkers._locker) {
                    while (ParallelSystemComponentsProcessorWorkers._itemQ.Count == 0) Monitor.Wait(ParallelSystemComponentsProcessorWorkers._locker);
                    if(!ParallelSystemComponentsProcessorWorkers._itemQ.TryDequeue(out item)) {
                        continue;
                    }
                }
                // This signals our exit.
                if (item == null) return;

                // Execute item
                try {
                    item();
                } catch(Exception e) {
                    //Console.WriteLine(e.Message);
                    UnityEngine.Debug.LogException(e);
                }
                
            }
        }
        public void Process(ICollection componentsToProcess, float deltaTime) {
            Process(componentsToProcess.Count, deltaTime);
        }

        public void Process(int componentsToProcessCount, float deltaTime) {
            //Stop here if there are no components to process
            if (componentsToProcessCount == 0) return;

            //Make sure we are not using more threads than components
            int workersToUse = Math.Min(ParallelSystemComponentsProcessorWorkers.workerCount, componentsToProcessCount);

            Interlocked.Exchange(ref ParallelSystemComponentsProcessorWorkers.workingCount, workersToUse);

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
                EnqueueItem(processActions[workerID]);
            }

            /*
            //Wait for queue to finish
            long _workingCountCheck = Interlocked.Read(ref workingCount);
            while(_workingCountCheck > 0) {
                _workingCountCheck = Interlocked.Read(ref workingCount);
            }
            */

            //Wait for queue to finish
            lock (ParallelSystemComponentsProcessorWorkers._workingCountLocker) {
                while (ParallelSystemComponentsProcessorWorkers.workingCount > 0) Monitor.Wait(ParallelSystemComponentsProcessorWorkers._workingCountLocker);
            }
        }

        public void Dispose() {
            ParallelSystemComponentsProcessorWorkers.componentsProcessorInstanceCount--;
            if(ParallelSystemComponentsProcessorWorkers.componentsProcessorInstanceCount <= 0) {
                Shutdown(true);
            }
        }
    }
}