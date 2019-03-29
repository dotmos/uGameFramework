using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System.Linq;
using FlatBuffers;
using System.Text;
using Zenject;

namespace ECS {
    public class EntityManager : IEntityManager {

        /// <summary>
        /// Holds components of entities
        /// </summary>
        //This is super cache unfriendly.
        //TODO: Make cache friendly and do not use a list of components, but use a list of ids, targeting component arrays of same component type. I.e. one array per component type
        protected readonly Dictionary<UID, HashSet<IComponent>> _entities;
        private readonly HashSet<int> _entityIDs;

        /// <summary>
        /// List of all registered systems
        /// </summary>
        private readonly List<ISystem> _systems;
#if ECS_PROFILING && UNITY_EDITOR
        /// <summary>
        /// Stops the time for all services per frame
        /// </summary>
        private StringBuilder logTxtBuilder = new StringBuilder();
        private readonly System.Diagnostics.Stopwatch watchOverall = new System.Diagnostics.Stopwatch();
        private float timer = 0;
        private readonly float timerInterval = 1.0f;
        private double maxElapsedTime = 0;
        private int mediumTicks = 0;
        private int highTicks = 0;
        private int veryhighTicks = 0;
        public static bool showLog = false;
#endif

        private readonly Queue<int> _recycledEntityIds;
        private int _lastEntityId { get; set; }
        static readonly int _startEntityID = 1;

        private readonly Queue<int> _recycledComponentIds;
        private int _lastComponentId { get; set; }
        static readonly int _startComponentID = 1;

        bool applicationIsQuitting = false;

        bool isInitialized = false;

        /// <summary>
        /// If set to true, entities will auto register themselves to systems. If set to false, you have to manually call EntityModified/EntitiesModified
        /// </summary>
        public bool AutoCallEntityModified { get; set; } = true;

        [Inject] DisposableManager dManager;

        public EntityManager() {
            _entities = new Dictionary<UID, HashSet<IComponent>>();
            _entityIDs = new HashSet<int>();
            _systems = new List<ISystem>();
            _recycledEntityIds = new Queue<int>();
            _recycledComponentIds = new Queue<int>();
            _lastEntityId = _startEntityID;
            _lastComponentId = _startComponentID;
        }

        [Inject]
        void OnInject() {
            dManager.Add(this);
        }

        /// <summary>
        /// Initialize the entity manager
        /// </summary>
        public virtual void Initialize() {
            //Register systems
            foreach (ISystem s in RegisterSystemsOnStartup()) {
                RegisterSystem(s);
            }

            /*
            //Start processing systems every frame
            Observable.EveryUpdate().Subscribe(e => {
                foreach (ISystem s in _systems) {
                    s.ProcessSystem();
                }
            });
            */

            UnityEngine.Application.quitting += () => { applicationIsQuitting = true; };

            isInitialized = true;
        }

