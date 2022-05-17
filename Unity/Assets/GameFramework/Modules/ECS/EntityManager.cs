using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System.Linq;
using FlatBuffers;
using System.Text;
using Zenject;
using Service.Serializer;

namespace ECS {
    public class EntityManager : DefaultSerializable2, IEntityManager {

        /// <summary>
        /// Holds components of entities
        /// </summary>
        //This is super cache unfriendly.
        //TODO: Make cache friendly and do not use a list of components, but use a list of ids, targeting component arrays of same component type. I.e. one array per component type
        protected Dictionary<UID, List<IComponent>> _entities;

        // TODO: do we need this? this hashset-functionality can be replaced by _entities.ContainsKey(...). Performance is the same (see UnitTestPerformance.cs TestCompareHashsetWithDictKey-Testcase)
        // private readonly HashSet<UID> _entityIDs;

        /// <summary>
        /// List of all registered systems
        /// </summary>
        protected readonly List<ISystem> _systems;
#if ECS_PROFILING
        /// <summary>
        /// Stops the time for all services per frame
        /// </summary>
        private StringBuilder logTxtBuilder = new StringBuilder();
        private readonly System.Diagnostics.Stopwatch watchOverall = new System.Diagnostics.Stopwatch();
        private readonly System.Diagnostics.Stopwatch secondWatch = new System.Diagnostics.Stopwatch();

        private double maxElapsedTime = 0;
        private int tickCounts = 0;
        private double avgElapsedTime = 0;
        private double avgSecond = 0;
        private int tickCountSecond = 0;
        private double _secondData = 0;
        private int mediumTicks = 0;
        private int highTicks = 0;
        private int veryhighTicks = 0;
        public static bool logEntityManager = false;

#endif

        private readonly Queue<UID> _recycledEntityIds;
        private int _lastEntityId { get; set; }
        static readonly int _startEntityID = 1;

        private readonly Queue<UID> _recycledComponentIds;
        private int _lastComponentId { get; set; }
        static readonly int _startComponentID = 1;

        bool applicationIsQuitting = false;

        protected bool isInitialized = false;

        /// <summary>
        /// If set to true, entities will auto register themselves to systems. If set to false, you have to manually call EntityModified/EntitiesModified
        /// </summary>
        public bool AutoCallEntityModified { get; set; } = true;

        [Inject] DisposableManager dManager;

