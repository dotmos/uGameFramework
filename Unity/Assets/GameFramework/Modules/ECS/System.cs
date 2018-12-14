﻿using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using Zenject;

namespace ECS {
    public abstract class System<TComponents> : IDisposable, ISystem where TComponents : ISystemComponents, new(){

        public IEntityManager entityManager { get; private set; }

        Service.Events.IEventsService eventService;

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
        protected List<UID> removedEntities;

        private CompositeDisposable disposables;

        public System() : this(null) {

        }

        public System(IEntityManager entityManager) {
            validEntities = new HashSet<UID>();
            componentsToProcess = new List<TComponents>(65535); //Initial size is ushort. Will allocate more, if needed.
            disposables = new CompositeDisposable();
            newComponents = new List<TComponents>();
            removedEntities = new List<UID>();

            SetEntityManager(entityManager);

            PreBind();

            Kernel.Instance.Inject(this);

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
        protected virtual void OnUnregistered(List<UID> unregisteredEntities) {
        }


        /// <summary>
        /// Process all entities
        /// </summary>
        protected virtual void ProcessAll(float deltaTime) {
            /*
            for(int i=0; i<componentsToProcess.Count; ++i) {
                //TComponents c = componentsToProcess[i];
                Process(componentsToProcess[i]);
                //componentsToProcess[i] = c;
            }
            */

            foreach(TComponents c in componentsToProcess) {
                try {
                    Process(c);
                }
                catch (Exception e){
                    UnityEngine.Debug.Log("There was a problem in process all in system:"+GetType());
                    UnityEngine.Debug.LogException(e);
                }
            }
        }

        public void ProcessSystem(float deltaTime) {
            if (newComponents.Count > 0) {
                OnRegistered(newComponents);
                newComponents.Clear();
            }
            if (removedEntities.Count > 0)
            {
                OnUnregistered(removedEntities);
                removedEntities.Clear();
            }
            try
            {
                ProcessAll(deltaTime);
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
        /// Call whenever an entity is modified
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
                UnregisterEntity(entity);
                RegisterEntity(entity);
            }
            if (valid && !wasValid) {
                RegisterEntity(entity);
            }
            if (!valid && wasValid) {
                UnregisterEntity(entity);
            }
        }
        

        void RegisterEntity(UID entity) {
            //UnityEngine.Debug.Log(entity.ID + "valid! Adding to system!");
            validEntities.Add(entity);
            //Add components to process
            TComponents components = _GetEntityComponents(entity);
            componentsToProcess.Add(components);

            newComponents.Add(components);
        }

        void UnregisterEntity(UID entity) {
            //UnityEngine.Debug.Log(entity.ID + " invalid! Removing from system!");
            //Remove components to process
            int _entityID = entity.ID;
            
            //TODO: Find a faster way to remove the components.
            TComponents components = componentsToProcess.Find(o => o.Entity.ID == _entityID);
            if (components != null) {
                componentsToProcess.Remove(components);
            }
            
            //componentsToProcess.RemoveWhere(v => v.Entity.ID == _entityID);
            validEntities.Remove(entity);
            removedEntities.Add(entity);
        }

        /// <summary>
        /// Checks if the entity can be used by the system
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected abstract bool IsEntityValid(UID entity);

        private TComponents _GetEntityComponents(UID entity) {
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

        /// <summary>
        /// Process the cached entity components
        /// </summary>
        /// <param name="components"></param>
        protected virtual void Process(TComponents components) { }
        
        public void AddDisposable(IDisposable disposable) {
            disposables.Add(disposable);
        }

        public virtual void Dispose() {
            disposables.Dispose();
            disposables = null;

            entityManager = null;

            validEntities.Clear();
            componentsToProcess.Clear();           
        }
    }
}
