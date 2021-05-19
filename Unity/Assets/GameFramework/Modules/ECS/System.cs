using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Zenject;
using UniRx;
using ParallelProcessing;
using UnityEngine;

namespace ECS {
#if TESTING
    public class TestingCallback_System
    {
        public enum CallbackState { start, end };
        public ISystem system;
        public CallbackState state; 
        public object componentsToProcess; // rethink this
    }
#endif

    public abstract class System<TComponents> : ISystem where TComponents : ISystemComponents, new(){

        public IEntityManager entityManager { get; private set; }

        Service.Events.IEventsService eventService;

        /// <summary>
        /// 
        /// </summary>
        /// 
#if ECS_PROFILING && UNITY_EDITOR
        private readonly System.Diagnostics.Stopwatch watchService = new System.Diagnostics.Stopwatch();
        private double maxElapsedTime = 0;
        private int mediumTicks = 0;
        private int highTicks = 0;
        private int veryhighTicks = 0;
        StringBuilder logTxtBuilder = new StringBuilder();
        private string systemType = "";
#endif

#if TESTING
        private TestingCallback_System tempStart = new TestingCallback_System() { 
            state = TestingCallback_System.CallbackState.start
        };

        private TestingCallback_System tempEnd = new TestingCallback_System() { 
            state = TestingCallback_System.CallbackState.end
        };

        private Service.Scripting.IScriptingService scriptingService;

        protected Service.Scripting.IScriptingService ScriptingService { 
            get { 
                if (scriptingService == null) {
                    scriptingService = Kernel.Instance.Resolve<Service.Scripting.IScriptingService>();
                }
                return scriptingService;
            }
        }
#endif

        //protected List<UID> validEntities;
        protected HashSet<UID> validEntities;
        protected TComponents[] componentsToProcess; //Was hashset in the past, but hashsets were super slow when multithreading systems. Testcase showed 28ms (hashset) vs 14ms (list)!
        int componentCount;
        /// <summary>
        /// The total count of entities/systemComponents that are being processed by the system
        /// </summary>
        protected int SystemComponentCount { get { return componentCount; } }

        protected HashSet<TComponents> pendingRemovalComponentsCheck;
        protected List<TComponents> pendingRemovalComponents; //a list of components that needs to be removed after the cycle
        /// <summary>
        /// LUT for quick TComponent access
        /// </summary>
        Dictionary<int, TComponents> componentsToProcessLUT;


        /// <summary>
        /// Cyclic execution distributes execution on multiple frames
        /// </summary>
        protected bool useCyclicExecution = false;

        /// <summary>
        /// Data about the current execution cycle (if enabled)
        /// </summary>
        protected CyclicExecutionData cyclicExecutionData = null;

        /// <summary>
        /// Temporarily store newly registered Component(packs) here, when a new entity got valid
        /// </summary>
        protected List<TComponents> newComponents;
        /// <summary>
        /// Temporarily store entities that got invalid
        /// </summary>
        protected List<TComponents> removedComponents;

        /// <summary>
        /// Temporarily store updated components here and call EntityUpdated when the system is Ticked again.
        /// </summary>
        protected List<TComponents> updatedComponents;
        /// <summary>
        /// Lookuptable for fast check if this component is already added to updated components-list
        /// </summary>
        protected HashSet<TComponents> updatedComponentsLUT;

        /// <summary>
        /// The delta time, that will be used for the next legit ProcessAll call. Once this value reaches a value that is higher or equal to SystemUpdateRate(), ProcessAll() call is valid.
        /// </summary>
        float currentUpdateDeltaTime = 0;
        float currentTimer = 0;

        protected ParallelProcessor parallelSystemComponentProcessor;

        string _systemName = null;
        string SystemName {
            get {
                if (_systemName == null) _systemName = this.GetType().ToString();
                return _systemName;
            }
        }
#if UNITY_EDITOR
        UnityEngine.Profiling.CustomSampler sampler;
#endif

        public System() : this(null) {

        }

