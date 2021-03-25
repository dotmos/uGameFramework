using ParallelProcessing;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ECS {

    public class FutureProcessor {
        private ParallelProcessor parallelQueue;
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
            parallelQueue = new ParallelProcessor(ProcessParallelQueueItem);
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
            if (!Kernel.IsMainThread()) {
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

        public void AddFuture(Future f,bool forceEnqueue=false) {
            if (f.ExecutionMode == FutureExecutionMode.onMainThread) {
                if (!forceEnqueue && Kernel.IsMainThread()) {
                    f.execute();
                } else {
                    mainThreadActions.Enqueue(f);
                }
            } else if (f.ExecutionMode == FutureExecutionMode.onParallelQueue) {
                parallelQueueActions.Enqueue(f);
            } else if (f.ExecutionMode == FutureExecutionMode.onOwnTask) {
                f.task = new Task(() => {
                    Thread.CurrentThread.Name = "future";
                    try
                    {
                        f.execute();
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                });
                f.task.Start();
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

        public enum FutureState {
            none,created,executed,finished,error,running
        }

        private Semaphore semaphore = new Semaphore(0, 1);

        private Func<object> logic;
        private object result;
        private bool finished = false;
        public FutureState state = FutureState.none;
        public Task task = null; 

        private FutureExecutionMode executionMode;
        public FutureExecutionMode ExecutionMode { get => executionMode; }

        public Future(FutureExecutionMode executionMode, Func<object> logic, bool addToFutureProcessor = true, bool forceEnqueue=false) {
            state = FutureState.created;
            this.logic = logic;
            this.executionMode = executionMode;

            if (addToFutureProcessor) FutureProcessor.Instance.AddFuture(this,forceEnqueue);
        }

        public bool IsFinished() {
            return finished;
        }

        public void execute() {
            try {
                state = FutureState.running;
                result = logic();
                state = FutureState.finished;
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError("Crashed future!");
                UnityEngine.Debug.LogException(e);
                state = FutureState.error;
            }
            finished = true;
            semaphore.Release();
        }

        public void WaitForFinish() {
            WaitForResult<object>();
        }

        public T WaitForResult<T>(int timeout=-1) {
            if (!finished) {
                if (executionMode == FutureExecutionMode.onParallelQueue) {
                    // waiting on main thread flushes/executes the current registered futures
                    FutureProcessor.Instance.ProcessParallelQueue();
                    if (result == null) {
                        UnityEngine.Debug.LogError("Waited for result on mainthread but got none!");
                    }
                } else {
                    if (executionMode == FutureExecutionMode.onMainThread && Kernel.IsMainThread()) {
                        UnityEngine.Debug.LogError("No result but on mainthread! future should have already been executed");
                    } else {
                        // the futures are executed on the 
                        if (timeout == -1) {
                            semaphore.WaitOne();
                        } else {
                            semaphore.WaitOne(timeout);
                        }
                    }
                }
            }
            return (T)result;
        }
    }

}
