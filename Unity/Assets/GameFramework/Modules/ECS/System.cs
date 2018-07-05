using System;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
    public abstract class System<TComponents> : ISystem where TComponents : new(){

        public EntityManager entityManager { get; private set; }

        protected List<UID> _validEntities;
        protected List<TComponents> _componentsToProcess;

        public System() : this(null) {

        }

        public System(EntityManager entityManager) {
            _validEntities = new List<UID>();
            SetEntityManager(entityManager);
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
        public virtual void EntityModified(ref UID entity) {
            bool valid = IsEntityValid(ref entity);
            bool wasValid = _validEntities.Contains(entity);

            if (valid && !wasValid) {
                _validEntities.Add(entity);
                _componentsToProcess.Add(GetEntityComponents(ref entity));
            }
            if (!valid && wasValid) {
                _validEntities.Remove(entity);
            }
        }

        protected abstract bool IsEntityValid(ref UID entity);
        protected abstract TComponents GetEntityComponents(ref UID entity);
    }
}
