using System;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
    public interface ISystem {
        /// <summary>
        /// Sets the entity manager for this system
        /// </summary>
        /// <param name="manager"></param>
        void SetEntityManager(IEntityManager manager);

        /// <summary>
        /// The entity manager of this system
        /// </summary>
        IEntityManager entityManager { get; }

        /// <summary>
        /// Call whenever an entity is modified
        /// </summary>
        /// <param name="entity"></param>
        void EntityModified(UID entity);

        void AddDisposable(IDisposable disposable);
        
        void ProcessSystem(float deltaTime);
    }
}

