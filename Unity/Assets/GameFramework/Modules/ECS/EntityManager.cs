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
        /// Lookup table for systems to fetch all entities to process.
        /// </summary>
        Dictionary<ISystem, List<UID>> _systemToEntity = new Dictionary<ISystem, List<UID>>();

        /// <summary>
        /// Lookup table for systems to fetch all relevant components.
        /// Components are stored as a list of entity components.
        /// I.e. entities have ComponentA & ComponentB that are needed by the system. List looks like this: Entity1.ComponentA, Entity1.ComponentB, Entity2.ComponentA, Entity2.ComponentB, etc.
        /// </summary>
        Dictionary<ISystem, List<IComponent>> _systemToComponents = new Dictionary<ISystem, List<IComponent>>();

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
        public void DestroyEntity(UID entity) {
            if (EntityExists(entity)) {
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
            if (EntityExists(entity)) {
                return (T)AddComponent(entity, new T());
            }
            return default(T);
        }
        public IComponent AddComponent(UID entity, IComponent component) {
            if (EntityExists(entity)) {
                _entities[entity].Add(component);
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
        public bool HasComponent<T>(UID entity) where T: IComponent {
            //TODO: This is very slow. Rethink how entities are stored.
            return GetComponent<T>(entity) != null ? true : false;
        }


        /// <summary>
        /// Register a system with the entity manager
        /// </summary>
        /// <param name="system"></param>
        public void RegisterSystem(ISystem system) {
            if (!_systems.Contains(system)) {
                _systems.Add(system);
                _systemToEntity.Add(system, new List<UID>());
                _systemToComponents.Add(system, new List<IComponent>());
            }
        }

        /// <summary>
        /// Unregister a system from the entity manager
        /// </summary>
        /// <param name="system"></param>
        public void UnregisterSystem(ISystem system) {
            if (_systems.Contains(system)) {
                _systems.Remove(system);
                if (_systemToEntity.ContainsKey(system)) {
                    _systemToEntity[system].Clear();
                    _systemToEntity[system] = null;
                    _systemToEntity.Remove(system);
                }
                _systemToComponents[system].Clear();
                _systemToComponents[system] = null;
                _systemToComponents.Remove(system);
            }
        }

        /// <summary>
        /// Registers an entity with the system
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="system"></param>
        public void RegisterEntityWithSystem(UID entity, ISystem system) {
            if (!_systemToEntity[system].Contains(entity)) {
                //Cache entity in _systemToEntity
                _systemToEntity[system].Add(entity);

                //Get all needed components for the system from the entity
                List<IComponent> components = system.GetNeededComponents(entity);
                //Cache components in _systemToComponents
                _systemToComponents[system].AddRange(components);
            }
        }

        /// <summary>
        /// Unregisters an entity from the system
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="system"></param>
        public void UnregisterEntityFromSystem(UID entity, ISystem system) {
            if (_systemToEntity[system].Contains(entity)) {
                //Remove cached components from _systemToComponents
                List<IComponent> components = _systemToComponents[system].FindAll(o => o.Entity.ID == entity.ID);
                foreach(IComponent c in components) {
                    _systemToComponents[system].Remove(c);
                }

                //Remove entity from _systemToEntitiy
                _systemToEntity[system].Remove(entity);
            }
        }
    }
}