using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace ECS {
    /// <summary>
    /// Helper class for setting up an entity through Unity's inspector
    /// </summary>
    public class MonoEntity : MonoBehaviour {
        public UID Entity { get; private set; }

        private IEntityManager entityManager;

        protected virtual void Awake() {
            Kernel.Instance.Inject(this);
        }

        [Inject]
        void OnInject(
            [Inject] IEntityManager entityManager
            ) {
            this.entityManager = entityManager;
            this.Entity = this.entityManager.CreateEntity();
        }

        public IEntityManager GetEntityManager() {
            return entityManager;
        }

        /// <summary>
        /// Call this when data inside entity components was modified and you want systems to update
        /// </summary>
        public void EntityModified() {
            GetEntityManager().EntityModified(Entity);
        }

        public bool HasEntityComponent<T>() where T:ECS.IComponent {
            return GetEntityManager().HasComponent<T>(Entity);
        }

        public T GetEntityComponent<T>() where T : ECS.IComponent {
            return GetEntityManager().GetComponent<T>(Entity);
        }
    }
}