        public EntityManager() {
            _entities = new Dictionary<UID, List<IComponent>>();
            //_entityIDs = new HashSet<UID>();
            _systems = new List<ISystem>();
            _recycledEntityIds = new Queue<UID>();
            _recycledComponentIds = new Queue<UID>();
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
        public virtual void Tick(float deltaTime,float unscaledTime,float systemScaled) {
            if (isInitialized) {
#if ECS_PROFILING
                watchOverall.Restart();
#endif
                int _systemCount = _systems.Count;
                for (int i = 0; i < _systemCount; ++i) {
                    try { _systems[i].ProcessSystem(deltaTime,unscaledTime,systemScaled); } catch (Exception e) { UnityEngine.Debug.LogException(e); }
                }
#if ECS_PROFILING
                watchOverall.Stop();
                if (logEntityManager) {

                    var elapsedTime = watchOverall.Elapsed.TotalMilliseconds;
                    _secondData += elapsedTime;
                    if (elapsedTime > maxElapsedTime) {
                        maxElapsedTime = elapsedTime;
                    }
                    if (tickCounts == 0) {
                        avgElapsedTime = elapsedTime;
                        tickCounts++;
                    } else {
                        avgElapsedTime = (avgElapsedTime * tickCounts + elapsedTime) / (tickCounts + 1);
                        tickCounts++;
                    }
                    if (elapsedTime > 1) {
                        veryhighTicks++;
                    } else if (elapsedTime > 0.1) {
                        highTicks++;
                    } else if (elapsedTime > 0.016666) {
                        mediumTicks++;
                    }

                    if (secondWatch.Elapsed.TotalSeconds > 1.0) {
                        if (tickCountSecond == 0) {
                            avgSecond = _secondData;
                        } else {
                            avgSecond = (avgSecond * tickCountSecond + _secondData) / (tickCountSecond+1);
                        }
                        tickCountSecond++;
                        _secondData = 0;
                        secondWatch.Restart();
                    }
                    if (!secondWatch.IsRunning) {
                        secondWatch.Restart();
                    }
                }
#endif
            }
        }

#if ECS_PROFILING
        public void ShowLog(bool showOnDevUIConsole = false) {

            foreach (var system in _systems.OrderByDescending(system => system.AvgElapsedTime)) {
                system.ShowLog(showOnDevUIConsole);
            }
            logTxtBuilder.Append("[calls:").Append(tickCounts).Append("]")
                .Append(" avg(sec):").Append(avgSecond)
                .Append(" avg(single):").Append(avgElapsedTime).Append(" max:").Append(maxElapsedTime).Append(" [>0.0166:").Append(mediumTicks).Append("|>0.1:").Append(highTicks)
                .Append("|>1.0:").Append(veryhighTicks).Append("] EntityManager").Append("\n\n");

            UnityEngine.Debug.Log(logTxtBuilder.ToString());
            if (showOnDevUIConsole) {
                Kernel.Instance.Resolve<Service.DevUIService.IDevUIService>().WriteToScriptingConsole(logTxtBuilder.ToString());
            }
            logTxtBuilder.Clear();

        }

        public void ResetLog() {
            avgElapsedTime = 0;
            maxElapsedTime = 0;
            mediumTicks = 0;
            highTicks = 0;
            veryhighTicks = 0;
            tickCounts = 0;
            avgElapsedTime = 0;

            foreach (var system in _systems) {
                system.ResetLog();
            }
        }

#endif

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
            int revision = 0;

            if (_lastEntityId + 1 >= int.MaxValue && _recycledEntityIds.Count == 0) {
                UnityEngine.Debug.LogError("WARNING: No free entity IDs! Restarting from 0. This is VERY unlikly to happen! There is a UID leak somewhere...");
                _lastEntityId = _startEntityID;
            }

            if (_recycledEntityIds.Count > 0) {
                UID  uid = _recycledEntityIds.Dequeue();
                id = uid.ID;
                revision = uid.Revision;
                if(revision+1 >= int.MaxValue) {
                    revision = 0;
                }
                revision++;
            } else {
                id = _lastEntityId;
                _lastEntityId++;
            }
            return CreateEntity(id, revision);
        }

        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <returns></returns>
        protected UID CreateEntity(int id, int revision) {
            UID uid = new UID(id, revision);
            //UnityEngine.Debug.Log(uid.ID);

            _entities.Add(uid, new List<IComponent>());
            //_entityIDs.Add(uid);

            return uid;
        }

        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <returns></returns>
        protected UID ThreadSafeCreateEntity(int id, int revision) {
            //UnityEngine.Debug.Log(uid.ID);
            lock (_entities) {
                UID uid = new UID(id, revision);
                _entities.Add(uid, new List<IComponent>());
                //_entityIDs.Add(uid);
                return uid;
            }
        }

        /// <summary>
        /// Destroys the entity
        /// </summary>
        /// <param name="entityID"></param>
        public void DestroyEntity(ref UID entity) {
            if (applicationIsQuitting) return;

            if (EntityExists(entity)) {
                //Dispose entity components
                while(_entities[entity].Count > 0) {
                    DisposeComponent(_entities[entity].First());
                }

                _entities[entity].Clear();
                //_entityIDs.Remove(entity);
                _EntityModified(entity);
                _entities[entity] = null;
                _entities.Remove(entity);
                _recycledEntityIds.Enqueue(entity);
                entity.SetNull(); // make it NULL
            }
        }

        ///// <summary>
        ///// Destroy all entities
        ///// </summary>
        //public void ClearAll() {
        //    _recycledEntityIds.Clear();
        //    _entities.Clear();
        //    _entityIDs.Clear();
        //    _lastEntityId = _startEntityID;
        //    _recycledComponentIds.Clear();
        //    _lastComponentId = _startComponentID;
        //    isInitialized = false;
        //    _systems.Clear();
        //}

        public int EntityCount() {
//            return _entityIDs.Count;
            return _entities.Count;
        }

