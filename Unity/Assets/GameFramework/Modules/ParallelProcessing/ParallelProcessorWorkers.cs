using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace ParallelProcessing {
    /// <summary>
    /// Helper class, holding static thread data used by ParallelProcessor<T>. This is very static and ugly on purpose, to increase overall performance.
    /// TODO: Refactor if i ever have an idea on how to make this more elegant and non-static.
    /// </summary>
    public static class ParallelProcessorWorkers {
        //static readonly object _locker = new object();
        //static Thread[] _workers;
        
        static Worker[] _workers;
        
        static ConcurrentQueue<Action> _itemQ = new ConcurrentQueue<Action>();

        public static Worker[] Workers {
            get { return _workers; }
        }
#if UNITY_PS5 && (!UNITY_EDITOR)
        readonly static int _workerCount = System.Math.Min(Environment.ProcessorCount * 2, 10);// Math.Max(1, Environment.ProcessorCount-1);
#else 
        readonly static int _workerCount = Environment.ProcessorCount;// Math.Max(1, Environment.ProcessorCount-1);
#endif

        private static bool throttleWorkers = false;

        public static void ThrottleWorkers(bool throttle,int throttleWorkerAmount = 1) {
            throttleWorkers = throttle;
            ThrottleWorkerAmount = throttleWorkerAmount;
        }
        public static int ThrottleWorkerAmount { get; set; } = 1;
        public static int MaxWorkerCount { get { return throttleWorkers ? ThrottleWorkerAmount : _workerCount; }}
        public static readonly object _workingCountLocker = new object();
        public static long _workingCount = 0;
        
        /// <summary>
        /// Processors using the workers
        /// </summary>
        public static int _processorInstanceCount = 0;

        public static void Setup() {
            if (_workers == null) {
                _workers = new Worker[MaxWorkerCount];
                // Create and start a separate thread for each worker
                for (int i = 0; i < MaxWorkerCount; i++) {
                    int workerID = i;
                    Worker worker = new Worker();

                    Thread t = new Thread(worker.Consume);
                    t.Name = "ParallelComponentProcessor-" + i.ToString();
                    t.IsBackground = true;
                    t.Priority = ThreadPriority.Highest;

                    worker.thread = t;
                    worker.workerID = workerID;
                    _workers[i] = worker;

                    t.Start();
                    //Task t = Task.Factory.StartNew(Consume, TaskCreationOptions.LongRunning);
                }
            }
        }

        
        public class Worker {
            public Thread thread;
            public int workerID;
            public readonly object _locker = new object();

            /// <summary>
            /// Consume and process a worker item
            /// </summary>
            public void Consume() {
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
            public void EnqueueItem(Action item) {
                lock (_locker) {
                    _itemQ.Enqueue(item);
                    // We must pulse because we're changing a blocking condition.
                    Monitor.Pulse(_locker);
                }
            }

            public void Join() {
                thread.Join();
            }
        }

        /// <summary>
        /// Add a new item to the queue of the given worker
        /// </summary>
        /// <param name="item"></param>
        public static void EnqueueItem(int workerID, Action item) {
            _workers[workerID].EnqueueItem(item);
        }
    }
}