        public System(IEntityManager entityManager) {
            validEntities = new HashSet<UID>();
            componentsToProcess = new TComponents[2048];
            componentsToProcessLUT = new Dictionary<int, TComponents>();
            pendingRemovalComponents = new List<TComponents>();
            pendingRemovalComponentsCheck = new HashSet<TComponents>();

            newComponents = new List<TComponents>();
            removedComponents = new List<TComponents>();
            updatedComponents = new List<TComponents>();
            updatedComponentsLUT = new HashSet<TComponents>();

            SetEntityManager(entityManager);

            PreBind();

            Kernel.Instance.Inject(this);

#if ECS_PROFILING && UNITY_EDITOR
            systemType = GetType().ToString();
#endif
            AfterBind();
        }

        [Inject]
        void OnInject(
            [Inject] Service.Events.IEventsService eventService
            ) {
            this.eventService = eventService;
        }
        
        protected virtual void PreBind() {

        }

        protected virtual void AfterBind() {
            if (UseParallelSystemComponentsProcessing()) {
#if UNITY_EDITOR
                sampler = UnityEngine.Profiling.CustomSampler.Create(SystemName);
#endif
                parallelSystemComponentProcessor = new ParallelProcessor(ProcessAtIndex);
            }
        }

        /// <summary>
        /// Publish the specified global event.
        /// </summary>
        /// <param name="evt">Evt.</param>
        protected void Publish(object evt) {
            eventService.Publish(evt);
        }
        protected void Publish(object evt, Subject<object> eventStream) {
            eventService.Publish(evt, eventStream);
        }

        /// <summary>
        /// Subscribe to a global event of type TEvent
        /// </summary>
        /// <typeparam name="TEvent">The 1st type parameter.</typeparam>
        protected IObservable<TEvent> OnEvent<TEvent>() {
            return eventService.OnEvent<TEvent>();
        }
        protected IObservable<TEvent> OnEvent<TEvent>(Subject<object> eventStream) {
            return eventService.OnEvent<TEvent>(eventStream);
        }


        /// <summary>
        /// initialize newly registered entities and their components
        /// </summary>
        /// <param name="registeredComponents"></param>
        protected virtual void OnRegistered(List<TComponents> newRegisteredComponents) {
        }

        /// <summary>
        /// handle entities removed from the system
        /// </summary>
        /// <param name="unregisteredEntities"></param>
        protected virtual void OnUnregistered(List<TComponents> unregisteredEntities) {
        }

        /// <summary>
        /// The updaterate to use for ProcessAll(). 0 updates every frame. 0.1f updates 10 times per second. 0.5f updates 2 times per second.
        /// </summary>
        /// <returns></returns>
        protected virtual float SystemUpdateRate() {
            return 0; 
        }


        /// <summary>
        /// Calls the system according to at fixed realtime(unscaled) rate like specified in SystemUpdateRate() or scales with the delta-time
        /// </summary>
        /// <returns></returns>
        protected virtual bool SystemFixedRate() {
            return true;
        }


        protected abstract bool UseParallelSystemComponentsProcessing();

        /// <summary>
        /// Should the system be force ticked each frame if deltaTime == 0 and SystemUpdateRate > 0 ?
        /// If force tick is enabled, ProcessAll/ProcessAtIndex will receive a deltaTime == 0 on tick.
        /// Default is false.
        /// </summary>
        /// <returns></returns>
        protected virtual bool ForceTickOnDeltaZero() {
            return false;
        }

        protected void EnableCyclicProcess(int amountWorkers, int minElementAmounts) {
            useCyclicExecution = true;
            cyclicExecutionData = new CyclicExecutionData(amountWorkers,minElementAmounts);
        }

        /// <summary>
        /// The maximum chunk size per thread when using parallel processing
        /// </summary>
        /// <returns></returns>
        protected virtual int MaxParallelChunkSize() {
            return 9999999;
        }

