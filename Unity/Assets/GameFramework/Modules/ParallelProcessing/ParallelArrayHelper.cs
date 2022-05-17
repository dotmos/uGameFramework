using System.Collections.Generic;

namespace ParallelProcessing {
    public static class ParallelArrayHelper {
        public static T[] CreateArrayForParallelProcessing<T>() {
            return new T[ParallelProcessorWorkers.MaxWorkerCount];
        }

        public static T[] CreateArrayForParallelProcessing<T>(T initialValue) {
            T[] result = new T[ParallelProcessorWorkers.MaxWorkerCount];
            for (int i = 0; i < result.Length; ++i) {
                result[i] = initialValue;
            }
            return result;
        }

        public static List<T>[] CreateListArrayForParallelProcessing<T>(int initialSize = 0) {
            List<T>[] result = new List<T>[ParallelProcessorWorkers.MaxWorkerCount];
            for(int i=0; i<result.Length; ++i) {
                result[i] = new List<T>(initialSize);
            }
            return result;
        }

        /// <summary>
        /// Clears all lists of the array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listArray"></param>
        public static void ClearLists<T>(List<T>[] listArray) {
            for (int i = 0; i < listArray.Length; ++i) {
                listArray[i].Clear();
            }
        }

        /// <summary>
        /// Set all values of the array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="value"></param>
        public static void SetAllValues<T>(T[] array, T value) {
            for (int i = 0; i < array.Length; ++i) {
                array[i] = value;
            }
        }

        /// <summary>
        /// Add all items of the lists inside the array to the output list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listArray"></param>
        /// <param name="outputList"></param>
        public static void MergeLists<T>(List<T>[] listArray, List<T> outputList) {
            for (int i = 0; i < listArray.Length; ++i) {
                outputList.AddRange(listArray[i]);
            }
        }
    }
}