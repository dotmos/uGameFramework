using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace ECS {
    public abstract class System<TComponents> : IDisposable, ISystem where TComponents : ISystemComponents, new(){

        public EntityManager entityManager { get; private set; }

        protected List<UID> validEntities;
        protected List<TComponents> componentsToProcess;

        private CompositeDisposable disposables;

        public System() : this(null) {

        }

        public System(EntityManager entityManager) {
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
        /// Sets the entity manager of this system
        /// </summary>
        /// <param name="manager"></param>
        public void SetEntityManager(EntityManager manager) {
            entityManager = manager;
        }

        /// <summary>
        /// Call whenever an entity is modified
        /// </summary>
        /// <param name="entity"></param>
        public virtual void EntityModified(UID entity) {
            bool valid = IsEntityValid(entity);
            bool wasValid = validEntities.Contains(entity);

            UnityEngine.Debug.Log(entity.ID + "valid: "+valid);

            if (valid && !wasValid) {
                UnityEngine.Debug.Log(entity.ID + "valid! Adding to system!");
                validEntities.Add(entity);
                componentsToProcess.Add(_GetEntityComponents(entity));
            }
            if (!valid && wasValid) {
                UnityEngine.Debug.Log(entity.ID + " invalid! Removing from system!");
                TComponents components = componentsToProcess.Find(o => o.Entity.ID == entity.ID);
                if (components != null) {
                    componentsToProcess.Remove(components);
                }
                validEntities.Remove(entity);
            }
        }

        protected abstract bool IsEntityValid(UID entity);
        private TComponents _GetEntityComponents(UID entity) {
            TComponents tc = new TComponents();
            tc.Entity = entity;
            return GetEntityComponents(ref tc);
        }
        protected abstract TComponents GetEntityComponents(ref TComponents components);

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
