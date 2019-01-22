using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECS {
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