using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ECS {

    public class FutureProcessor {
        private ParallelSystemComponentsProcessor<object> parallelQueue;
        private ConcurrentQueue<Future> parallelQueueActions = new ConcurrentQueue<Future>();
        private ConcurrentQueue<Future> mainThreadActions = new ConcurrentQueue<Future>();

        System.Diagnostics.Stopwatch timeCheck = new System.Diagnostics.Stopwatch();
        /// <summary>
        /// The max time after which the mainThread-actions executions stop till next call
        /// </summary>
        private float timePerMainframe;

        private static FutureProcessor instance = new FutureProcessor();

        public static FutureProcessor Instance { get => instance; }

        public FutureProcessor(float maxMsPerMainThread=250.0f) {
            parallelQueue = new ParallelSystemComponentsProcessor<object>(ProcessParallelQueueItem);
            timePerMainframe = maxMsPerMainThread;
        }

        private void ProcessParallelQueueItem(int idx, float dt) {
            if (parallelQueueActions.TryDequeue(out Future action)) {
                action.execute();
            }
        }

        public bool HasMainThreadActions() {
            return mainThreadActions.Count > 0;
        }

        public void ProcessMainThreadActions() {
            if (!Kernel.Instance.IsMainThread()) {
                UnityEngine.Debug.LogError("You have to be on mainthread to execute the mainthread-actions");
                return;
            }
            int amount = mainThreadActions.Count;
            timeCheck.Restart();
            while (mainThreadActions.TryDequeue(out Future future)) {
                future.execute();
                if (timeCheck.ElapsedMilliseconds > timePerMainframe) break;
            }
        }

        public void ProcessParallelQueue(float dt=0.0f) {
            parallelQueue.Process(parallelQueueActions.Count, dt);
        }

        public void AddFuture(Future f) {
            if (f.ExecutionMode == FutureExecutionMode.onMainThread) {
                if (Kernel.Instance.IsMainThread()) {
                    f.execute();
                } else {
                    mainThreadActions.Enqueue(f);
                }
            } else if (f.ExecutionMode == FutureExecutionMode.onParallelQueue) {
                parallelQueueActions.Enqueue(f);
            } else if (f.ExecutionMode == FutureExecutionMode.onOwnTask) {
                Task t = new Task(() => { f.execute(); });
                t.Start();
            }
        }
    }

    public enum FutureExecutionMode {
        /// <summary>
        /// the future is executed on the main-thread
        /// </summary>
        onMainThread,
        /// <summary>
        /// a new thread is created for this future
        /// </summary>
        onOwnTask,
        /// <summary>
        /// the future is executed by an parallelQueue on the workthread
        /// </summary>
        onParallelQueue
    }

    public class Future {

        private Semaphore semaphore = new Semaphore(0, 1);

        private Func<object> logic;
        private object result;
        private bool finished = false;
        private FutureExecutionMode executionMode;
        public FutureExecutionMode ExecutionMode { get => executionMode; }

        public Future(FutureExecutionMode executionMode, Func<object> logic, bool addToFutureProcessor = true) {
            this.logic = logic;
            this.executionMode = executionMode;

            if (addToFutureProcessor) FutureProcessor.Instance.AddFuture(this);
        }

        public bool IsFinished() {
            return finished;
        }

        public void execute() {
            result = logic();
            finished = true;
            semaphore.Release();
        }

        public void WaitForFinish() {
            WaitForResult<object>();
        }

        public T WaitForResult<T>() {
            if (!finished) {
                if (executionMode == FutureExecutionMode.onParallelQueue) {
                    // waiting on main thread flushes/executes the current registered futures
                    FutureProcessor.Instance.ProcessParallelQueue();
                    if (result == null) {
                        UnityEngine.Debug.LogError("Waited for result on mainthread but got none!");
                    }
                } else {
                    if (executionMode == FutureExecutionMode.onMainThread && Kernel.Instance.IsMainThread()) {
                        UnityEngine.Debug.LogError("No result but on mainthread! future should have already been executed");
                    } else {
                        // the futures are executed on the 
                        semaphore.WaitOne();
                    }
                }
            }
            return (T)result;
        }
    }

}
