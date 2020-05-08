using System;
using System.Collections;
using System.Threading;

namespace ParallelProcessing {
    public class ParallelProcessor : IDisposable /*where T : ISystemComponents*/ {
                /// <summary>
        /// Cached action to process for this system
        /// </summary>
        Action[] processActions;
        class ProcessActionData {
            public int startComponentIndex;
            public int endComponentIndex;
            public float deltaTime;
        }
        /// <summary>
        /// Data to process in processActions.
        /// </summary>
        ProcessActionData[] processActionData;


        public ParallelProcessor(Action<int, float> componentAction) {
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
                        ProcessActionData processData = processActionData[threadID];
                        for (int componentIndex = processData.startComponentIndex; componentIndex <= processData.endComponentIndex; ++componentIndex) {
                            componentAction(componentIndex, processData.deltaTime);
                        }
                    }
                    catch (Exception e) {
                        Console.WriteLine("Error in ParallelSystemComponentsProcessor: "+e.Message);
                        UnityEngine.Debug.LogException(e);
                    }
                    finally {
                        //Interlocked.Decrement(ref workingCount);
                        lock (ParallelProcessorWorkers._workingCountLocker) {
                            ParallelProcessorWorkers._workingCount--;
                            Monitor.Pulse(ParallelProcessorWorkers._workingCountLocker);
                        }
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
            foreach (Thread worker in ParallelProcessorWorkers.Workers)
                ParallelProcessorWorkers.EnqueueItem(null);

            // Wait for workers to finish
            if (waitForWorkers)
                foreach (Thread worker in ParallelProcessorWorkers.Workers)
                    worker.Join();
        }
        
        public void Process(ICollection componentsToProcess, float deltaTime) {
            Process(componentsToProcess.Count, deltaTime);
        }

        public void Process(int componentsToProcessCount, float deltaTime) {
            //Stop here if there are no components to process
            if (componentsToProcessCount == 0) return;

            //Make sure we are not using more threads than components
            int workersToUse = Math.Min(ParallelProcessorWorkers.MaxWorkerCount, componentsToProcessCount);

            Interlocked.Exchange(ref ParallelProcessorWorkers._workingCount, workersToUse);

            int componentChunk = (int)Math.Floor((float)componentsToProcessCount / (float)workersToUse);
            for (int i = 0; i < workersToUse; ++i) {
                int workerID = i;

                //Setup worker tasks
                int startComponentIndex = workerID * componentChunk;
                int endComponentIndex = 0;
                if (workerID != workersToUse - 1) {
                    endComponentIndex = (workerID * componentChunk) + componentChunk - 1;
                }
                //Work on last chunk. Last chunk has more elements than other chunks if (this.systemComponents.Count % this.maxThreads != 0)
                else {
                    endComponentIndex = componentsToProcessCount - 1;
                }
                
                //Set process action data
                processActionData[workerID].deltaTime = deltaTime;
                processActionData[workerID].startComponentIndex = startComponentIndex;
                processActionData[workerID].endComponentIndex = endComponentIndex;

                //Process action on worker
                ParallelProcessorWorkers.EnqueueItem(processActions[workerID]);
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