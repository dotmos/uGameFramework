using System;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
    public class EntityManager : IEntityManager {

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

        private Queue<int> _recycledEntityIds;
        private int _lastEntityId { get; set; }
        static readonly int _startEntityID = 1;

        private Queue<int> _recycledComponentIds;
        private int _lastComponentId { get; set; }
        static readonly int _startComponentID = 1;

        public EntityManager() {
            _entities = new Dictionary<UID, List<IComponent>>();
            _systems = new List<ISystem>();
            _recycledEntityIds = new Queue<int>();
            _recycledComponentIds = new Queue<int>();
            _lastEntityId = _startEntityID;
            _lastComponentId = _startComponentID;

            //Register systems
            foreach(ISystem s in RegisterSystemsOnStartup()) {
                RegisterSystem(s);
            }
        }

        /// <summary>
        /// Overwrite to register these systems when this instance is created
        /// </summary>
        /// <returns></returns>
        protected virtual List<ISystem> RegisterSystemsOnStartup() {
            return new List<ISystem>();
        }

        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <returns></returns>
        public UID CreateEntity() {
            int id = -1;

            if (_lastEntityId + 1 >= int.MaxValue && _recycledEntityIds.Count == 0) {
                //UnityEngine.Debug.LogError("WARNING: No free entity IDs! Restarting from 0. This should not happen!");
                _lastEntityId = _startEntityID;
            }

            if (_recycledEntityIds.Count > 0) {
                id = _recycledEntityIds.Dequeue();
            } else {
                id = _lastEntityId;
                _lastEntityId++;
            }

            UID uid = new UID(id);
            //UnityEngine.Debug.Log(uid.ID);

            _entities.Add(uid, new List<IComponent>());

            return uid;
        }

        /// <summary>
        /// Destroys the entity
        /// </summary>
        /// <param name="entityID"></param>
        public void DestroyEntity(ref UID entity) {
            if (EntityExists(entity)) {
                _entities[entity].Clear();
                EntityModified(entity);
                _entities[entity] = null;
                _entities.Remove(entity);
                _recycledEntityIds.Enqueue(entity.ID);
                entity.ID = -1;
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
        /// Give the component a valid ID. Does nothing if component already has an id.
        /// </summary>
        /// <param name="component"></param>
        public void SetupComponentID(IComponent component) {
            if (component.ID.ID == 0) {

                int id = -1;

                if (_lastComponentId + 1 >= int.MaxValue && _recycledComponentIds.Count == 0) {
                    //UnityEngine.Debug.LogError("WARNING: No free entity IDs! Restarting from 0. This should not happen!");
                    _lastComponentId = _startComponentID;
                }

                if (_recycledEntityIds.Count > 0) {
                    id = _recycledComponentIds.Dequeue();
                }
                else {
                    id = _lastComponentId;
                    _lastComponentId++;
                }

                component.ID = new UID(id);
                //UnityEngine.Debug.Log(uid.ID);
            }
        }

        /// <summary>
        /// Adds the component to the entity. If component already exists, no new component will be added and existing component will be returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public T AddComponent<T>(UID entity) where T : IComponent, new() {
            //UnityEngine.Debug.Log("Adding component "+typeof(T)+" to entity:" + entity.ID);
            if (EntityExists(entity)) {
                if (!HasComponent<T>(entity)) {
                    IComponent component = new T();
                    return (T)AddComponent(entity, component);
                }
                else {
                    return GetComponent<T>(entity);
                }
            }
            return default(T);
        }

        public IComponent AddComponent(UID entity, IComponent component) {
            if (EntityExists(entity) && !HasComponent(entity, component)) {
                //component.Entity.SetID(entity.ID);
                component.Entity = entity;
                SetupComponentID(component);
                _entities[entity].Add(component);
                EntityModified(entity);
                //UnityEngine.Debug.Log("Added component " + component.GetType() + " to entity:" + entity.ID);
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
            if(component != null && EntityExists(entity) && HasComponent(entity, component)) {
                _entities[entity].Remove(component);
                component.Entity.SetID(-1);
                EntityModified(entity);
            }
        }
        
        /// <summary>
        /// Disposes the component, freeing it's ID and calling Dispose()
        /// </summary>
        /// <param name="component"></param>
        public void DisposeComponent(ref IComponent component) {
            RemoveComponent(component.Entity, component);
            _recycledComponentIds.Enqueue(component.ID.ID);
            component.ID = new UID(-1);
            component.Dispose();
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

            //UnityEngine.Debug.LogError("Entity " + entity.ID + " does not exist!");
            //throw new Exception("Entity " + entity.ID + " does not exist!");
            return default(T);
        }

        /// <summary>
        /// Gets all components for an entity. Returns null if entity does not exist
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<IComponent> GetAllComponents(UID entity) {
            if (EntityExists(entity)) {
                return _entities[entity];
            } else {
                return null;
            }
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
                foreach(UID e in _entities.Keys) {
                    system.EntityModified(e);
                }
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
        /// Call whenever an entity is modified and systems need to update.
        /// </summary>
        /// <param name="entity"></param>
        public virtual void EntityModified(UID entity) {
            for(int i=0; i<_systems.Count; ++i) {
                _systems[i].EntityModified(entity);
            }
        }
    }
}