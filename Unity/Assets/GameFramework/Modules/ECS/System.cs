﻿using System;
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
#if ECS_PROFILING
        private readonly System.Diagnostics.Stopwatch frameWatch = new System.Diagnostics.Stopwatch();
        public  readonly System.Diagnostics.Stopwatch secondWatch = new System.Diagnostics.Stopwatch();


        private int tickCounts = 0;
        public double avgElapsedTime = 0;

        public double AvgElapsedTime => avgSecond;
        public double _secondData = 0;
        public double lastFPS = 0;

        public double avgSecond = 0;
        public int avgSecondCount = 0;
        
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
        protected List<UID> pendingNewEntities; //a list of components that needs to be added after the cycle
        protected HashSet<UID> pendingNewEntitiesCheck; //check if this entity is marked as new already
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
            pendingNewEntities = new List<UID>();
            pendingNewEntitiesCheck = new HashSet<UID>();

            newComponents = new List<TComponents>();
            removedComponents = new List<TComponents>();
            updatedComponents = new List<TComponents>();
            updatedComponentsLUT = new HashSet<TComponents>();

            SetEntityManager(entityManager);

            PreBind();

            Kernel.Instance.Inject(this);

#if ECS_PROFILING 
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

        protected void EnableCyclicProcess(int ticksPerCycle, int minElementAmounts) {
            useCyclicExecution = true;
            cyclicExecutionData = new CyclicExecutionData(ticksPerCycle,minElementAmounts);
        }

        /// <summary>
        /// The maximum chunk size per thread when using parallel processing
        /// </summary>
        /// <returns></returns>
        protected virtual int MaxParallelChunkSize() {
            return 9999999;
        }

        private void ProcessPendingNewEntities() {
            int pendingNewAmount = pendingNewEntities.Count;
            if (pendingNewAmount > 0) {
                for (int i = pendingNewAmount - 1; i >= 0; i--) {
                    UID newEntity = pendingNewEntities[i];
                    _RegisterEntity(newEntity);
                }
                pendingNewEntities.Clear(); 
                pendingNewEntitiesCheck.Clear();
            }
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
                            if (pendingNewEntities.Count > 0) {
                                ProcessPendingNewEntities();
                            }
                            // too less elements for the cycle to be used => default way
                            parallelSystemComponentProcessor.Process(componentCount, deltaTime,MaxParallelChunkSize());
                            return; // finish here
                        } else {
                            // start a new cycle and get the deltaTime for the current cycle-tick
                            cyclicDt = cyclicExecutionData.SetStartCycleData(componentCount, deltaTime);
                        }
                    } else {
                        cyclicDt = cyclicExecutionData.NextCycleData(deltaTime);
                    }
                    if (cyclicExecutionData.currentCycleTick == 0) {
                        CycleBeforeFirstCycle();
                    }
                    parallelSystemComponentProcessor.Process(cyclicExecutionData.nextAmountElements, cyclicDt, MaxParallelChunkSize(), cyclicExecutionData.nextStartElement);
                    CycleTickFinished(cyclicExecutionData.currentCycleTick);
                    if (cyclicExecutionData.cyclicExecutionFinished) {
                        // now is the time to apply pending removals
                        int pendingRemovalAmount = pendingRemovalComponents.Count;

                        if (pendingRemovalAmount > 0) {
                            for (int i = pendingRemovalAmount - 1; i >= 0; i--) {
                                var _removeComps = pendingRemovalComponents[i];
                                for (int idx = 0; idx < componentCount; ++idx) {
                                    if (componentsToProcess[idx].Entity.ID == _removeComps.Entity.ID) {
                                        componentsToProcess[idx] = componentsToProcess[componentCount - 1]; //Put last item in array to position of item that should be deleted, overwriting (and therefore deleting) it
                                        break;
                                    }
                                }
                                componentsToProcessLUT.Remove(_removeComps.Entity.ID);
                            }
                            pendingRemovalComponents.Clear();
                            pendingRemovalComponentsCheck.Clear();
                        }

                        ProcessPendingNewEntities();


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
                    ProcessAtIndex(i, deltaTime, -1);
                }
            }      
        }

        protected abstract void ProcessAtIndex(int componentIndex, float deltaTime, int workerID);



        public void ProcessSystem(float deltaTime,float unscaled, float systemScaled) {
#if TESTING
            //tempStart.system = this;
            //tempStart.componentsToProcess = componentsToProcess;
            //ScriptingService.Callback("system", GetType(), tempStart);
#endif
            //Tell system there are new components
            if (newComponents.Count > 0 && (!useCyclicExecution || cyclicExecutionData.cyclicExecutionFinished)) {
                OnRegistered(newComponents);
                int _count = newComponents.Count;
                for(int i=0; i< _count; ++i) {
                    TComponents components = newComponents[i];
                    EntityUpdated(ref components);
                }
                newComponents.Clear();
            }
            //Tell system components were updated
            if(updatedComponents.Count > 0 && (!useCyclicExecution || cyclicExecutionData.cyclicExecutionFinished)) {
                int _count = updatedComponents.Count;
                for(int i=0; i< _count; ++i) {
                    TComponents components = updatedComponents[i];
                    EntityUpdated(ref components);
                }
                updatedComponents.Clear();
                updatedComponentsLUT.Clear();
            }
            //Tell system that components were removed
            if (removedComponents.Count > 0 && (!useCyclicExecution || cyclicExecutionData.cyclicExecutionFinished))
            {
                OnUnregistered(removedComponents);
                removedComponents.Clear();
            }
            try
            {
#if SYSTEMS_LEGACY
                currentTimer += deltaTime;
#else
                currentTimer += SystemFixedRate() ? unscaled : systemScaled;
#endif
                currentUpdateDeltaTime += deltaTime;
                //Process system components
                if (currentTimer >= SystemUpdateRate()) {
#if ECS_PROFILING
                    frameWatch.Restart();
#endif

                    //Regular tick
                    ProcessAll(currentUpdateDeltaTime);
                    currentUpdateDeltaTime = 0;
                    currentTimer = 0;
#if ECS_PROFILING
                    frameWatch.Stop();

                    if (EntityManager.logEntityManager) {
                        var elapsedTime = frameWatch.Elapsed.TotalMilliseconds;

                        _secondData += elapsedTime;
                        if (!secondWatch.IsRunning) {
                            secondWatch.Restart();
                        }

                        if (elapsedTime > maxElapsedTime) {
                            maxElapsedTime = elapsedTime;
                        }
                        if (tickCounts == 0) {
                            avgElapsedTime = elapsedTime;
                            tickCounts++;
                        } else {
                            avgElapsedTime = (avgElapsedTime * tickCounts + elapsedTime) / (tickCounts + 1);
                            tickCounts++;
                        }
                        if (elapsedTime > 5) {
                            veryhighTicks++;
                        } else if (elapsedTime > 3) {
                            highTicks++;
                        } else if (elapsedTime > 1) {
                            mediumTicks++;
                        }
                    }

#endif

                } else if(deltaTime == 0 && ForceTickOnDeltaZero()) {
                    //Force tick with deltaTime 0
                    ProcessAll(0);
                }
#if ECS_PROFILING
                if (secondWatch.Elapsed.TotalSeconds > 1.0) {
                    if (avgSecondCount == 0) {
                        avgSecond = _secondData;
                    } else {
                        avgSecond = (avgSecond * avgSecondCount + _secondData) / (avgSecondCount + 1);
                    }
                    avgSecondCount++;
                    lastFPS = _secondData;
                    _secondData = 0;
                    secondWatch.Restart();
                }
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

#if ECS_PROFILING
        public void ShowLog(bool showOnDevUIConsole=false,bool forceAll=true) {
            logTxtBuilder.Clear();
            if (!forceAll && avgElapsedTime < 0.005) {
                return;
            }

            
            logTxtBuilder.Append("calls:")
                .Append(tickCounts).Append(" avg(sec):").Append(avgSecond.ToString("F5").TrimEnd('0'))
                .Append(" avg(single):").Append(avgElapsedTime.ToString("F5").TrimEnd('0'))
                .Append("] max:").Append(maxElapsedTime)
                .Append(" [>1ms:").Append(mediumTicks)
                .Append("|>3ms:").Append(highTicks)
                .Append("|>5ms:").Append(veryhighTicks)
                .Append("] System:").Append(GetType().Name);

            UnityEngine.Debug.Log(logTxtBuilder.ToString());
            if (showOnDevUIConsole) {
                Kernel.Instance.Resolve<Service.DevUIService.IDevUIService>().WriteToScriptingConsole(logTxtBuilder.ToString());
            }
        }

        public void ResetLog() {
            avgElapsedTime = 0;
            maxElapsedTime = 0;
            mediumTicks = 0;
            highTicks = 0;
            veryhighTicks = 0;
            tickCounts = 0;
            avgElapsedTime = 0;
            avgSecond = 0;
            avgSecondCount = 0;
            _secondData = 0;
        }

#endif


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
                if (!pendingNewEntitiesCheck.Contains(entity)) {
                    RegisterEntity(entity);
                }
            }
            else if (!valid && wasValid) {
                UnregisterEntity(entity);
            }
        }

        /// <summary>
        /// getting called just before the first cycle-tick
        /// </summary>
        protected virtual void CycleBeforeFirstCycle() { }
        
        /// <summary>
        /// Called after another batch is finished
        /// </summary>
        /// <param name="currentTick"></param>
        protected virtual void CycleTickFinished(int currentTick) { 
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

        void _RegisterEntity(UID entity) {
            //UnityEngine.Debug.Log(entity.ID + "valid! Adding to system!");
            validEntities.Add(entity);
            //Add components to process
            TComponents components = _CreateSystemComponentsForEntity(entity);
            if (componentCount == componentsToProcess.Length) {
                //TComponents[] newComponentsToProcess = new TComponents[componentsToProcess.Length * 2];
                //componentsToProcess.CopyTo(newComponentsToProcess, 0);
                Array.Resize(ref componentsToProcess, componentsToProcess.Length * 2);
            }
            componentsToProcess[componentCount] = components;
            componentCount++;
#if UNITY_EDITOR
            if (componentsToProcessLUT.ContainsKey(entity.ID)) {
                var newcomps = entityManager.GetAllComponents(entity);
                var current = GetSystemComponentsForEntity(entity);
            }
#endif
            try {
                componentsToProcessLUT.Add(entity.ID, components);
            }
            catch (Exception e) {
                componentsToProcessLUT[entity.ID] = components;
                Debug.LogException(e); 
            }
            newComponents.Add(components);
        }

        void RegisterEntity(UID entity) {
            if (!useCyclicExecution) {
                // immediately register entity
                _RegisterEntity(entity);
            } else {
                if (!pendingNewEntitiesCheck.Contains(entity)) {
                    // in cyclic execution, we need to wait for a full cycle to end to register
                    pendingNewEntities.Add(entity);
                    pendingNewEntitiesCheck.Add(entity);
                }
            }
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
                    pendingRemovalComponentsCheck.Add(components);
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
            TComponents[] allValidComponents = new TComponents[componentCount];
            Array.Copy(componentsToProcess, allValidComponents, componentCount);
            List<TComponents> _allComponents = new List<TComponents>(allValidComponents);

            OnUnregistered(_allComponents);
            _allComponents.Clear();
            allValidComponents = null;


            validEntities.Clear();
            //componentsToProcess.Clear();
            componentsToProcess = new TComponents[1];
            componentCount = 0;
            componentsToProcessLUT.Clear();
            pendingNewEntities.Clear();
            pendingNewEntitiesCheck.Clear();
            pendingRemovalComponents.Clear();
            pendingRemovalComponentsCheck.Clear();
        }

        public virtual void Dispose() {
            if(parallelSystemComponentProcessor != null) parallelSystemComponentProcessor.Dispose();

            entityManager = null;

            validEntities.Clear();
            //componentsToProcess.Clear();
            componentsToProcess = new TComponents[1];
            componentCount = 0;
            componentsToProcessLUT.Clear();
            pendingNewEntities.Clear();
            pendingNewEntitiesCheck.Clear();
            pendingRemovalComponents.Clear();
            pendingRemovalComponentsCheck.Clear();

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
            ///  amount of ticks to distribute the complete element amount on
            /// </summary>
            public int ticksPerCycle = 0;


            /// <summary>
            /// Each cycle needs to maintain its deltatime until it is triggered next
            /// </summary>
            float[] deltaTimesPerCycle;

            /// <summary>
            /// Amount of elements to process by the cycle 
            /// </summary>
            public int processElementsPerCycle = 0;
            /// <summary>
            /// current cycle tick
            /// </summary>
            public int currentCycleTick = 0;
            /// <summary>
            /// amount elements left
            /// </summary>
            public int amountElements = 0;
            /// <summary>
            /// start elementIdx for the next cycle-tick
            /// </summary>
            public int nextStartElement = 0;
            /// <summary>
            /// amount of elements to be processed next cycle tick
            /// </summary>
            public int nextAmountElements = 0;

            public CyclicExecutionData(int ticksPerCycle,int cyclicExecutionMinAmount=1000) {
                this.ticksPerCycle = ticksPerCycle;
                this.cyclicExecutionMinAmount = cyclicExecutionMinAmount;
                deltaTimesPerCycle = new float[ticksPerCycle];
            }


            /// <summary>
            /// Start a cycle by specifiying the amount of elements in the system
            /// </summary>
            /// <param name="amountElements"></param>
            public float SetStartCycleData(int amountElements,float dt) {
                if (!cyclicExecutionFinished) {
                    throw new Exception("Tried to start cycle, but cycle-execution is already executing!");
                } 

                cyclicExecutionFinished = false;
                this.amountElements = amountElements;
                processElementsPerCycle = (int)Mathf.Ceil(amountElements / ticksPerCycle) + ticksPerCycle;
                nextStartElement = 0;
                nextAmountElements = Mathf.Min(processElementsPerCycle,amountElements);
                this.amountElements -= nextAmountElements;
                currentCycleTick = 0;
                float _dt = AddAndGetDeltaTimeForCycle(0, dt);
                return _dt;
            }


            public float NextCycleData(float dt) {
                currentCycleTick++;
                nextStartElement += nextAmountElements;
                nextAmountElements = Mathf.Min(processElementsPerCycle,amountElements);
                amountElements -= nextAmountElements;
                if (amountElements == 0) {
                    cyclicExecutionFinished = true; // finished
                }

                float _dt = AddAndGetDeltaTimeForCycle(currentCycleTick, dt);
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
                for (int i = 0; i < ticksPerCycle; i++) {
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
