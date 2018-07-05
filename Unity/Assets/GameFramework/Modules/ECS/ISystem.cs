using System.Collections;
using System.Collections.Generic;

namespace ECS {
    public interface ISystem {
        /// <summary>
        /// Sets the entity manager for this system
        /// </summary>
        /// <param name="manager"></param>
        void SetEntityManager(EntityManager manager);

        /// <summary>
        /// The entity manager of this system
        /// </summary>
        EntityManager entityManager { get; }

        /// <summary>
        /// Call whenever an entity is modified
        /// </summary>
        /// <param name="entity"></param>
        void EntityModified(ref UID entity);
    }
}

