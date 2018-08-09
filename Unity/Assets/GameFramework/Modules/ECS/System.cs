using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace ECS {
    public abstract class System<TComponents> : IDisposable, ISystem where TComponents : ISystemComponents, new(){

        public IEntityManager entityManager { get; private set; }

        protected List<UID> validEntities;
        protected List<TComponents> componentsToProcess;

        private CompositeDisposable disposables;

        public System() : this(null) {

        }

        public System(IEntityManager entityManager) {
            validEntities = new List<UID>();
            componentsToProcess = new List<TComponents>();
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
        }

        void UnregisterEntity(UID entity) {
            //UnityEngine.Debug.Log(entity.ID + " invalid! Removing from system!");
            //Remove components to process
            int _entityID = entity.ID;
            TComponents components = componentsToProcess.Find(o => o.Entity.ID == _entityID);
            if (components != null) {
                componentsToProcess.Remove(components);
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
