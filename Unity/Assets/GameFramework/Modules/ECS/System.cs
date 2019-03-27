using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UniRx;
using Zenject;

namespace ECS {
    public abstract class System<TComponents> : IDisposable, ISystem where TComponents : ISystemComponents, new(){

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
        //protected List<UID> validEntities;
        protected HashSet<UID> validEntities;
        protected List<TComponents> componentsToProcess; //Was hashset in the past, but hashsets were super slow when multithreading systems. Testcase showed 28ms (hashset) vs 14ms (list)!

        /// <summary>
        /// Temporarily store newly registered Component(packs) here, when a new entity got valid
        /// </summary>
        protected List<TComponents> newComponents;
        /// <summary>
        /// Temporarily store entities that got invalid
        /// </summary>
        protected List<TComponents> removedComponents;

        private CompositeDisposable disposables;

        /// <summary>
        /// The delta time, that will be used for the next legit ProcessAll call. Once this value reaches a value that is higher or equal to SystemUpdateRate(), ProcessAll() call is valid.
        /// </summary>
        float currentUpdateDeltaTime = 0;

        ParallelSystemComponentsProcessor<TComponents> parallelSystemComponentProcessor;

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
            componentsToProcess = new List<TComponents>(65535); //Initial size is ushort. Will allocate more, if needed.
            disposables = new CompositeDisposable();
            newComponents = new List<TComponents>();
            removedComponents = new List<TComponents>();

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
                parallelSystemComponentProcessor = new ParallelSystemComponentsProcessor<TComponents>(ProcessAtIndex);
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


        protected abstract bool UseParallelSystemComponentsProcessing();

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



                parallelSystemComponentProcessor.Process(componentsToProcess, deltaTime);

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
                for (int i = 0; i < componentsToProcess.Count; ++i) {
                    ProcessAtIndex(i, deltaTime);
                }
            }      
        }

        protected abstract void ProcessAtIndex(int componentIndex, float deltaTime);

        public void ProcessSystem(float deltaTime) {
            if (newComponents.Count > 0) {
                OnRegistered(newComponents);
                for(int i=0; i<newComponents.Count; ++i) {
                    TComponents components = newComponents[i];
                    EntityUpdated(ref components);
                }
                newComponents.Clear();
            }
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
                currentUpdateDeltaTime += deltaTime;
                //Process system components
                if (currentUpdateDeltaTime >= SystemUpdateRate()) {
                    ProcessAll(currentUpdateDeltaTime);
                    currentUpdateDeltaTime = 0;
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
        /// Get components for entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected TComponents GetSystemComponentsForEntity(UID entity) {
            TComponents components = componentsToProcess.Find(o => o.Entity == entity);
            return components;
        }


        void UpdateEntity(UID entity) {
            var entityComponents = GetSystemComponentsForEntity(entity);
            GetEntityComponents(entityComponents, entity);
            EntityUpdated(ref entityComponents);
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
            componentsToProcess.Add(components);

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
                componentsToProcess.Remove(components);
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
            // TODO: Get rid of this again
            //tc.EntityManager = entityManager;
            return GetEntityComponents(tc, entity);
        }

        /// <summary>
        /// Caches all needed components from the entity
        /// </summary>
        /// <param name="components"></param>
        /// <returns></returns>
        protected abstract TComponents GetEntityComponents(TComponents components, UID entity);
       
        public void AddDisposable(IDisposable disposable) {
            disposables.Add(disposable);
        }

        public virtual void Dispose() {
            if(parallelSystemComponentProcessor != null) parallelSystemComponentProcessor.Dispose();

            disposables.Dispose();
            disposables = null;

            entityManager = null;

            validEntities.Clear();
            componentsToProcess.Clear();

            UnityEngine.Debug.Log("System disposed");
        }
    }
}
