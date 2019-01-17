using System;
using System.Collections.Generic;
using System.Threading;

namespace ECS {
    /// <summary>
    /// Helper class to process ECS system components in parallel without the GC allocs of Parallel.For/ForEach. Also, it seems as if this custom implementation is 80-100% faster than Parallel.For/Foreach for some reason.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ParallelSystemComponentsProcessor<T> : IDisposable where T : ISystemComponents {
        int maxThreads = 0;
        ThreadStart[] threads;
        int _currentThreads = 0;
        object lock_currentThreads = new object();
        int CurrentThreads {
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
        float DeltaTime;

        public ParallelSystemComponentsProcessor(int degreeOfParalelism = 0) {
            if (degreeOfParalelism == 0) maxThreads = Environment.ProcessorCount;
            else maxThreads = degreeOfParalelism;
        }

        public void Setup(Action<int, float> componentAction, List<T> systemComponents) {
            threads = new ThreadStart[maxThreads];
            for (int t = 0; t < maxThreads; ++t) {
                int threadID = t;
                threads[t] = new ThreadStart(() => {
                    int componentChunk = systemComponents.Count / maxThreads;
                    for (int i = threadID * componentChunk; i < (threadID * componentChunk) + componentChunk; ++i) {
                        componentAction(i, DeltaTime);
                    }
                    CurrentThreads--;
                });
            }
        }

        public void Invoke(float deltaTime) {
            CurrentThreads = maxThreads;
            DeltaTime = deltaTime;
            for (int i = 0; i < maxThreads; ++i) {
                threads[i].Invoke();
            }
            while (CurrentThreads > 0) { }
        }

        public void Dispose() {
            for (int i = 0; i < threads.Length; ++i) {
                threads[i].EndInvoke(null);
                threads[i] = null;
            }
            threads = null;
        }
    }
}