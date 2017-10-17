using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UnityEngine;

namespace Service.AsyncManager {
    public class AsyncFuture {
        public bool finished = false;


        /// <summary>
        /// Wait inside a coroutine till this future comes true
        /// </summary>
        /// <returns></returns>
        public IEnumerator CoWait(int timeout=-1) {
            long waitTill = AsyncManagerImpl.getCurrentTimeMs() + timeout;
            while (!finished) {
                yield return null;
                if (timeout!=-1 && AsyncManagerImpl.getCurrentTimeMs() > waitTill) {
                    Debug.LogWarning("TIMEOUT COWait!");
                    break;
                }
            }
        }
    }

    class AsyncManagerImpl : AsyncManagerBase {


        /// <summary>
        /// The max amount the actions that get executed on the main thread block the main-thread
        /// This only ensures that after one action the execution is stopped till next frame. the action
        /// still needs to be as low latency as possible. There is no timeout or such
        /// </summary>
        private const int MAIN_THREAD_MAX_MS = 75;

        private static int MAINTHREAD_ID = Thread.CurrentThread.ManagedThreadId;

        private class ConcurrentWorker {
            public ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();
        }

        private class Worker {
            public Queue<Action> actions = new Queue<Action>();
        }

        private IDisposable disposableMainThreadWorker = null;
        private IDisposable disposableWorker = null;

        private Worker workerMainThread;
        private ConcurrentWorker workerWorkerThread;

        public static long getCurrentTimeMs() {
            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            return milliseconds;
        }

        protected override void AfterInitialize() {
            workerMainThread = new Worker();
            workerWorkerThread = new ConcurrentWorker();
        }

        public static bool IsMainThread() {
            return Thread.CurrentThread.ManagedThreadId == MAINTHREAD_ID;
        }

        /// <summary>
        /// Add an action and its callback
        /// </summary>
        /// <param name="act"></param>
        /// <param name="onFinished"></param>
        public override AsyncFuture AddToWorkerThread(Action act,Action onFinished) {
            var result = new AsyncFuture() { finished = false };
            workerWorkerThread.actions.Enqueue(act);
            workerWorkerThread.actions.Enqueue(()=> { result.finished = true; });
            if (onFinished != null) {
                workerWorkerThread.actions.Enqueue(onFinished);
            }
            if (disposableWorker == null) {
                disposableWorker = Observable.Start(ThreadWorkerAction).ObserveOnMainThread().Subscribe(_=> {
                    disposableWorker=null;
                });
            }
            return result;
        }

        private void ThreadWorkerAction() {
            while (workerWorkerThread.actions.Count > 0) {
                Action act;
                workerWorkerThread.actions.TryDequeue(out act);
                if (act != null) {
                    act();
                } else {
                    // busy wait
                }
            }
        }

        /// <summary>
        /// Add an action to be executated queued making sure to not take more time than set in MAIN_THREAD_MAX_MS (more or less)
        /// </summary>
        /// <param name="act"></param>
        public override AsyncFuture AddToMainThread(Action act) {
            var result = new AsyncFuture() { finished = false };
            workerMainThread.actions.Enqueue(act);
            workerMainThread.actions.Enqueue(() => { result.finished = true; });
            // start the worker to process the queue if not running
            if (disposableMainThreadWorker == null) {
                disposableMainThreadWorker = Observable.FromMicroCoroutine((cToken) => CoRoutineMainThreadWorker()).Subscribe(_ => {
                    disposableMainThreadWorker = null;
                });
            }
            return result;
        }

        private IEnumerator CoRoutineMainThreadWorker() {
            // calculate when the next break have to be done
            long maxTimeForWorkerTillBreak = getCurrentTimeMs() + MAIN_THREAD_MAX_MS;
            int count = 0;
            while (workerMainThread.actions.Count > 0) {
                Action act = workerMainThread.actions.Dequeue() as Action;
                act();
                count++;
                // did we already violate the maxTime? If yes, break
                if (getCurrentTimeMs() >= maxTimeForWorkerTillBreak) {
                    //Debug.Log("ThreadManager: handled " + count + " actions per Call");
                    yield return null;
                    count = 0;
                    maxTimeForWorkerTillBreak = getCurrentTimeMs() + MAIN_THREAD_MAX_MS;
                }
            }
        }

        /// <summary>
        /// Convenience-Method: Call an action either immedately or queue it to MainThread
        /// </summary>
        /// <param name="act">the action</param>
        /// <param name="usingCoroutine">queue action to async-main-queue</param>
        /// <returns>Future that tells when or if the action is called</returns>
        public override AsyncFuture Call(Action act, bool usingCoroutine) {
            if (usingCoroutine) {
                // queue to async-queue
                return _asyncManager.AddToMainThread(act);
            } else {
                var result = new AsyncFuture() { finished = true };
                // immediately call
                act();
                return result;
            }
        }

        /// <summary>
        /// Dispose the current running threads
        /// </summary>
        public override void DisposeThreads() {
            if (disposableMainThreadWorker != null) {
                disposableMainThreadWorker.Dispose();
                disposableMainThreadWorker = null;
            }
            if (disposableWorker != null) {
                disposableWorker.Dispose();
                disposableWorker = null;
            }
            workerMainThread.actions.Clear();
        }

        protected override void OnDispose() {
            // do your IDispose-actions here. It is called right after disposables got disposed
        }


    }
}
