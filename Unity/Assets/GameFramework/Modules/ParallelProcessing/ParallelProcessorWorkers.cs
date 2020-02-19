using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ParallelProcessing {
    /// <summary>
    /// Helper class, holding static thread data used by ParallelProcessor<T>. This is very static and ugly on purpose, to increase overall performance.
    /// TODO: Refactor if i ever have an idea on how to make this more elegant and non-static.
    /// </summary>
    public static class ParallelProcessorWorkers {
        static readonly object _locker = new object();
        static Thread[] _workers;
        
        static ConcurrentQueue<Action> _itemQ = new ConcurrentQueue<Action>();


        public static Thread[] Workers {
            get { return _workers; }
        }
        public static int WorkerCount { get; private set; }
        public static readonly object _workingCountLocker = new object();
        public static long _workingCount = 0;
        
        /// <summary>
        /// Processors using the workers
        /// </summary>
        public static int _processorInstanceCount = 0;

        public static void Setup() {
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
}