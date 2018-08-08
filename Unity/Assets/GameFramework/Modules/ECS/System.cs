﻿using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace ECS {
    public abstract class System<TComponents> : IDisposable, ISystem where TComponents : ISystemComponents, new(){

        public IEntityManager entityManager { get; private set; }

        protected List<UID> validEntities;
        protected List<TComponents> componentsToProcess;
        protected Dictionary<UID, IDisposable> processUpdates;

        private CompositeDisposable disposables;

        public System() : this(null) {

        }

        public System(IEntityManager entityManager) {
            validEntities = new List<UID>();
            componentsToProcess = new List<TComponents>();
            processUpdates = new Dictionary<UID, IDisposable>();
            disposables = new CompositeDisposable();

            SetEntityManager(entityManager);

            PreBind();

            AfterBind();
        }
        
        protected virtual void PreBind() {

        }

        protected virtual void AfterBind() {
        }

        /// <summary>
        /// Process all entities
        /// </summary>
        protected virtual void ProcessAll() {
            for(int i=0; i<componentsToProcess.Count; ++i) {
                //TComponents c = componentsToProcess[i];
                Process(componentsToProcess[i]);
                //componentsToProcess[i] = c;
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
            //Add rx process update
            IObservable<TComponents> processUpdate = SetupProcessUpdate(components);
            if (processUpdate != null) {
                processUpdates.Add(entity, processUpdate.Subscribe(e => Process(e)));
            }
        }

        void UnregisterEntity(UID entity) {
            //UnityEngine.Debug.Log(entity.ID + " invalid! Removing from system!");
            //Remove components to process
            int _entityID = entity.ID;
            TComponents components = componentsToProcess.Find(o => o.Entity.ID == _entityID);
            if (components != null) {
                componentsToProcess.Remove(components);
            }
            //Remove rx process update
            if (processUpdates.ContainsKey(entity)) {
                processUpdates[entity].Dispose();
                processUpdates[entity] = null;
                processUpdates.Remove(entity);
            }
            validEntities.Remove(entity);
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

        /// <summary>
        /// Optional rx binding for entity processing. If not set, you have to manually call Process() for entity components (i.e. in an update loop)
        /// </summary>
        /// <param name="components"></param>
        /// <returns></returns>
        protected virtual IObservable<TComponents> SetupProcessUpdate(TComponents components) {
            return null;
        }

        public void AddDisposable(IDisposable disposable) {
            disposables.Add(disposable);
        }

        public virtual void Dispose() {
            disposables.Dispose();
            disposables = null;

            entityManager = null;

            validEntities.Clear();
            componentsToProcess.Clear();

            foreach(IDisposable i in processUpdates.Values) {
                i.Dispose();
            }
            processUpdates.Clear();
        }
    }
}