        /// <summary>
        /// Process all entities. Will add deltaTime to an internal counter and then update entities based on SystemUpdateRate()
        /// </summary>
        protected virtual void ProcessAll(float deltaTime) {

           
            if (UseParallelSystemComponentsProcessing()) {
                /*
#if UNITY_EDITOR
                UnityEngine.Profiling.Profiler.BeginThreadProfiling("Parallel System Components Processor", SystemName);
                sampler.Begin();
#endif
                */

                if (useCyclicExecution) {
                    float cyclicDt = 0;
                    if (cyclicExecutionData.cyclicExecutionFinished) {
                        if (componentCount < cyclicExecutionData.cyclicExecutionMinAmount) {
                            // too less elements for the cycle to be used => default way
                            parallelSystemComponentProcessor.Process(componentCount, deltaTime,MaxParallelChunkSize());
                            return; // finish here
                        } else {
                            // start a new cycle and get the deltaTime for the current cycle-frame
                            cyclicDt = cyclicExecutionData.SetStartCycleData(componentCount, deltaTime);
                        }
                    } else {
                        cyclicDt = cyclicExecutionData.NextCycleData(deltaTime);
                    }
                    if (cyclicExecutionData.currentCycleFrame == 0) {
                        CycleBeforeFirstCycle();
                    }
                    parallelSystemComponentProcessor.Process(cyclicExecutionData.nextAmountElements, cyclicDt, MaxParallelChunkSize(), cyclicExecutionData.nextStartElement);
                    CycleFrameFinished(cyclicExecutionData.currentCycleFrame);
                    if (cyclicExecutionData.cyclicExecutionFinished) {
                        // now is the time to apply pending removals
                        int pendingRemovalAmount = pendingRemovalComponents.Count;

                        if (pendingRemovalAmount > 0) {
                            for (int i = -1; i >= 0; i--) {
                                var _removeComps = pendingRemovalComponents[i];
                                for (int idx = 0; idx < componentCount; ++idx) {
                                    if (componentsToProcess[idx].Entity.ID == _removeComps.Entity.ID) {
                                        componentsToProcess[idx] = componentsToProcess[componentCount - 1]; //Put last item in array to position of item that should be deleted, overwriting (and therefore deleting) it
                                    }
                                }
                                componentsToProcessLUT.Remove(_removeComps.Entity.ID);
                            }
                            pendingRemovalComponents.Clear();
                            pendingRemovalComponentsCheck.Clear();
                        }
                        CycleCompleted();
                    }


                } else {
                    // default way
                    parallelSystemComponentProcessor.Process(componentCount, deltaTime,MaxParallelChunkSize());
                }

                /*
                //Workaround for broken parallelSystemComponentProcessor. Produces garbage.
                int degreeOfParallelism = Environment.ProcessorCount;
                System.Threading.Tasks.ParallelLoopResult result = System.Threading.Tasks.Parallel.For(0, degreeOfParallelism, workerId =>
                {
                    var max = componentsToProcess.Count * (workerId + 1) / degreeOfParallelism;
                    for (int i = componentsToProcess.Count * workerId / degreeOfParallelism; i < max; i++)
                        //array[i] = array[i] * factor;
                        ProcessAtIndex(i, currentUpdateDeltaTime);
                });

                while (!result.IsCompleted) { }
                */

                /*
#if UNITY_EDITOR
                sampler.End();
                UnityEngine.Profiling.Profiler.EndThreadProfiling();
#endif
                */
            }
            else {
                //int _count = componentsToProcess.Count;
                for (int i = 0; i < componentCount; ++i) {
                    ProcessAtIndex(i, deltaTime);
                }
            }      
        }

        protected abstract void ProcessAtIndex(int componentIndex, float deltaTime);



