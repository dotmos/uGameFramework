using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

    public class AsyncAction {
        public bool global = false; // if true the action will not be disposed when calling DisposeThreads(true)
        public Action action = null; 
    }

    partial class AsyncManagerImpl : AsyncManagerBase {

        // flag to be set so that the workerthread know when to remove all not global actions
        protected bool workerThreadRemoveNotGlobalActions = false;

        /// <summary>
        /// The max amount the actions that get executed on the main thread block the main-thread
        /// This only ensures that after one action the execution is stopped till next frame. the action
        /// still needs to be as low latency as possible. There is no timeout or such
        /// </summary>
        private const int MAIN_THREAD_MAX_MS = 75;

        private class ConcurrentWorker {
            public ConcurrentQueue<AsyncAction> actions = new ConcurrentQueue<AsyncAction>();
        }

        private class Worker {
            public Queue<AsyncAction> actions = new Queue<AsyncAction>();
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



        /// <summary>
        /// Add an action and its callback
        /// </summary>
        /// <param name="act"></param>
        /// <param name="onFinished"></param>
        public override AsyncFuture AddToWorkerThread(Action act,Action onFinished,bool global=false) {
            if (workerThreadRemoveNotGlobalActions && global == false) {
                Debug.LogWarning("AsyncManager doesn't accept non global actions atm. Skipping");
                return new AsyncFuture() { finished = true };
            }

            var result = new AsyncFuture() { finished = false };
            workerWorkerThread.actions.Enqueue(new AsyncAction() { action=act,global=global });
            workerWorkerThread.actions.Enqueue(new AsyncAction() { action = () => { result.finished = true; }, global = global });
            if (onFinished != null) {
                workerWorkerThread.actions.Enqueue(new AsyncAction() { action = onFinished, global = global });
            }
            if (disposableWorker == null) {
                disposableWorker = Observable.Start(ThreadWorkerAction).ObserveOnMainThread().Subscribe(_=> {
                    disposableWorker.Dispose();
                    disposableWorker =null;
                });
            }
            return result;
        }

        private void ThreadWorkerAction() {
            while (workerWorkerThread.actions.Count > 0) {
                if (workerThreadRemoveNotGlobalActions) {
                    // remove all not global actions;
                    workerWorkerThread.actions = new ConcurrentQueue<AsyncAction>(workerWorkerThread.actions.Where(aAct => aAct.global == true).ToList());
                    workerThreadRemoveNotGlobalActions = false;
                }
                AsyncAction act;
                workerWorkerThread.actions.TryDequeue(out act);
                if (act != null) {
                    try {
                        act.action();
                    }
                    catch (Exception e) {
                        Debug.LogError("Catched AsyncManager.Call(worker-thread)-Exception:" + e + "\n" + e.StackTrace);
                    }
                } else {
                    // busy wait
                }
            }
        }

        /// <summary>
        /// Add an action to be executated queued making sure to not take more time than set in MAIN_THREAD_MAX_MS (more or less)
        /// </summary>
        /// <param name="act"></param>
        public override AsyncFuture AddToMainThread(Action act,bool global=false) {
            var result = new AsyncFuture() { finished = false };
            workerMainThread.actions.Enqueue(new AsyncAction() { action = act, global = global });
            workerMainThread.actions.Enqueue(new AsyncAction() { action=() => { result.finished = true; } , global = global });
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
                AsyncAction act = workerMainThread.actions.Dequeue() as AsyncAction;
                try {
                    act.action();
                }
                catch (Exception e) {
                    Debug.LogError("Catched AsyncManager.Call(MainThread-Queued)-Exception:" + e + "\n" + e.StackTrace);
                }

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
        public override AsyncFuture Call(Action act, bool usingCoroutine,bool global) {
            if (usingCoroutine) {
                // queue to async-queue
                return _asyncManager.AddToMainThread(act,global);
            } else {
                var result = new AsyncFuture() { finished = true };
                // immediately call
                try {
                    act();
                }
                catch (Exception e) {
                    Debug.LogError("Catched AsyncManager.Call(Mainthread-immediate)-Exception:" + e + "\n" + e.StackTrace);
                }
                return result;
            }
        }

        /// <summary>
        /// Dispose the current running threads
        /// </summary>
        public override void DisposeThreads(bool ignoreGlobals=false) {
            if (ignoreGlobals) {
                // we need to keep the threads running but only remove the not globals
                workerThreadRemoveNotGlobalActions = true;
                // rewrite workerMainThread and remove all not-globals
                workerMainThread.actions = new Queue<AsyncAction>(workerMainThread.actions.Where(aAct=>aAct.global==true).ToList());
                return;
            }
            if (disposableMainThreadWorker != null) {
                disposableMainThreadWorker.Dispose();
                disposableMainThreadWorker = null;
            }
            if (disposableWorker != null) {
                disposableWorker.Dispose();
                disposableWorker = null;
            }
            workerMainThread.actions.Clear();
            workerWorkerThread.actions = new ConcurrentQueue<AsyncAction>();
        }

        protected override void OnDispose() {
            // do your IDispose-actions here. It is called right after disposables got disposed
        }


    }
}
