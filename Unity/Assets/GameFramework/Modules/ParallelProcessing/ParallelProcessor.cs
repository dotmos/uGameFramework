#define METHOD3

using System;
using System.Collections;
using System.Threading;
using UnityEngine.UI;

namespace ParallelProcessing {
    public class ParallelProcessor : IDisposable /*where T : ISystemComponents*/ {
                /// <summary>
        /// Cached action to process for this system
        /// </summary>
        Action[] processActions;
        class ProcessActionData {
            public int startIndex;
            public int endIndex;
            public int indexIncrease = 1;

            public float deltaTime;
        }
        /// <summary>
        /// Data to process in processActions.
        /// </summary>
        ProcessActionData[] processActionData;

        /// <summary>
        /// The current index that a thread will use 
        /// </summary>
        int currentItemIndex;
        /// <summary>
        /// The total item count. If currentItemIndex >= itemCount, all work has been processed
        /// </summary>
        int totalItemCount;
        int currentFrameStartIdx;

        int chunkSize;
        int currentChunkIndex;

        public ParallelProcessor(Action<int, float, int> componentAction) {
            ParallelProcessorWorkers.Setup();
            ParallelProcessorWorkers._processorInstanceCount++;

            processActionData = new ProcessActionData[ParallelProcessorWorkers.MaxWorkerCount];
            processActions = new Action[ParallelProcessorWorkers.MaxWorkerCount];
            for (int i = 0; i < ParallelProcessorWorkers.MaxWorkerCount; ++i) {
                processActionData[i] = new ProcessActionData();
                int threadID = i;
                processActions[i] = () => {
                    //Thread.MemoryBarrier();
                    try {
                        UnityEngine.Profiling.Profiler.BeginSample("ECSTask");
                        
                        ProcessActionData processData = processActionData[threadID];
                        //Work on linear range of indices. Problem: Might get indices that are very heavy to compute, while others threads are idling
                        //~10ms
                        /*
                        for (int componentIndex = processData.startIndex; componentIndex < processData.endIndex; ++componentIndex) {
                            componentAction(componentIndex, processData.deltaTime);
                        }
                        */
                        
                        //Work on range of indices, using static offset: Might geht indices that are very heavy to compute, but less likely than the above version
                        //~11ms
                        /*
                        for (int index = processData.startIndex; index < processData.endIndex; index += processData.indexIncrease) {
                            componentAction(index, processData.deltaTime);
                        }
                        */
                        

                        //Get next index and work on it. Problem: Locking
                        //~14ms
                        /*
                        int _index;
                        //while(currentItemIndex < itemCount) {
                        // Update: Not using a while loop, as it might stall the app if something bad happens inside the loop
                        for(int dummy=0; dummy < totalItemCount; ++dummy) {
                            _index = Interlocked.Increment(ref currentItemIndex)-1;
                            if(_index < totalItemCount) {
                                componentAction(_index, processData.deltaTime);
                            } else {
                                break;
                            }
                        }
                        */
                        
                        for(int dummy=0; dummy< totalItemCount; ++dummy) {
                            int _chunkIndex = Interlocked.Increment(ref currentChunkIndex)-1;
                            int startIndex = _chunkIndex * chunkSize;
                            if (startIndex < totalItemCount) {
                                int endIndex = Math.Min(startIndex + chunkSize, totalItemCount) + currentFrameStartIdx;
                                for (int componentIndex = startIndex + currentFrameStartIdx; componentIndex < endIndex; ++componentIndex) {
                                    componentAction(componentIndex, processData.deltaTime, threadID);
                                }
                            } else {
                                break;
                            }
                        }
                        
                    }
                    catch (Exception e) {
                        Console.WriteLine("Error in ParallelSystemComponentsProcessor: "+e.Message);
                        UnityEngine.Debug.LogException(e);
                    }
                    finally {
                        UnityEngine.Profiling.Profiler.BeginSample("SemaphorWait");

                        //Interlocked.Decrement(ref workingCount);
                        lock (ParallelProcessorWorkers._workingCountLocker) {
                            ParallelProcessorWorkers._workingCount--;
                            Monitor.Pulse(ParallelProcessorWorkers._workingCountLocker);
                        }
                        
                        UnityEngine.Profiling.Profiler.EndSample();
                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                };
            }


        }



        /// <summary>
        /// Stop processing
        /// </summary>
        /// <param name="waitForWorkers"></param>
        void Shutdown(bool waitForWorkers) {
            // Enqueue one null item per worker to make each exit.
            foreach (ParallelProcessorWorkers.Worker worker in ParallelProcessorWorkers.Workers) {
                ParallelProcessorWorkers.EnqueueItem(worker.workerID, null);
            }
                

            // Wait for workers to finish
            if (waitForWorkers) {
                foreach (ParallelProcessorWorkers.Worker worker in ParallelProcessorWorkers.Workers) {
                    worker.Join();
                }
            }
                
        }
        
        public void Process(ICollection componentsToProcess, float deltaTime, int maxChunkSize = 9999999) {
            Process(componentsToProcess.Count, deltaTime, maxChunkSize);
        }

        public void Process(int componentsToProcessCount, float deltaTime, int maxChunkSize = 9999999, int startIDX = 0) {
            //Stop here if there are no components to process
            if (componentsToProcessCount == 0) return;

            //Make sure we are not using more threads than components
            int workersToUse = Math.Min(ParallelProcessorWorkers.MaxWorkerCount, componentsToProcessCount);
            //Make sure chunk size is small enough so all threads have work to do
            maxChunkSize = Math.Max(maxChunkSize, 1);
            maxChunkSize = Math.Min(maxChunkSize, (int)((float)componentsToProcessCount / workersToUse));

            Interlocked.Exchange(ref ParallelProcessorWorkers._workingCount, workersToUse);

            int componentChunk = (int)Math.Floor((float)componentsToProcessCount / (float)workersToUse);

            totalItemCount = componentsToProcessCount;
            currentItemIndex = 0;
            currentChunkIndex = 0;
            chunkSize = maxChunkSize;

            for (int i = 0; i < workersToUse; ++i) {
                int workerID = i;

                /*
                //Method 1: One chunk per thread
                //Setup worker tasks
                int startComponentIndex = workerID * componentChunk;
                int endComponentIndex = 0;
                if (workerID != workersToUse - 1) {
                    endComponentIndex = (workerID * componentChunk) + componentChunk;
                }
                //Work on last chunk. Last chunk has more elements than other chunks if (this.systemComponents.Count % this.maxThreads != 0)
                else {
                    endComponentIndex = componentsToProcessCount;
                }
                
                //Set process action data
                processActionData[workerID].deltaTime = deltaTime;
                processActionData[workerID].startIndex = startComponentIndex;
                processActionData[workerID].endIndex = endComponentIndex;
                */


                /*
                //Method 2: Non-locking linear index with static offset
                processActionData[workerID].deltaTime = deltaTime;
                processActionData[workerID].startIndex = workerID;
                processActionData[workerID].endIndex = componentsToProcessCount;
                processActionData[workerID].indexIncrease = workersToUse;
                */

#if METHOD3
                //Method 3: Get next linear index. Locking.
                currentFrameStartIdx = startIDX;
                processActionData[workerID].deltaTime = deltaTime;
#endif

                //Process action on worker
                ParallelProcessorWorkers.EnqueueItem(workerID, processActions[workerID]);
            }

            /*
            //Wait for queue to finish
            long _workingCountCheck = Interlocked.Read(ref workingCount);
            while(_workingCountCheck > 0) {
                _workingCountCheck = Interlocked.Read(ref workingCount);
            }
            */

            //Wait for queue to finish
            lock (ParallelProcessorWorkers._workingCountLocker) {
                while (ParallelProcessorWorkers._workingCount > 0) Monitor.Wait(ParallelProcessorWorkers._workingCountLocker);
            }
        }

        public void Dispose() {
            ParallelProcessorWorkers._processorInstanceCount--;
            if(ParallelProcessorWorkers._processorInstanceCount <= 0) {
                Shutdown(true);
            }
        }
    }
}