        public void ProcessSystem(float deltaTime,float unscaled, float systemScaled) {
#if TESTING
            //tempStart.system = this;
            //tempStart.componentsToProcess = componentsToProcess;
            //ScriptingService.Callback("system", GetType(), tempStart);
#endif
            //Tell system there are new components
            if (newComponents.Count > 0) {
                OnRegistered(newComponents);
                int _count = newComponents.Count;
                for(int i=0; i< _count; ++i) {
                    TComponents components = newComponents[i];
                    EntityUpdated(ref components);
                }
                newComponents.Clear();
            }
            //Tell system components were updated
            if(updatedComponents.Count > 0) {
                int _count = updatedComponents.Count;
                for(int i=0; i< _count; ++i) {
                    TComponents components = updatedComponents[i];
                    EntityUpdated(ref components);
                }
                updatedComponents.Clear();
                updatedComponentsLUT.Clear();
            }
            //Tell system that components were removed
            if (removedComponents.Count > 0)
            {
                OnUnregistered(removedComponents);
                removedComponents.Clear();
            }
            try
            {
#if ECS_PROFILING && UNITY_EDITOR
                watchService.Restart();
#endif

#if SYSTEMS_LEGACY
                currentTimer += deltaTime;
#else
                currentTimer += SystemFixedRate() ? unscaled : systemScaled;
#endif
                currentUpdateDeltaTime += deltaTime;
                //Process system components
                if (currentTimer >= SystemUpdateRate()) {
                    //Regular tick
                    ProcessAll(currentUpdateDeltaTime);
                    currentUpdateDeltaTime = 0;
                    currentTimer = 0;
                } else if(deltaTime == 0 && ForceTickOnDeltaZero()) {
                    //Force tick with deltaTime 0
                    ProcessAll(0);
                }
#if ECS_PROFILING && UNITY_EDITOR
                watchService.Stop();
                var elapsedTime = watchService.Elapsed.TotalSeconds;
                if (elapsedTime > maxElapsedTime) {
                    maxElapsedTime = elapsedTime;
                }
                if (elapsedTime > 1) {
                    veryhighTicks++;
                } 
                else if (elapsedTime > 0.1) {
                    highTicks++;
                } else if (elapsedTime > 0.016666){
                    mediumTicks++;
                }
                if (EntityManager.showLog) {
                    logTxtBuilder.Clear();
                    logTxtBuilder.Append(elapsedTime).Append("(max:").Append(maxElapsedTime).Append(" [>0.0166:").Append(mediumTicks).Append("|>0.1:").Append(highTicks)
                        .Append("|>1.0:").Append(veryhighTicks).Append("] System:").Append(systemType);
                    UnityEngine.Debug.Log(logTxtBuilder.ToString());
                };

#endif
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("There was a problem in process-all in system:" + GetType());
                UnityEngine.Debug.LogException(e);
            }

#if TESTING
            //tempEnd.system = this;
            //tempEnd.componentsToProcess = componentsToProcess;
            //ScriptingService.Callback("system", GetType(), tempEnd);
#endif

        }



        /// <summary>
        /// Sets the entity manager of this system
        /// </summary>
        /// <param name="manager"></param>
        public void SetEntityManager(IEntityManager manager) {
            entityManager = manager;
        }

        protected T GetComponent<T>(UID entity) where T : IComponent{
            return entityManager.GetComponent<T>(entity);
        }

        protected bool HasComponent<T>(UID entity) where T : IComponent {
            return entityManager.HasComponent<T>(entity);
        }

        /// <summary>
        /// Call whenever an entity is modified. Entity might no longer be valid for this system.
        /// </summary>
        /// <param name="entity"></param>
        public virtual void EntityModified(UID entity) {
            bool valid = IsEntityValid(entity);
            bool wasValid = validEntities.Contains(entity);
            //if(validEntities.Find(v => v.ID == entity.ID).ID > 0) {
            //    wasValid = true;
            //}

            //UnityEngine.Debug.Log(entity.ID + "valid: "+valid);

            if(valid && wasValid) {
                UpdateEntity(entity);
            }
            else if (valid && !wasValid) {
                RegisterEntity(entity);
            }
            else if (!valid && wasValid) {
                UnregisterEntity(entity);
            }
        }

        /// <summary>
        /// getting called just before the first cycle-frame
        /// </summary>
        protected virtual void CycleBeforeFirstCycle() { }
        
