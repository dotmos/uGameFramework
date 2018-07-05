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
        Dictionary<UID, List<IComponent>> _entities;

        /// <summary>
        /// List of all registered systems
        /// </summary>
        List<ISystem> _systems;

        private Queue<int> _recycledIds;
        private int _lastId { get; set; }

        public EntityManager() {
            _entities = new Dictionary<UID, List<IComponent>>();
            _systems = new List<ISystem>();
            _recycledIds = new Queue<int>();
            _lastId = 0;
        }

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
            UnityEngine.Debug.Log(uid.ID);

            _entities.Add(uid, new List<IComponent>());

            return uid;
        }

        /// <summary>
        /// Destroys the entity
        /// </summary>
        /// <param name="entityID"></param>
        public void DestroyEntity(UID entity) {
            if (EntityExists(entity)) {
                _entities[entity].Clear();
                EntityModified(entity);
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
        public bool EntityExists(UID entity) {
            return _entities.ContainsKey(entity);
        }


        /// <summary>
        /// Adds the component to the entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public T AddComponent<T>(UID entity) where T : IComponent, new() {
            UnityEngine.Debug.Log("Adding component "+typeof(T)+" to entity:" + entity.ID);
            if (EntityExists(entity)) {
                IComponent component = new T();
                return (T)AddComponent(entity, component);
            }
            return default(T);
        }
        public IComponent AddComponent(UID entity, IComponent component) {
            if (EntityExists(entity) && !HasComponent(entity, component)) {
                //component.Entity.SetID(entity.ID);
                component.Entity = entity;
                _entities[entity].Add(component);
                EntityModified(entity);
                UnityEngine.Debug.Log("Added component " + component.GetType() + " to entity:" + entity.ID);
                return component;
            }
            return null;
        }

        /// <summary>
        /// Removes the component from the entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void RemoveComponent<T>(UID entity) where T : IComponent {
            RemoveComponent(entity, GetComponent<T>(entity));
        }
        public void RemoveComponent(UID entity, IComponent component) {
            if(component != null && EntityExists(entity)) {
                _entities[entity].Remove(component);
                component.Entity.SetID(-1);
                EntityModified(entity);
            }
        }

        /// <summary>
        /// Gets the component from the entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T GetComponent<T>(UID entity) where T : IComponent{
            if (EntityExists(entity)) {
                //TODO: This is slow. Rethink how entities are stored.
                IComponent c = _entities[entity].Find(o => o is T);
                if(c != null) {
                    return (T)c;
                }
            }

            UnityEngine.Debug.LogError("Entity " + entity.ID + " does not exist!");
            //throw new Exception("Entity " + entity.ID + " does not exist!");
            return default(T);
        }

        /// <summary>
        /// Checks if an entity has the component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool HasComponent<T>(UID entity) where T: IComponent {
            if (EntityExists(entity)) {
                //TODO: This is slow. Rethink how entities/components are stored.
                IComponent c = _entities[entity].Find(o => o is T);
                if (c != null) {
                    return true;
                }
            }
            return false;
        }

        public bool HasComponent(UID entity, IComponent component) {
            if (EntityExists(entity)) {
                //TODO: This is slow. Rethink how entities/components are stored.
                IComponent c = _entities[entity].Find(o => o == component);
                if (c != null) {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Register a system with the entity manager
        /// </summary>
        /// <param name="system"></param>
        public void RegisterSystem(ISystem system) {
            if (system.entityManager == null && !_systems.Contains(system)) {
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
        protected virtual void EntityModified(UID entity) {
            for(int i=0; i<_systems.Count; ++i) {
                _systems[i].EntityModified(entity);
            }
        }
    }
}