using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ParallelProcessing {
    /// <summary>
    /// Optimized parallel list to be used inside a ParallelProcessor. Supports lock-free adding.
    /// NOTE: IList<T> functions are ONLY supported inside ParallelProcessor.Process(..)!
    /// If possible, use GetListForX methods instead of .Add, .Remove etc. as it's much fast to fetch the list once and then use it.
    /// Once the processor is finished, you can access all list entries using GetAllLists and ProcessLists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ParallelProcessorAddList<T> : IList<T> {

        List<List<T>> threadLists;
        /// <summary>
        /// LUT for accessing a list based on the thread id.
        /// </summary>
        Dictionary<int, List<T>> threadListLUT;

        public ParallelProcessorAddList() {
            ParallelProcessorWorkers.Setup();

            threadLists = new List<List<T>>();
            threadListLUT = new Dictionary<int, List<T>>();
            foreach(Thread t in ParallelProcessorWorkers.Workers) {
                List<T> list = new List<T>();
                threadLists.Add(list);
                threadListLUT[t.ManagedThreadId] = list;
            }
        }

        /// <summary>
        /// Returns the list that is being used by the thread
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public List<T> GetListForThread(Thread t) {
            return GetListForThreadId(t.ManagedThreadId);
        }

        /// <summary>
        /// Returns the list that is being used by the thread
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<T> GetListForThreadId(int id) {
            return threadListLUT[id];
        }

        /// <summary>
        /// Returns the list that is being used by the current thread
        /// </summary>
        /// <returns></returns>
        public List<T> GetListForCurrentThread() {
            return threadListLUT[Thread.CurrentThread.ManagedThreadId];
        }

        /// <summary>
        /// Process an action on all lists. Call this AFTER ParallelProcessor.Process() is finished and the lists are no longer accessed by other threads.
        /// </summary>
        /// <param name="a"></param>
        public void ProcessLists(Action<T> a) {
            for(int l=0; l<threadLists.Count; ++l) {
                List<T> list = threadLists[l];
                int count = list.Count;
                for (int i = 0; i < count; ++i) {
                    a(list[i]);
                }
            }
        }

        /// <summary>
        /// Returns all thread lists. Call this AFTER ParallelProcessor.Process() is finished and the lists are no longer accessed by other threads.
        /// </summary>
        /// <returns></returns>
        public List<List<T>> GetAllLists() {
            return threadLists;
        }




        public T this[int index] {
            get {
                return threadListLUT[Thread.CurrentThread.ManagedThreadId][index];
            }
            set {
                threadListLUT[Thread.CurrentThread.ManagedThreadId][index] = value;
            }
        }

        public int Count {
            get {
                return threadListLUT[Thread.CurrentThread.ManagedThreadId].Count;
            }
        }

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(T item) {
            threadListLUT[Thread.CurrentThread.ManagedThreadId].Add(item);
        }

        public void Clear() {
            threadListLUT[Thread.CurrentThread.ManagedThreadId].Clear();
        }

        public bool Contains(T item) {
            return threadListLUT[Thread.CurrentThread.ManagedThreadId].Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            threadListLUT[Thread.CurrentThread.ManagedThreadId].CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator() {
            return threadListLUT[Thread.CurrentThread.ManagedThreadId].GetEnumerator();
        }

        public int IndexOf(T item) {
            return threadListLUT[Thread.CurrentThread.ManagedThreadId].IndexOf(item);
        }

        public void Insert(int index, T item) {
            threadListLUT[Thread.CurrentThread.ManagedThreadId].Insert(index, item);
        }

        public bool Remove(T item) {
            return threadListLUT[Thread.CurrentThread.ManagedThreadId].Remove(item);
        }

        public void RemoveAt(int index) {
            threadListLUT[Thread.CurrentThread.ManagedThreadId].RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return threadListLUT[Thread.CurrentThread.ManagedThreadId].GetEnumerator();
        }
    }
}