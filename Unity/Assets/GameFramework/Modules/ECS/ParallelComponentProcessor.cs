using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ECS {

    public class ThreadedSystemProcessor<T> : IDisposable where T : ISystemComponents {
        static readonly object _locker = new object();
        static Thread[] _workers;
        static int workerCount;
        static Queue<Action> _itemQ = new Queue<Action>();
        /*
        class WorkerTask {

        }
        static WorkerTask[] workerTasks;
        */

        //readonly object _workingCountLocker = new object();
        long workingCount = 0;


        public ThreadedSystemProcessor() {
            if(_workers == null) {
                workerCount = Environment.ProcessorCount;
                //workerTasks = new WorkerTask[workerCount];
                _workers = new Thread[workerCount];

                // Create and start a separate thread for each worker
                for (int i = 0; i < workerCount; i++)
                    //workerTasks[i] = new WorkerTask();
                    (_workers[i] = new Thread(Consume)).Start();
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
                    item = _itemQ.Dequeue();
                }
                // This signals our exit.
                if (item == null) return;
                // Execute item
                item();                           
            }
        }

        public void Process(List<T> componentsToProcess, Action<int, float> componentAction, float deltaTime) {
            //Stop here if there are no components to process
            if (componentsToProcess.Count == 0) return;

            //Make sure we are not using more threads than components
            int workersToUse = Math.Min(workerCount, componentsToProcess.Count);

            int componentChunk = (int)Math.Floor((float)componentsToProcess.Count / (float)workersToUse);
            for (int i = 0; i < workersToUse; ++i) {
                int threadID = i;

                //Setup worker tasks
                int startComponentIndex = threadID * componentChunk;
                int endComponentIndex = 0;
                if (threadID != workersToUse - 1) {
                    endComponentIndex = (threadID * componentChunk) + componentChunk - 1;
                }
                //Work on last chunk. Last chunk has more elements than other chunks if (this.systemComponents.Count % this.maxThreads != 0)
                else {
                    endComponentIndex = componentsToProcess.Count - 1;
                }

                //Create action to send to worker thread
                Action chunkAction = () => {
                    for (int componentIndex = startComponentIndex; componentIndex < endComponentIndex; ++componentIndex) {
                        componentAction(componentIndex, deltaTime);
                    }
                    Interlocked.Decrement(ref workingCount);
                };

                Interlocked.Increment(ref workingCount);
                //Send action to thread
                EnqueueItem(chunkAction);
            }

            //Wait for queue to finish
            long _workingCountCheck = Interlocked.Read(ref workingCount);
            while(_workingCountCheck > 0) {
                _workingCountCheck = Interlocked.Read(ref workingCount);
            }

            /*
            //wait for queue to finish
            lock (_locker) {
                while (_itemQ.Count > 0) Monitor.Wait(_locker);
                return;
            }
            */
        }

        public void Dispose() {
            Shutdown(true);
        }
    }



    public class ParallelSystemProcessor<T> : IDisposable where T : ISystemComponents{
        /// <summary>
        /// Worker threads used across all ParallelSystemProcessor
        /// </summary>
        static Task[] workers;

        /// <summary>
        /// All worker tasks per worker
        /// </summary>
        static ConcurrentQueue<WorkerTask>[] workerQueue;

        /// <summary>
        /// If > 0, main thread will wait.
        /// TODO: Use Wait & Pulse instead?
        /// </summary>
        long workerQueueCount;

        /// <summary>
        /// Class holding all data for a worker task
        /// </summary>
        struct WorkerTask {
            public List<T> componentsToProcess;
            public Action<int, float> componentAction;
            public int threadID;
            public int maxThreads;

            public float deltaTime;
            public int startComponentIndex;
            public int endComponentIndex;
        }
        /// <summary>
        /// Cached worker tasks for this ParallelSystemProcessor
        /// </summary>
        WorkerTask[] workerTaskCache;
        
        /// <summary>
        /// Components to process
        /// </summary>
        private List<T> componentsToProcess;

        private Action<int, float> componentAction;

        public ParallelSystemProcessor(List<T> componentsToProcess, Action<int, float> action) {
            /*
            //Spin up worker threads if they are not already running
            if(workers == null || workers.Length == 0) {

                //workers = new Task[Environment.ProcessorCount];
                workers = new Task[(int)(Environment.ProcessorCount*0.5f)];
                workerQueue = new ConcurrentQueue<WorkerTask>[workers.Length];
                for (int i=0; i< workers.Length; ++i) {
                    workerQueue[i] = new ConcurrentQueue<WorkerTask>();

                    int threadID = i;
                    //Spin up worker thread
                    workers[i] = Task.Factory.StartNew(() => {
                        WorkerTask taskCache;
                        while (true) {
                            //Work on task queue
                            if(workerQueue[threadID].Count > 0) {
                                workerQueue[threadID].TryDequeue(out taskCache);
                                float deltaTime = taskCache.deltaTime;
                                //Process components
                                for(int componentIndex=taskCache.startComponentIndex; componentIndex<=taskCache.endComponentIndex; ++componentIndex) {
                                    taskCache.componentAction(componentIndex, deltaTime);
                                }

                                Interlocked.Decrement(ref workerQueueCount);
                            }
                        }
                    }, TaskCreationOptions.LongRunning);
                }
            }
            */

            if(workers == null) {
                workers = new Task[(int)(Environment.ProcessorCount*0.5f)];
            }

            this.workerTaskCache = new WorkerTask[workers.Length];
            for(int i=0; i< this.workerTaskCache.Length; ++i) {
                int threadID = i;
                WorkerTask workerTask = new WorkerTask();
                workerTask.componentAction = action;
                workerTask.componentsToProcess = componentsToProcess;
                workerTask.maxThreads = workers.Length;
                workerTask.threadID = threadID;
                this.workerTaskCache[i] = workerTask;
            }
            this.componentsToProcess = componentsToProcess;
            this.componentAction = action;
        }

        /// <summary>
        /// Start processing. Locks the main thread and waits for worker threads to finish. Returns true once finished.
        /// </summary>
        /// <param name="deltaTime"></param>
        public bool Process(float deltaTime) {
            /*
            int componentChunk = (int)Math.Floor((float)this.componentsToProcess.Count / (float)workers.Length);
            for(int i=0; i<workers.Length; ++i) {
                int threadID = i;

                //Setup worker tasks
                workerTaskCache[threadID].startComponentIndex = threadID * componentChunk;
                workerTaskCache[threadID].deltaTime = deltaTime;
                if (threadID != workers.Length - 1) {
                    workerTaskCache[threadID].endComponentIndex = (threadID * componentChunk) + componentChunk - 1;
                }
                //Work on last chunk. Last chunk has more elements than other chunks if (this.systemComponents.Count % this.maxThreads != 0)
                else {
                    workerTaskCache[threadID].endComponentIndex = this.componentsToProcess.Count - 1;
                }

                //Start working on task
                Interlocked.Increment(ref workerQueueCount);
                workerQueue[threadID].Enqueue(workerTaskCache[threadID]);
            }

            long currentWorkerQueueCount = Interlocked.Read(ref workerQueueCount);
            while(currentWorkerQueueCount > 0) {
                currentWorkerQueueCount = Interlocked.Read(ref workerQueueCount);
            }
            */


            int componentChunk = (int)Math.Floor((float)this.componentsToProcess.Count / (float)workers.Length);
            for (int i = 0; i < workers.Length; ++i) {
                int threadID = i;

                //Setup worker tasks
                workerTaskCache[threadID].startComponentIndex = threadID * componentChunk;
                workerTaskCache[threadID].deltaTime = deltaTime;
                if (threadID != workers.Length - 1) {
                    workerTaskCache[threadID].endComponentIndex = (threadID * componentChunk) + componentChunk - 1;
                }
                //Work on last chunk. Last chunk has more elements than other chunks if (this.systemComponents.Count % this.maxThreads != 0)
                else {
                    workerTaskCache[threadID].endComponentIndex = this.componentsToProcess.Count - 1;
                }

                workers[i] = Task.Factory.StartNew(() => {
                    WorkerTask workerTask = workerTaskCache[threadID];
                    Action<int, float> workerAction = workerTask.componentAction;
                    for (int componentIndex= workerTask.startComponentIndex; componentIndex< workerTask.endComponentIndex; ++componentIndex) {
                        workerAction(componentIndex, deltaTime);
                    }
                });
            }

            //wait for all tasks to finish
            Task.WaitAll(workers);


            return true;
        }

        public void Dispose() {
            componentsToProcess = null;
            workerTaskCache = null;

            if(workers != null) {
                foreach(Task t in workers) {
                    t.Dispose();
                }
                workers = null;
            }
        }
    }

    
    /// <summary>
    /// Helper class to process ECS system components in parallel.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ParallelSystemComponentsProcessor<T> : IDisposable where T : ISystemComponents {
        int maxThreads = 0;
        int _currentThreads = 0;
        object lock_currentThreads = new object();
        protected int CurrentThreads {
            get {
                lock (lock_currentThreads) {
                    return _currentThreads;
                }
            }
            set {
                lock (lock_currentThreads) {
                    _currentThreads = value;
                }
            }
        }
        List<T> componentsToProcess;
        Action<int, float> componentAction;
        Task[] tasks;
        Action[] taskAction;

        public void Setup(Action<int, float> componentAction, List<T> componentsToProcess, int maxThreads = 0) {
            if (this.maxThreads == 0) this.maxThreads = Environment.ProcessorCount;
            this.componentsToProcess = componentsToProcess;
            this.componentAction = componentAction;
            this.tasks = new Task[this.maxThreads];
            this.taskAction = new Action[this.maxThreads];
            for(int i=0; i<this.maxThreads; ++i) {
                int workerID = i;
                this.taskAction[i] = () => DoWork(workerID, 0.01f);
            }
        }

        public void Invoke(float deltaTime) {
            //Not cool. Creates garbage.
            //Create some sort of thread pool, then keep threads alive and give them work to do instead of creating new tasks/threads
            int degreeOfParallelism = Environment.ProcessorCount;
            ParallelLoopResult result = Parallel.For(0, degreeOfParallelism, workerId => {
                var max = componentsToProcess.Count * (workerId + 1) / degreeOfParallelism;
                for (int i = componentsToProcess.Count * workerId / degreeOfParallelism; i < max; i++)
                    //array[i] = array[i] * factor;
                    componentAction(i, deltaTime);
            });
            while (!result.IsCompleted) { }

            /*
            //CurrentThreads = 1;
            for(int i=0; i<maxThreads; ++i) {
                this.tasks[i] = Task.Factory.StartNew(this.taskAction[i]);
            }

            Task t = Task.WhenAll(this.tasks);
            while (!t.IsCompleted) { }
            */
            /*
            Task t = Task.Factory.StartNew(() => {
                for (int i = 0; i < componentsToProcess.Count; ++i) {
                    componentAction(i, deltaTime);
                }
                //CurrentThreads = 0;
            });
            while (!t.IsCompleted) { }
            */
        }

        void DoWork(int workerID, float deltaTime) {
            int componentChunk = (int)Math.Floor((float)this.componentsToProcess.Count / (float)this.maxThreads);
            //Work on chunks
            if (workerID != maxThreads - 1) {
                for (int i = workerID * componentChunk; i < (workerID * componentChunk) + componentChunk; ++i) {
                    this.componentAction(i, deltaTime);
                }
            }
            //Work on last chunk. Last chunk has more elements than other chunks if (this.systemComponents.Count % this.maxThreads != 0)
            else {
                for (int i = workerID * componentChunk; i < componentsToProcess.Count; ++i) {
                    this.componentAction(i, deltaTime);
                }
            }
        }

        /*
        void ThreadProc(object state) {
            int componentChunk = (int)Math.Floor((float)this.systemComponents.Count / (float)this.maxThreads);
            //Work on chunks
            if (threadID != maxThreads - 1) {
                for (int i = this.threadID * componentChunk; i < (this.threadID * componentChunk) + componentChunk; ++i) {
                    this.componentAction(i, this.deltaTime);
                }
            }
            //Work on last chunk. Last chunk has more elements than other chunks if (this.systemComponents.Count % this.maxThreads != 0)
            else {
                for (int i = this.threadID * componentChunk; i < systemComponents.Count; ++i) {
                    this.componentAction(i, this.deltaTime);
                }
            }

            this.parallelProcessor.CurrentThreads -= 1;
        }
        */


        public void Dispose() {
        }
    }
}