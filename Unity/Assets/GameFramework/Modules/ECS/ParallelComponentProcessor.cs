using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ECS {

    public class ParallelSystemComponentsProcessor<T> : IDisposable where T : ISystemComponents {
        static readonly object _locker = new object();
        static Thread[] _workers;
        static int workerCount;
        static ConcurrentQueue<Action> _itemQ = new ConcurrentQueue<Action>();

        static readonly object _workingCountLocker = new object();
        static long workingCount = 0;
        
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
            if (_workers == null) {
                workerCount = Math.Max(Environment.ProcessorCount - 1, 0); //Math.Max(1, (int)(Environment.ProcessorCount*0.5f));// Math.Max(Environment.ProcessorCount-1, 0);
                _workers = new Thread[workerCount];
                // Create and start a separate thread for each worker
                for (int i = 0; i < workerCount; i++) {
                    Thread t = new Thread(Consume);
                    t.IsBackground = true;
                    t.Start();
                    //Task t = Task.Factory.StartNew(Consume, TaskCreationOptions.LongRunning);
                    _workers[i] = t;
                }                    
            }

            processActionData = new ProcessActionData[workerCount];
            processActions = new Action[workerCount];
            for (int i = 0; i < workerCount; ++i) {
                processActionData[i] = new ProcessActionData();
                int threadID = i;
                processActions[i] = () => {
                    //Thread.MemoryBarrier();
                    ProcessActionData processData = processActionData[threadID];
                    for (int componentIndex = processData.startComponentIndex; componentIndex <= processData.endComponentIndex; ++componentIndex) {
                        componentAction(componentIndex, processData.deltaTime);
                    }
                    //Interlocked.Decrement(ref workingCount);
                    lock (_workingCountLocker) {
                        workingCount--;
                        Monitor.Pulse(_workingCountLocker);
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
            foreach (Thread worker in _workers)
                EnqueueItem(null);

            // Wait for workers to finish
            if (waitForWorkers)
                foreach (Thread worker in _workers)
                    worker.Join();
        }

        /// <summary>
        /// Add a new item to the worker queue
        /// </summary>
        /// <param name="item"></param>
        void EnqueueItem(Action item) {
            lock (_locker) {
                _itemQ.Enqueue(item);
                // We must pulse because we're changing a blocking condition.
                Monitor.Pulse(_locker);
            }
        }

        /// <summary>
        /// Consume and process a worker item
        /// </summary>
        void Consume() {
            // Keep consuming until told otherwise.
            while (true) {
                Action item;
                lock (_locker) {
                    while (_itemQ.Count == 0) Monitor.Wait(_locker);
                    if(!_itemQ.TryDequeue(out item)) {
                        continue;
                    }
                }
                // This signals our exit.
                if (item == null) return;
                // Execute item
                item();
            }
        }

        public void Process(List<T> componentsToProcess, float deltaTime) {
            //Stop here if there are no components to process
            if (componentsToProcess.Count == 0) return;

            //Make sure we are not using more threads than components
            int workersToUse = Math.Min(workerCount, componentsToProcess.Count);

            Interlocked.Exchange(ref workingCount, workersToUse);

            int componentChunk = (int)Math.Floor((float)componentsToProcess.Count / (float)workersToUse);
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
                    endComponentIndex = componentsToProcess.Count - 1;
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
            lock (_workingCountLocker) {
                while (workingCount > 0) Monitor.Wait(_workingCountLocker);
            }
        }

        public void Dispose() {
            Shutdown(true);
        }
    }
}