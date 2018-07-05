using System;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
    public class EntityManager {

        /// <summary>
        /// Holds components of entities
        /// </summary>
        //This is super cache unfriendly.
        //TODO: Make cache friendly and do not use a list of components, but use a list of ids, targeting component arrays of same component type. I.e. one array per component type
        Dictionary<UID, List<IComponent>> _entities = new Dictionary<UID, List<IComponent>>();

        /// <summary>
        /// List of all registered systems
        /// </summary>
        List<ISystem> _systems = new List<ISystem>();

        private Queue<int> _recycledIds;
        private int _lastId { get; set; } = 0;

        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <returns></returns>
        public UID CreateEntity() {
            int id = 0;

            if (_lastId + 1 >= int.MaxValue && _recycledIds.Count == 0) {
                UnityEngine.Debug.LogError("WARNING: No free entity IDs! Restarting from 0. This should not happen!");
                _lastId = 0;
            }

            if (_recycledIds.Count > 0) {
                id = _recycledIds.Dequeue();
            } else {
                id = _lastId;
                _lastId++;
            }

            UID uid = new UID(id);

            _entities.Add(uid, new List<IComponent>());

            return uid;
        }

        /// <summary>
        /// Destroys the entity
        /// </summary>
        /// <param name="entityID"></param>
        public void DestroyEntity(ref UID entity) {
            if (EntityExists(ref entity)) {
                _entities[entity].Clear();
                EntityModified(ref entity);
                _entities[entity] = null;
                _entities.Remove(entity);
                _recycledIds.Enqueue(entity.ID);
            }
        }

        /// <summary>
        /// Checks if the entity is alive/exists
        /// </summary>
        /// <param name="entityID"></param>
        /// <returns></returns>
        public bool EntityExists(ref UID entity) {
            return _entities.ContainsKey(entity);
        }


        /// <summary>
        /// Adds the component to the entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public T AddComponent<T>(ref UID entity) where T : IComponent, new() {
            if (EntityExists(ref entity)) {
                return (T)AddComponent(ref entity, new T());
            }
            return default(T);
        }
        public IComponent AddComponent(ref UID entity, IComponent component) {
            if (component.Entity.ID == -1 || EntityExists(ref entity)) {
                component.Entity.SetID(entity.ID);
                _entities[entity].Add(component);
                EntityModified(ref entity);
            }
            return null;
        }

        /// <summary>
        /// Removes the component from the entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void RemoveComponent<T>(ref UID entity) where T : IComponent {
            RemoveComponent(ref entity, GetComponent<T>(ref entity));
        }
        public void RemoveComponent(ref UID entity, IComponent component) {
            if(component != null && EntityExists(ref entity)) {
                _entities[entity].Remove(component);
                component.Entity.SetID(-1);
                EntityModified(ref entity);
            }
        }

        /// <summary>
        /// Gets the component from the entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T GetComponent<T>(ref UID entity) where T : IComponent{
            if (EntityExists(ref entity)) {
                //TODO: This is very slow. Rethink how entities are stored.
                IComponent c = _entities[entity].Find(o => o is T);
                if(c != null) {
                    return (T)c;
                }
            }
            return default(T);
        }

        /// <summary>
        /// Checks if an entity has the component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool HasComponent<T>(ref UID entity) where T: IComponent {
            //TODO: This is very slow. Rethink how entities are stored.
            return GetComponent<T>(ref entity) != null ? true : false;
        }


        /// <summary>
        /// Register a system with the entity manager
        /// </summary>
        /// <param name="system"></param>
        public void RegisterSystem(ISystem system) {
            if (system.entityManager != null && !_systems.Contains(system)) {
                system.SetEntityManager(this);
                _systems.Add(system);
            }
        }

        /// <summary>
        /// Unregister a system from the entity manager
        /// </summary>
        /// <param name="system"></param>
        public void UnregisterSystem(ISystem system) {
            if (_systems.Contains(system)) {
                _systems.Remove(system);
                system.SetEntityManager(null);
            }
        }

        /// <summary>
        /// Call whenever an entity is modified
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void EntityModified(ref UID entity) {
            for(int i=0; i<_systems.Count; ++i) {
                _systems[i].EntityModified(ref entity);
            }
        }
    }
}