        /// <summary>
        /// Updates all systems for this frame
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void Tick(float deltaTime) {
            if (isInitialized) {
#if ECS_PROFILING && UNITY_EDITOR
                timer -= UnityEngine.Time.deltaTime;
                if (timer <= 0) {
                    timer = timerInterval;
                    showLog = true;
                }
                watchOverall.Restart();
#endif
                for (int i = 0; i < _systems.Count; ++i) {
                    _systems[i].ProcessSystem(deltaTime);
                }
#if ECS_PROFILING && UNITY_EDITOR
                watchOverall.Stop();
                var elapsedTime = watchOverall.Elapsed.TotalSeconds;
                if (elapsedTime > maxElapsedTime) {
                    maxElapsedTime = elapsedTime;
                }
                if (elapsedTime > 1) {
                    veryhighTicks++;
                } else if (elapsedTime > 0.1) {
                    highTicks++;
                } else if (elapsedTime > 0.016666) {
                    mediumTicks++;
                }
                if (showLog) {
                    logTxtBuilder.Clear();
                    logTxtBuilder.Append("------").Append(deltaTime).Append("-----\nECS-Tick:").Append(elapsedTime).Append("(max:").Append(maxElapsedTime).Append(" [>0.0166:").Append(mediumTicks).Append("|>0.1:").Append(highTicks)
                        .Append("|>1.0:").Append(veryhighTicks).Append("] System:");
                    UnityEngine.Debug.Log(logTxtBuilder.ToString());
                    showLog = false;
                };
                
#endif
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
            return CreateEntity(id);
        }

        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <returns></returns>
        protected UID CreateEntity(int id) {
            UID uid = new UID(id);
            //UnityEngine.Debug.Log(uid.ID);

            _entities.Add(uid, new HashSet<IComponent>());
            _entityIDs.Add(uid.ID);

            return uid;
        }

        /// <summary>
        /// Destroys the entity
        /// </summary>
        /// <param name="entityID"></param>
        public void DestroyEntity(ref UID entity) {
            if (applicationIsQuitting) return;

            if (EntityExists(entity)) {
                _entities[entity].Clear();
                _entityIDs.Remove(entity.ID);
                _EntityModified(entity);
                _entities[entity] = null;
                _entities.Remove(entity);
                _recycledEntityIds.Enqueue(entity.ID);
                entity.ID = 0; // make it NULL
            }
        }

        /// <summary>
        /// Destroy all entities
        /// </summary>
        public void DestroyAllEntities() {
            while (_entities.Count > 0) {
                var entity = _entities.ElementAt(0).Key;
                DestroyEntity(ref entity );
            }
        }

        /// <summary>
        /// Checks if the entity is alive/exists
        /// </summary>
        /// <param name="entityID"></param>
        /// <returns></returns>
        public bool EntityExists(UID entity) {
            //return _entities.ContainsKey(entity);
            if (entity.ID == 0) return false;

            return _entityIDs.Contains(entity.ID);
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

                if (_recycledComponentIds.Count > 0) {
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
            if (EntityExists(entity) && !HasComponent(entity, component.GetType())) {
                //component.Entity.SetID(entity.ID);
                component.Entity = entity;
                SetupComponentID(component);
                _entities[entity].Add(component);
                _EntityModified(entity);
                //UnityEngine.Debug.Log("Added component " + component.GetType() + " to entity:" + entity.ID);
                return component;
            }
            return null;
        }

        public IComponent AddComponent(UID entity, Type componentType) {
            if (EntityExists(entity)) {
                if (!HasComponent(entity, componentType)) {
                    IComponent component = CreateComponentFromType(componentType);
                    return AddComponent(entity, component);
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a shallow copy of the component
        /// </summary>
        /// <param name="componentToClone"></param>
        /// <returns></returns>
        public IComponent CloneComponent(IComponent componentToClone) {
            /*
            ConstructorInfo ctor = componentToClone.GetType().GetConstructor(System.Type.EmptyTypes);
            if (ctor != null) {
                object componentObject = ctor.Invoke(null);
                IComponent component = (IComponent)componentObject;
                //componentToClone.CopyFields(component);
                component.CopyValues(componentToClone);
                component.ID = new UID();
                SetupComponentID(component);
                return component;
            }
            */
            if(componentToClone != null) {
                IComponent newComponent = CreateComponentFromType(componentToClone.GetType());
                newComponent.CopyValues(componentToClone,true);
                newComponent.ID = new UID();
                SetupComponentID(newComponent);
                return newComponent;
            }

            return null;
        }

        IComponent CreateComponentFromType(Type componentType) {
            IComponent newComponent = (IComponent)Activator.CreateInstance(componentType);
            return newComponent;
        }

        /// <summary>
        /// Removes the component from the entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void RemoveComponent<T>(UID entity) where T : IComponent {
            if (applicationIsQuitting) return;

            RemoveComponent(entity, GetComponent<T>(entity));
        }
        public void RemoveComponent(UID entity, IComponent component) {
            if (applicationIsQuitting) return;

            if (component != null && EntityExists(entity) && HasComponent(entity, component)) {
                _entities[entity].Remove(component);
                component.Entity.SetID(-1);
                _EntityModified(entity);
            }
        }
        
        /// <summary>
        /// Disposes the component, freeing it's ID and calling Dispose()
        /// </summary>
        /// <param name="component"></param>
        public void DisposeComponent(IComponent component) {
            if (applicationIsQuitting) return;

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
                //IComponent c = _entities[entity].Find(o => o is T);

                //Hashset version. More gc friendly and less laggy, especially when creating LOTS of entities in one frame
                IComponent c = null;// 
                foreach(IComponent comp in _entities[entity]) {
                    if(comp is T) {
                        c = comp;
                        break;
                    }
                }
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
                return _entities[entity].ToList();
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
                //TODO: This is "slow" and produces garbage. Rethink how entities/components are stored.
                //IComponent c = _entities[entity].Find(o => o is T);

                //Hashset version. More gc friendly and less laggy, especially when creating LOTS of entities in one frame
                IComponent c = null;// 
                foreach (IComponent comp in _entities[entity]) {
                    if (comp is T) {
                        c = comp;
                        break;
                    }
                }
                if (c != null) {
                    return true;
                }
            }
            return false;
        }

        public bool HasComponent(UID entity, IComponent component) {
            if (EntityExists(entity)) {
                //TODO: This is "slow" and produces garbage. Rethink how entities/components are stored.
                //IComponent c = _entities[entity].Find(o => o == component);
                //if (c != null) {
                //    return true;
                //}

                //Hashset version. More gc friendly and less laggy, especially when creating LOTS of entities in one frame
                return _entities[entity].Contains(component);
            }
            return false;
        }

        public bool HasComponent(UID entity, Type componentType) {
            if (EntityExists(entity)) {
                IComponent c = null;// 
                foreach (IComponent comp in _entities[entity]) {
                    if (comp.GetType() == componentType) {
                        c = comp;
                        break;
                    }
                }
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
        void _EntityModified(UID entity) {
            if (AutoCallEntityModified) {
                EntityModified(entity);
            }
        }

        /// <summary>
        /// Call whenever an entity is modified and systems need to update.
        /// </summary>
        /// <param name="entity"></param>
        public virtual void EntityModified(UID entity) {
            for (int i = 0; i < _systems.Count; ++i) {
                _systems[i].EntityModified(entity);
            }
        }

        /// <summary>
        /// Manually tell manager to update all systems with given entities. No need to call this, if manager is set to autoCallEntityModified
        /// </summary>
        /// <param name="entities"></param>
        public virtual void EntitiesModified(List<UID> entities) {
            for(int i=0; i<entities.Count; ++i) {
                EntityModified(entities[i]);
            }
        }

        public void ResetIDs() {
            _recycledComponentIds.Clear();
            _recycledEntityIds.Clear();
            _lastComponentId = _startComponentID;
            _lastEntityId = _startEntityID;
        }

        public virtual int Serialize(FlatBufferBuilder builder) {
            UnityEngine.Debug.LogError("FLATBUFFER (DE)SERIALIZER NOT ACTIVATED! Implement (De)Serialize-Methods in your own");
            return 0;
        }

        public virtual void Deserialize(object incoming) {
            UnityEngine.Debug.LogError("FLATBUFFER (DE)SERIALIZER NOT ACTIVATED! Implement (De)Serialize-Methods in your own");
        }

        public virtual void Deserialize(ByteBuffer buf) {
            UnityEngine.Debug.LogError("FLATBUFFER (DE)SERIALIZER NOT ACTIVATED! Implement (De)Serialize-Methods in your own");
        }

        public void Dispose() {
            dManager.Remove(this);
            List<ISystem> systemsToRemove = new List<ISystem>(_systems);
            foreach(ISystem s in systemsToRemove) {
                UnregisterSystem(s);
                s.Dispose();
            }
            _systems.Clear();

            foreach(KeyValuePair<UID, HashSet<IComponent>> kv in _entities) {
                kv.Value.Clear();
            }
            _entities.Clear();

            _recycledEntityIds.Clear();
            _recycledComponentIds.Clear();
        }
    }
}