        /// <summary>
        /// Called after another batch is finished
        /// </summary>
        /// <param name="cycleFrame"></param>
        protected virtual void CycleFrameFinished(int cycleFrame) { 
        }
        /// <summary>
        /// getting called just after the cycle finished with pendingRemoval-Entities already applied
        /// </summary>
        protected virtual void CycleCompleted() { }

        /// <summary>
        /// Get components for entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected TComponents GetSystemComponentsForEntity(UID entity) {
            //TComponents components = componentsToProcess.Find(o => o.Entity == entity);
            TComponents components;
            if(componentsToProcessLUT.TryGetValue(entity.ID, out components)) {
                return components;
            } else {
                return default(TComponents);
            }
        }


        void UpdateEntity(UID entity) {
            TComponents entityComponents = GetSystemComponentsForEntity(entity);
            GetEntityComponents(entityComponents, entity);

            if (entityComponents!=null && !updatedComponentsLUT.Contains(entityComponents)) {
                updatedComponents.Add(entityComponents);
                updatedComponentsLUT.Add(entityComponents);
            }
        }

        /// <summary>
        /// Called when a valid entity was updated
        /// </summary>
        /// <param name="components"></param>
        protected virtual void EntityUpdated(ref TComponents components) { }

        void RegisterEntity(UID entity) {
            //UnityEngine.Debug.Log(entity.ID + "valid! Adding to system!");
            validEntities.Add(entity);
            //Add components to process
            TComponents components = _CreateSystemComponentsForEntity(entity);
            if(componentCount == componentsToProcess.Length) {
                //TComponents[] newComponentsToProcess = new TComponents[componentsToProcess.Length * 2];
                //componentsToProcess.CopyTo(newComponentsToProcess, 0);
                Array.Resize(ref componentsToProcess, componentsToProcess.Length * 2);
            }
            componentsToProcess[componentCount]=components;
            componentCount++;
            componentsToProcessLUT.Add(entity.ID, components);
            newComponents.Add(components);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        void UnregisterEntity(UID entity) {
            //UnityEngine.Debug.Log(entity.ID + " invalid! Removing from system!");
            //Remove components to process
            int _entityID = entity.ID;

            //TODO: Find a faster way to remove the components.
            TComponents components = GetSystemComponentsForEntity(entity);
            if (components != null) {
                if (!useCyclicExecution) {
                    for(int i=0; i< componentCount; ++i) {
                        if(componentsToProcess[i].Entity.ID == components.Entity.ID) {
                            componentsToProcess[i] = componentsToProcess[componentCount - 1]; //Put last item in array to position of item that should be deleted, overwriting (and therefore deleting) it
                        }
                    }
                    //componentsToProcess.Remove(components);
                    componentCount--;
                    componentsToProcessLUT.Remove(components.Entity.ID);
                } else {
                    pendingRemovalComponents.Add(components);
                }
            }

            //componentsToProcess.RemoveWhere(v => v.Entity.ID == _entityID);
            validEntities.Remove(entity);

            removedComponents.Add(components);
        }

        /// <summary>
        /// Checks if the entity can be used by the system
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected abstract bool IsEntityValid(UID entity);

        private TComponents _CreateSystemComponentsForEntity(UID entity) {
            TComponents tc = new TComponents();
            tc.Entity = entity;
            return GetEntityComponents(tc, entity);
        }

        /// <summary>
        /// Caches all needed components from the entity
        /// </summary>
        /// <param name="components"></param>
        /// <returns></returns>
        protected abstract TComponents GetEntityComponents(TComponents components, UID entity);
 
        /// <summary>
        /// Remove all entities from the system
        /// </summary>
        public virtual void RemoveAllEntities() {
            List<TComponents> _allComponents = new List<TComponents>(componentsToProcess);
            OnUnregistered(_allComponents);
            _allComponents.Clear();
            
            validEntities.Clear();
            //componentsToProcess.Clear();
            componentsToProcess = new TComponents[2048];
            componentCount = 0;
            componentsToProcessLUT.Clear();
        }

        public virtual void Dispose() {
            if(parallelSystemComponentProcessor != null) parallelSystemComponentProcessor.Dispose();

            entityManager = null;

            validEntities.Clear();
            //componentsToProcess.Clear();
            componentsToProcess = new TComponents[1];
            componentCount = 0;
            componentsToProcessLUT.Clear();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("System("+this.GetType().Name+") disposed");
#endif
        }

        protected class CyclicExecutionData {
            /// <summary>
            /// Is a cyclic execution running
            /// </summary>
            public bool cyclicExecutionFinished = true;

            /// <summary>
            /// Minium amount of elements for cyclic execution to take place
            /// </summary>
            public int cyclicExecutionMinAmount = 1000;

            /// <summary>
            ///  amount of frames to distribute the complete element amount on
            /// </summary>
            public int amountFrames = 0;


            /// <summary>
            /// Each cycle needs to maintain its deltatime until it is triggered next
            /// </summary>
            float[] deltaTimesPerCycle;

            /// <summary>
            /// Amount of elements to process by the cycle 
            /// </summary>
            public int processElementsPerCycle = 0;
            /// <summary>
            /// current cycle frame
            /// </summary>
            public int currentCycleFrame = 0;
            /// <summary>
            /// amount elements left
            /// </summary>
            public int amountElements = 0;
            /// <summary>
            /// start elementIdx for the next cycle-frame
            /// </summary>
            public int nextStartElement = 0;
            /// <summary>
            /// amount of elements to be processed next cycle frame
            /// </summary>
            public int nextAmountElements = 0;

            public CyclicExecutionData(int amountWorkers,int cyclicExecutionMinAmount=1000) {
                this.amountFrames = amountWorkers;
                this.cyclicExecutionMinAmount = 1000;
                deltaTimesPerCycle = new float[amountWorkers];
            }


            /// <summary>
            /// Start a cycle by specifiying the amount of element in the system
            /// </summary>
            /// <param name="amountElements"></param>
            public float SetStartCycleData(int amountElements,float dt) {
                if (!cyclicExecutionFinished) {
                    throw new Exception("Tried to start cycle, but cycle-execution is already executing!");
                } 

                cyclicExecutionFinished = false;
                this.amountElements = amountElements;
                processElementsPerCycle = (int)Mathf.Ceil(amountElements / amountFrames) + amountFrames;
                nextStartElement = 0;
                nextAmountElements = Mathf.Min(processElementsPerCycle,amountElements);
                this.amountElements -= nextAmountElements;
                currentCycleFrame = 0;
                float _dt = AddAndGetDeltaTimeForCycle(0, dt);
                return _dt;
            }


            public float NextCycleData(float dt) {
                currentCycleFrame++;
                nextStartElement += nextAmountElements;
                nextAmountElements = Mathf.Min(processElementsPerCycle,amountElements);
                amountElements -= nextAmountElements;
                if (amountElements == 0) {
                    cyclicExecutionFinished = true; // finished
                }

                float _dt = AddAndGetDeltaTimeForCycle(currentCycleFrame, dt);
                return _dt;
            }


            /// <summary>
            /// Add deltatime to all deltaTimeCycles and get and clear deltaTime for specified cycle 
            /// </summary>
            /// <param name="cycle"></param>
            /// <param name="dt"></param>
            /// <returns></returns>
//            StringBuilder stb = new StringBuilder();
            public float AddAndGetDeltaTimeForCycle(int cycle, float dt) {
                float result=0;
  //              stb.Clear();
                for (int i = 0; i < amountFrames; i++) {
                    if (i == cycle) {
                        result = deltaTimesPerCycle[i] + dt;
                        deltaTimesPerCycle[i] = 0;
                    } else {
                        deltaTimesPerCycle[i] += dt;
                    }

    //                stb.Append(deltaTimesPerCycle[i]).Append(" | ");
                }
      //          stb.Append("  ## current: ").Append(result);
        //        Debug.Log(stb.ToString());
                return result;
            }


        }
    }
}