        /// <summary>
        /// Checks if the entity is alive/exists
        /// </summary>
        /// <param name="entityID"></param>
        /// <returns></returns>
        public bool EntityExists(UID entity) {
            //return _entities.ContainsKey(entity);
            if (entity.ID == 0) return false;

//            return _entityIDs.Contains(entity);
            return _entities.ContainsKey(entity);
        }

        /// <summary>
        /// Tries to find an entity for the supplied ID. This funtion is VERY slow!
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UID? GetEntityForID_SLOW(int id) {
//            foreach (UID uid in _entityIDs) {
            foreach (UID uid in _entities.Keys) {
                    if (uid.ID == id) return uid;
            }
            return null;
        }

        /// <summary>
        /// Give the component a valid ID. Does nothing if component already has an id.
        /// </summary>
        /// <param name="component"></param>
        public void SetupComponentID(IComponent component) {
            if (component.ID.IsNull()) {

                int id = -1;
                int revision = 0;

                if (_lastComponentId + 1 >= int.MaxValue && _recycledComponentIds.Count == 0) {
                    //UnityEngine.Debug.LogError("WARNING: No free entity IDs! Restarting from 0. This should not happen!");
                    _lastComponentId = _startComponentID;
                }

                if (_recycledComponentIds.Count > 0) {
                    UID uid = _recycledComponentIds.Dequeue();
                    id = uid.ID;
                    revision = uid.Revision;
                    if(revision+1 > int.MaxValue) {
                        revision = 0;
                    }
                    revision++;
                }
                else {
                    id = _lastComponentId;
                    _lastComponentId++;
                }

                component.ID = new UID(id, revision);
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
        /// Set the entities component for this type. A component of this type already added to this entity
        /// will be removed beforehand
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        public IComponent SetComponent(UID entity, IComponent component) {
            if (_entities.TryGetValue(entity, out List<IComponent> comps)) {
                RemoveComponent(entity,component);

                if (component != null) {
                    component.Entity = entity;
                    SetupComponentID(component);
                    comps.Add(component);
                }
            }
            //component.Entity.SetID(entity.ID);
            _EntityModified(entity);
            //UnityEngine.Debug.Log("Added component " + component.GetType() + " to entity:" + entity.ID);
            return component;
        }

        public IComponent ThreadSafeSetComponent(UID entity, IComponent component) {
            lock (_entities) {
                return SetComponent(entity, component);
            }
        }


        public IComponent SetComponent<T>(UID entity, T component) where T : IComponent  {
            if (_entities.TryGetValue(entity, out List<IComponent> comps)) {
                RemoveComponent<T>(entity);


                if (component != null) {
                    component.Entity = entity;
                    SetupComponentID(component);
                    comps.Add(component);
                }
            }
            //component.Entity.SetID(entity.ID);
            _EntityModified(entity);
            //UnityEngine.Debug.Log("Added component " + component.GetType() + " to entity:" + entity.ID);
            return component;
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

            Type type = typeof(T);
            RemoveComponent(entity, type);
        }
        public void RemoveComponent(UID entity, IComponent component) {
            if (applicationIsQuitting) return;

            RemoveComponent(entity, component.GetType());
        }

        public void RemoveComponent(UID entity, Type componentType) {
            if (applicationIsQuitting) return;

            if (_entities.TryGetValue(entity,out List<IComponent> comps)) {
                for (int i = comps.Count - 1; i >= 0; i--) {
                    if (comps[i].GetType() == componentType) {
                        comps.RemoveAt(i);
                        _EntityModified(entity);
                        return;
                    }
                }
            }
        }
        
        /// <summary>
        /// Disposes the component, freeing it's ID and calling Dispose()
        /// </summary>
        /// <param name="component"></param>
        public void DisposeComponent(IComponent component) {
            if (applicationIsQuitting) return;

            RemoveComponent(component.Entity, component);
            _recycledComponentIds.Enqueue(component.ID);
            component.Dispose();
            component.ID.SetNull();
            //component.Entity = UID.NULL; //NOTE: Do not set entity to NULL. The way things are set-up right now, this might break ISystem.OnUnregistered() since components are still available, but entity is not set.
        }

        /// <summary>
        /// Gets the component from the entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T GetComponent<T>(UID entity) where T : IComponent{
            if (_entities.TryGetValue(entity,out List<IComponent> components)) {
                int _componentsCount = components.Count;
                Type t = typeof(T);
                for(int i=0; i< _componentsCount; ++i){
                    IComponent comp = components[i];
                    if (comp.GetType() == t) {
                        return (T)comp;
                    }
                }
            }

            return default;
        }

        public T GetComponentTEST<T>(UID entity) where T : IComponent {
            if (EntityExists(entity)) {
                List<IComponent> components = _entities[entity];

                int _componentsCount = components.Count;
                Type t = typeof(T);
                for (int i = 0; i < _componentsCount; ++i) {

                    IComponent comp = components[i];
                    // ~3 ms
                    if (comp.GetType() == t) {
                        return (T)comp;
                    }
                    

                    /*
                    // ~8 ms
                    if(comp is T) {
                        return (T)comp;
                    }

                    // ~8 ms
                    if (comp is T castedComp) {
                        return castedComp;
                    }
                    */

                }

            }

            return default;
        }


        /// <summary>
        /// Get component for this entity by type
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="componentType"></param>
        /// <returns></returns>
        public IComponent GetComponent(UID entity,Type componentType) {
            if (_entities.TryGetValue(entity, out List<IComponent> components)) {
                for (int i = 0, _componentsCount = components.Count; i < _componentsCount; ++i) {
                    IComponent comp = components[i];
                    if (comp.GetType() == componentType) {
                        return comp;
                    }
                }
            }

            return null;
        }

        public object ThreadSafeGetComponent(UID entity, Type componentType) {
            if (EntityExists(entity)) {
                IComponent c = null;// 
                List<IComponent> entityComponents = null;
                lock (_entities) {
                    entityComponents = _entities[entity];
                }
                for(int i=0; i<entityComponents.Count; ++i){
                    IComponent comp = entityComponents[i];
                    if (comp.GetType() == componentType) {
                        c = comp;
                        break;
                    }
                }
                if (c != null) {
                    return c;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all components for an entity. Returns null if entity does not exist
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<IComponent> GetAllComponents(UID entity) {
            if (_entities.TryGetValue(entity,out List<IComponent> comps)) {
                return comps.ToList();
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
            return HasComponent(entity, typeof(T));
        }

        public bool HasComponent(UID entity, IComponent component) {
            return HasComponent(entity,component.GetType());
        }

        public bool HasComponent(UID entity, Type componentType) {
            // TODO: HashSet based, better?
            if (_entities.TryGetValue(entity, out List<IComponent> comps)) {
                for (int i = comps.Count - 1; i >= 0; i--) {
                    if (comps[i].GetType() == componentType) {
                        return true;
                    }
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
                system.RemoveAllEntities();
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
            int _entityCount = entities.Count;
            for(int i=0; i<_entityCount; ++i) {
                EntityModified(entities[i]);
            }
        }



        public virtual int Serialize(FlatBufferBuilder builder) {

            VectorOffset _recycledUIDOffset = FlatBufferSerializer.CreateManualList<UID>(builder, _recycledEntityIds.ToList());
            VectorOffset _recycledComponentIDOffset = FlatBufferSerializer.CreateManualList<UID>(builder, _recycledComponentIds.ToList());
            builder.StartTable(6);
            builder.AddInt(0, _lastEntityId, 0);
            builder.AddInt(1, _lastComponentId, 0);
            builder.AddInt(2, _recycledEntityIds.Count, 0);
            builder.AddOffset(3, _recycledUIDOffset.Value, 0);
            builder.AddInt(4, _recycledComponentIds.Count, 0);
            builder.AddOffset(5, _recycledComponentIDOffset.Value, 0);
            

            return builder.EndTable();
        }

        public override void Ser2CreateTable(SerializationContext ctx, FlatBufferBuilder builder) {
            base.Ser2CreateTable(ctx, builder);
            builder.StartTable(4);
            builder.AddInt(0, _lastEntityId, 0);
            builder.AddInt(1, _lastComponentId, 0);
            ctx.AddReferenceOffset(2, new List<UID>(_recycledEntityIds));
            ctx.AddReferenceOffset(3, new List<UID>(_recycledComponentIds));
            int tblPos = builder.EndTable();
            ser2table = new ExtendedTable(tblPos, builder);
            //ctx.AddReferenceOffset(2, _recycledEntityIds);
            //ctx.AddReferenceOffset(3, _recycledComponentIds);
        }

        public override void Ser2Deserialize(int tblOffset, DeserializationContext ctx) {
            base.Ser2Deserialize(tblOffset, ctx);
            
            _lastEntityId = ser2table.GetInt(0);
            _lastComponentId = ser2table.GetInt(1);
            UID[] tempUIDs = null;
            ser2table.GetStructArray<UID>(2,ref tempUIDs);
            if (tempUIDs != null) {
                for (int i = 0; i < tempUIDs.Length; i++) {
                    _recycledEntityIds.Enqueue(tempUIDs[i]);
                }
            }
            ser2table.GetStructArray<UID>(3, ref tempUIDs);
            if (tempUIDs != null) {
                for (int i = 0; i < tempUIDs.Length; i++) {
                    _recycledComponentIds.Enqueue(tempUIDs[i]);
                }
            }
        }

        public virtual void Deserialize(object incoming) {
            if (incoming == null){
                return;
            }

            Clear();

            FBManualObject manual = FlatBufferSerializer.GetManualObject(incoming);
            _lastEntityId = manual.GetInt(0);
            _lastComponentId = manual.GetInt(1);

            // recycled entityIDS
            //IList<UID> recycledIds = manual.GetPrimitiveList<UID>(2);
            //((List<UID>)recycledIds).ForEach(o => _recycledEntityIds.Enqueue(o));
            int recycledIdsCount = manual.GetInt(2);
            List<object> tempList = FlatBufferSerializer.poolListObject.GetList(recycledIdsCount);
            for (int i = 0; i < recycledIdsCount; i++) tempList.Add(manual.GetListElemAt<Serial.FBUID>(3,i));
            List<UID> recycledIds = (List<UID>)FlatBufferSerializer.DeserializeList<UID, Serial.FBUID>(3, recycledIdsCount, tempList, null, false);
            recycledIds.ForEach(o => _recycledEntityIds.Enqueue(o));
            FlatBufferSerializer.poolListObject.Release(tempList);

            // recycled componentIDs
            //recycledIds = manual.GetPrimitiveList<UID>(5);
            //((List<UID>)recycledIds).ForEach(o => _recycledComponentIds.Enqueue(o));
            recycledIdsCount = manual.GetInt(4);
            tempList = FlatBufferSerializer.poolListObject.GetList(recycledIdsCount);
            int listLength = manual.GetListLength(5);
            for (int i = 0; i < recycledIdsCount; i++) tempList.Add(manual.GetListElemAt<Serial.FBUID>(5, i));
            
            recycledIds = (List<UID>)FlatBufferSerializer.DeserializeList<UID, Serial.FBUID>(5, recycledIdsCount, tempList, null, false);
            recycledIds.ForEach(o => _recycledComponentIds.Enqueue(o));
            FlatBufferSerializer.poolListObject.Release(tempList);
        }

        public virtual void Deserialize(ByteBuffer buf) {
            UnityEngine.Debug.LogError("FLATBUFFER (DE)SERIALIZER NOT ACTIVATED! Implement (De)Serialize-Methods in your own");
        }

        public void Dispose() {
            dManager.Remove(this);
            Clear();
        }

        public void Clear() {
            List<ISystem> systemsToRemove = new List<ISystem>(_systems);
            foreach(ISystem s in systemsToRemove) {
                UnregisterSystem(s);
                s.Dispose();
            }
            _systems.Clear();

            foreach(KeyValuePair<UID, List<IComponent>> kv in _entities) {
                kv.Value.Clear();
            }
            _entities.Clear();
            //_entityIDs.Clear();
            _lastComponentId = _startComponentID;
            _lastEntityId = _startEntityID;
            _recycledEntityIds.Clear();
            _recycledComponentIds.Clear();
        }

    }
}