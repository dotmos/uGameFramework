using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ParallelProcessing {
    public class ParallelProcessorVariable<T> {
        /// <summary>
        /// LUT for accessing a value based on the thread id.
        /// </summary>
        Dictionary<int, T> threadVariableLUT;

        /// <summary>
        /// Cached thread IDs for garbage free LUT iterating
        /// </summary>
        List<int> cachedThreadIDs = new List<int>();

        /// <summary>
        /// If otherThreads is supplied, otherThreads will also get values in addition to the ParallelProcessorWorker threads
        /// </summary>
        /// <param name="mainThread"></param>
        public ParallelProcessorVariable(Thread[] otherThreads = null) : this(() => { return default; }, otherThreads) { }

        public ParallelProcessorVariable(Func<T> initialValue, Thread[] otherThreads = null) {
            ParallelProcessorWorkers.Setup();

            threadVariableLUT = new Dictionary<int, T>(ParallelProcessorWorkers.MaxWorkerCount);
            
            //Create worker thread lists
            foreach (ParallelProcessorWorkers.Worker worker in ParallelProcessorWorkers.Workers) {
                threadVariableLUT[worker.thread.ManagedThreadId] = initialValue();
                cachedThreadIDs.Add(worker.thread.ManagedThreadId);
            }
            //Also create lists for other threads?
            if (otherThreads != null) {
                foreach (Thread t in otherThreads) {
                    threadVariableLUT[t.ManagedThreadId] = initialValue();
                    cachedThreadIDs.Add(t.ManagedThreadId);
                }
            }
        }

        /// <summary>
        /// Returns the value that is being used by the thread
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public T GetValueForThread(Thread t) {
            return GetValueForThreadId(t.ManagedThreadId);
        }

        /// <summary>
        /// Returns the value that is being used by the thread
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetValueForThreadId(int id) {
            return threadVariableLUT[id];
        }

        /// <summary>
        /// Returns the value that is being used by the current thread
        /// </summary>
        /// <returns></returns>
        public T GetValueForCurrentThread() {
            return threadVariableLUT[Thread.CurrentThread.ManagedThreadId];
        }

        /// <summary>
        /// The value that is being used by the current thread
        /// </summary>
        public T Value {
            get {
                return GetValueForCurrentThread();
            }
            set {
                SetValueForCurrentThread(value);
            }
        }

        /// <summary>
        /// Set the value for the current thread
        /// </summary>
        /// <param name="value"></param>
        public void SetValueForCurrentThread(T value) {
            threadVariableLUT[Thread.CurrentThread.ManagedThreadId] = value;
        }

        /// <summary>
        /// Returns all values
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, T>.ValueCollection GetAllValues() {
            return threadVariableLUT.Values;
        }

        /// <summary>
        /// Do an action on all values
        /// </summary>
        /// <param name="valueAction"></param>
        public void OperateOnValues(Action<T> valueAction) {
            for(int i=0; i<cachedThreadIDs.Count; ++i) {
                valueAction(threadVariableLUT[cachedThreadIDs[i]]);
            }
        }
        

        /// <summary>
        /// Clears all values to their default value
        /// </summary>
        public void DefaultAllValues() {
            SetAllValues(default);
        }

        /// <summary>
        /// Set all values
        /// </summary>
        /// <param name="value"></param>
        public void SetAllValues(T value) {
            for(int i=0; i<cachedThreadIDs.Count; ++i){
                threadVariableLUT[cachedThreadIDs[i]] = value;
            }
        }        
    }
}