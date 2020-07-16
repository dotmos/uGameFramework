#define FLATBUFFER_CHECK_TYPES

using System.Collections.Generic;
using System;
using FlatBuffers;
using System.Linq;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using ECS;
using UnityEngine;

namespace Service.Serializer
{
    
    public interface IFBSerializeAsTypedObject { }
    public interface IFB2Context { 
        bool IsValid();
        void Invalidate();
    }
    public class Type2IntMapper : DefaultSerializable2
    {
        public static Type2IntMapper instance=new Type2IntMapper();
        
        private StringBuilder stb = new StringBuilder();

        private String GetTypeName(Type type) {
            stb.Clear();
            string assemblyName = type.Assembly.FullName;
            stb.Append(type.FullName).Append(", ").Append(assemblyName.Substring(0, assemblyName.IndexOf(',')));
            return stb.ToString();
        }

        private Type2IntMapper() { }

        Dictionary<int, Type> id2type  = new Dictionary<int, Type>();
        Dictionary<Type, int> type2id = new Dictionary<Type, int>();
        Dictionary<int, String> id2typeAsString = new Dictionary<int, string>();


        const int initialCount = 100;

        int idCounter = initialCount;

        private object lock_addType = new object();

        public void Clear() {
            ser2table = ExtendedTable.NULL;
            id2type.Clear();
            type2id.Clear();
            id2typeAsString.Clear();
        }

        public Type GetTypeFromId(int id) {
            if (id2type.TryGetValue(id,out Type type)) {
                return type;
            }
            throw new ArgumentException($"no type with id:{id} assigned");
        }

        public int GetIdFromType(Type type) {
            if (type2id.TryGetValue(type, out int typeId)) { 
                return typeId;
            }

            lock (lock_addType) {
                if (type2id.TryGetValue(type, out typeId)) { // double check for case where while waiting from lock the wanted type was created.
                    return typeId;
                }
                int id = idCounter++;
                type2id[type] = id;
                id2type[id] = type;
                id2typeAsString[id] = GetTypeName(type);
                return id;
            }
        }

        public override void Ser2CreateTable(SerializationContext sctx, FlatBufferBuilder builder) {
            base.Ser2CreateTable(sctx, builder);
            
            int pos = builder.CreateIDictionary(id2typeAsString, sctx);
            ser2table = new ExtendedTable(pos, builder);
            sctx.ResolveLateReferences();
        }

        //public override void Ser2Deserialize(int tblOffset, DeserializationContext dctx) {
        //    base.Ser2Deserialize(tblOffset, dctx);
        //    DeserializeFromOffset(base.ser2table.__tbl.bb_pos, dctx, true);
        //}

        public void DeserializeFromOffset(int offset, DeserializationContext dctx, bool isDirectBuffer = true) {
            type2id = new Dictionary<Type, int>();
            id2type = new Dictionary<int, Type>();
            
            id2typeAsString = (Dictionary<int, String>)ser2table.GetDictionaryFromOffset(offset, id2typeAsString, dctx,true);
            // TODO: do this in postprocess
            if (id2typeAsString == null) return;

            foreach (var kv in id2typeAsString) {
                Type type = Type.GetType(kv.Value);
                id2type[kv.Key] = type;
                type2id[type] = kv.Key;
            }
            idCounter = id2typeAsString.Count + 100;
        }
    }

    public interface IFBSerializable2  {
        /// <summary>
        /// entry point for serialization
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        int Ser2Serialize(SerializationContext ctx);

        /// <summary>
        /// serialize the current dataset. Override this (this equals to Serialization1's Serialize(..)-call).
        /// CAUTION: you need to finish by creating an ExtendedTable like this:
        ///    int tblPos = builder.EndTable();
        ///    ser2table = new ExtendedTable(tblPos, builder);
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="builder"></param>
        void Ser2CreateTable(SerializationContext ctx, FlatBuffers.FlatBufferBuilder builder);

        /// <summary>
        /// Update the current dataset (unused for now)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="builder"></param>
        void Ser2UpdateTable(SerializationContext ctx, FlatBuffers.FlatBufferBuilder builder);

        /// <summary>
        /// deserialize an object at the given tblOffset. override this. (this equals to Serialization2's Deserialize(..)-call).
        /// </summary>
        /// <param name="tblOffset"></param>
        /// <param name="ctx"></param>
        void Ser2Deserialize(int tblOffset, DeserializationContext ctx);
        //void Ser2Deserialize(DeserializationContext ctx);

        /// <summary>
        /// Cleanup context
        /// </summary>
        void Ser2Clear();
        /// <summary>
        /// Serialization-Flags (unused atm)
        /// </summary>
        int Ser2Flags { get; set; }

        /// <summary>
        /// Context (Serialization/Deserialization) this object was processed with (needed especially for multithreads serialization)
        /// </summary>
        IFB2Context Ser2Context { get; set; }
        /// <summary>
        /// Check for valid context. Contexts gets invalidated after loading/saving
        /// </summary>
        bool Ser2HasValidContext { get; }
        /// <summary>
        /// The current ser2table which points to the position in the buffer this object and provides deserialization functions (comparable to ManualObject)
        /// </summary>
        ExtendedTable Ser2Table { get; }
        bool Ser2HasOffset { get; }
        int Ser2Offset { get; set; }
    }

    public class Serializer2Flags {
        public const int FLAG_DIRTY = 1;
        public const int FLAG_DONT_SERIALIZE = 2;
    };

    public interface IFBSerializable2Struct
    {
        int Put(FlatBuffers.FlatBufferBuilder builder, bool prepare=true);
        void Get(ExtendedTable table, int fbPos);

        int ByteSize { get; }
    }

    public class DefaultSerializable2 : IFBSerializable2
    {
        [JsonIgnore]
        public ExtendedTable ser2table = ExtendedTable.NULL;

        [JsonIgnore]
        public ExtendedTable Ser2Table => ser2table;

        [JsonIgnore]
        public bool Ser2Flags { get; set; }

        [JsonIgnore]
        public bool Ser2HasOffset => Ser2HasValidContext && !ser2table.IsNULL() && ser2table.bb!=null;

        [JsonIgnore]
        public int Ser2Offset { get => ser2table.offset; set => ser2table.offset = value; }

        [JsonIgnore]
        int IFBSerializable2.Ser2Flags { get; set; }
        [JsonIgnore]
        public IFB2Context Ser2Context { get; set; }
        
        [JsonIgnore]
        public bool Ser2HasValidContext => Ser2Context != null && Ser2Context.IsValid();

        [JsonIgnore]
        private object lock_state = new object();


        public virtual int Ser2Serialize(SerializationContext ctx) {
#if TESTING
            if (Ser2HasOffset && Ser2HasValidContext) {
                if (Ser2Context == this) {
                    UnityEngine.Debug.LogError($"Ser2Serialize called for {GetType()} but it was already serialized by this sctx");
                } else {
                    UnityEngine.Debug.LogError($"Ser2Serialize called for {GetType()} but it was already serialized by another context");
                }
            }
#endif
            Ser2CreateTable(ctx, ctx.builder);

            // update-mechnanism not implemented at the moment
            //if (!Ser2HasOffset) {
            //    Ser2CreateTable(ctx, ctx.builder);
            //} else {
            //    Ser2UpdateTable(ctx, ctx.builder);
            //}
            return ser2table.offset;
        }

        public virtual void Ser2CreateTable(SerializationContext ctx, FlatBufferBuilder builder) { }
        public virtual void Ser2UpdateTable(SerializationContext ctx, FlatBufferBuilder builder) { }
        public virtual void Ser2Deserialize(int tblOffset, DeserializationContext ctx) {
            ser2table = new ExtendedTable(tblOffset, ctx.bb);
        }
        protected void SetTable(int tblOffset, DeserializationContext dctx) {
            ser2table = new ExtendedTable(tblOffset, dctx.bb);
        }
        protected void SetTable(int tblOffset, ByteBuffer bb) {
            ser2table = new ExtendedTable(tblOffset, bb);
        }


        public virtual void Ser2Clear() {
            ser2table = ExtendedTable.NULL;
        }
    }

    public class DeserializationContext : IFB2Context
    {
        public static int current_savegame_dataformat = 0;
        
        private readonly Dictionary<int, object> pos2obj = new Dictionary<int, object>();
        private readonly Queue<IFBPostDeserialization> postDeserializations = new Queue<IFBPostDeserialization>();

        private readonly Dictionary<int, Queue<object>> postDeserializeQueues = new Dictionary<int, Queue<object>>();
        private readonly Type typeObject = typeof(object);

        public ByteBuffer bb;

        private ExtendedTable extTbl; // make helper calls available. (obviously only offset-based calls makes sense here)
        private bool isValid = true;

        public bool IsValid() {
            return isValid;
        }

        public void Invalidate() {
            isValid = false;
        }

        public DeserializationContext(ByteBuffer bb) {
            this.bb = bb;
            extTbl = new ExtendedTable(0, bb);
        }

        private void ClearTables() {
            foreach (KeyValuePair<int, object> kv in pos2obj) {
                if (kv.Value is IFBSerializable2) {
                    ((IFBSerializable2)kv.Value).Ser2Clear();
                }
            }
            Type2IntMapper.instance.Ser2Clear();
        }

        public void AddToPostProcessObjects(int queueNr,object uid) {
            if (postDeserializeQueues.TryGetValue(queueNr, out Queue<object> queue)) {
                queue.Enqueue(uid);
            } else {
                Queue <object> newQueue = new Queue<object>();
                postDeserializeQueues[queueNr] = newQueue;
                newQueue.Enqueue(uid);
            }
        }

        public Queue<object> GetPostProcessUIDQueue(int queueNr) {
            if (postDeserializeQueues.TryGetValue(queueNr,out Queue<object> queue)){
                return queue;
            }

            return null;
        }

        public void RemovePostProcessUIDQueue(int queueNr) {
            postDeserializeQueues.Remove(queueNr);
        }

        public void AddOnPostDeserializationObject(IFBPostDeserialization obj) {
            postDeserializations.Enqueue(obj);
        }

        public void ProcessPostSerialization(IEntityManager em,int fileDataFormat,int currentDataFormat) { 
            while (postDeserializations.Count > 0) {
                var obj = postDeserializations.Dequeue();
                obj.OnPostDeserialization(em, this, fileDataFormat, currentDataFormat,true);
            }
        }

        public DeserializationContext(byte[] buf, bool readTypes=true) {
            this.bb = new ByteBuffer(buf);
#if TESTING
            ByteBuffer.__debug_bb = this.bb;
#endif
            
            extTbl = new ExtendedTable(0, bb);
            if (readTypes) {
                Type2IntMapper.instance.ser2table = new ExtendedTable(4, bb);
                int typeDataAddress = Type2IntMapper.instance.ser2table.__tbl.__indirect(4);
                Type2IntMapper.instance.DeserializeFromOffset(typeDataAddress, this, true);

            }
        }

        public T GetOrCreate<T>(int bufferOffset, ref T obj) where T: new() {
            if (bufferOffset == 0) {
                obj = default(T);
                return obj;
            }

            T cachedObject = _GetCachedObject<T>(bufferOffset);
            if (cachedObject != null) {
                obj = cachedObject;
                return cachedObject;
            }
            obj = (T)_GetOrCreate(bufferOffset, typeof(T), obj);
            return obj;
        }

        public T GetOrCreate<T>(int bufferOffset, object obj = null) where T : IFBSerializable2 {
            return (T)GetOrCreate(bufferOffset, typeof(T), obj);
        }
        
        public object GetOrCreate(int bufferOffset, Type objectType,object obj=null) {
            if(obj == null) {
                object cachedObject = _GetCachedObject(bufferOffset, objectType);
                if (cachedObject != null) {
                    return cachedObject;
                }
            }
            
            object newObject = _GetOrCreate(bufferOffset, objectType, obj);
            return newObject;
        }

        private T _GetCachedObject<T>(int bufferOffset) {
            if (bufferOffset == 0) return default(T);

            if (pos2obj.TryGetValue(bufferOffset, out object result)) {
                return (T)result;
            }
            return default(T);
        }
        private object _GetCachedObject(int bufferOffset, Type objType) {
            if (bufferOffset == 0) return null;

            if (pos2obj.TryGetValue(bufferOffset, out object result)) {
#if FLATBUFFER_CHECK_TYPES
                if (objType!=typeObject && result.GetType() != objType && !ExtendedTable.typeISerializeAsTypedObject.IsAssignableFrom(objType)) {
                    UnityEngine.Debug.LogError($"Got unexpected type from cached object! expected:{objType} in_cache:{result.GetType()} at offset:{bufferOffset}");
                }                
#endif
                return result;
            }
            return null;
        }

        public void AddToCache(int pos,object obj) {
            pos2obj[pos] = obj;
        }


        private object _GetOrCreate(int bufferOffset, Type objType, object obj=null) {
            if (obj != null) {
                objType = obj.GetType();
            }
            if (ExtendedTable.typeIFBSerializable2.IsAssignableFrom(objType)) {
                //if (ExtendedTable.typeISerializeAsTypedObject.IsAssignableFrom(objType)) {
                //    int typeId = bb.GetInt(bufferOffset + 4);
                //    objType = Type2IntMapper.instance.GetTypeFromId(typeId);
                //}
                var newIFBSer2obj = (IFBSerializable2)obj ?? (IFBSerializable2)Activator.CreateInstance(objType);
                pos2obj[bufferOffset] = newIFBSer2obj;
                newIFBSer2obj.Ser2Deserialize(bufferOffset, this);
                if (newIFBSer2obj is IFBPostDeserialization) {
                    AddOnPostDeserializationObject((IFBPostDeserialization)newIFBSer2obj);
                }
                return newIFBSer2obj;
            } else if (ExtendedTable.typeIList.IsAssignableFrom(objType)) {
                var newList = (IList)obj ?? (IList)Activator.CreateInstance(objType);
                pos2obj[bufferOffset] = newList;
                newList = extTbl.GetListFromOffset(bufferOffset, objType, this, newList, false);
                return newList;
            } else if (ExtendedTable.typeIDictionary.IsAssignableFrom(objType)) {
                var newDict = (IDictionary)obj ?? (IDictionary)Activator.CreateInstance(objType);
                pos2obj[bufferOffset] = newDict;
                newDict = extTbl.GetDictionaryFromOffset(bufferOffset, newDict, this, false);
                return newDict;
            } else if (objType == ExtendedTable.typeString) {
                string stringData = extTbl.GetStringFromOffset(bufferOffset);
                pos2obj[bufferOffset] = stringData;
                return stringData;
            } else {
                UnityEngine.Debug.LogError($"Deserializer: GetOrCreate of type({objType}) not supported!");
                return null;
            }

        }



        //public T GetOrCreate<T>(int bufferOffset,Type type) where T : IFBSerializable2 {
        //    if (bufferOffset == 0) return default(T);

        //    if (pos2obj.TryGetValue(bufferOffset, out IFBSerializable2 result)) {
        //        return (T)result;
        //    } else {
        //        var newObject = (T)Activator.CreateInstance(type);
        //        // todo: security-checks? 
        //        pos2obj[bufferOffset] = newObject;
        //        newObject.Ser2Deserialize(bufferOffset, this);
        //        return newObject;
        //    }
        //}

        public object GetReferenceByType(int bufferOffset, Type objType, object obj=null) {
            //var dctxType = typeof(DeserializationContext);
            //var method = dctxType.GetMethod("GetReference");
            //var genMethod = method.MakeGenericMethod(objType);
            //var result = genMethod.Invoke(this, new object[] { bufferOffset,obj });
            if (bufferOffset == 0) return null;

            if (ExtendedTable.typeObservableDict.IsAssignableFrom(objType)) {
                IObservableDictionary dict = (IObservableDictionary)obj;
                if (dict == null) {
                    dict =(IObservableDictionary) Activator.CreateInstance(objType);
                } else {
                    dict.InnerIDict.Clear();
                }
                var inner = GetOrCreate(bufferOffset, dict.InnerIDict.GetType() , dict.InnerIDict);
                return obj;
            }
            else if (ExtendedTable.typeObservableList.IsAssignableFrom(objType)) {
                IObservableList obsList = (IObservableList)obj;
                if (obsList == null) {
                    obsList = (IObservableList)Activator.CreateInstance(objType);
                } else {
                    obsList.InnerIList.Clear();
                }
                var inner = GetOrCreate(bufferOffset, obsList.InnerIList.GetType(), obsList.InnerIList);
                return obsList;
            }

            obj = GetOrCreate(bufferOffset, objType, obj);

            return obj;
        }

        public ObservableList<T> GetReference<T>(int bufferOffset, ref ObservableList<T> obj) {
            if (bufferOffset == 0) {
                return null;
            }

            if (obj == null) {
                obj = new ObservableList<T>();
            } else {
                obj.Clear();
            }

            obj = (ObservableList <T>)GetReferenceByType(bufferOffset, typeof(ObservableList<T>), obj);
            return obj;
        }

        public ObservableDictionary<TKey,TValue> GetReference<TKey,TValue>(int bufferOffset, ref ObservableDictionary<TKey,TValue> obj)  {
            if (bufferOffset == 0) {
                return null;
            }

            if (obj == null) {
                obj = new ObservableDictionary<TKey, TValue>();
            } else {
                obj.Clear();
            }

            obj = (ObservableDictionary<TKey, TValue>)GetReferenceByType(bufferOffset, typeof(ObservableDictionary<TKey, TValue>), obj);
            return obj;
        }


        public T GetReference<T>(int bufferOffset, ref T obj) where T :  new() {
            // TODO: white/black-listing...
            if (bufferOffset == 0) {
                return default(T);
            }

            obj = (T)GetOrCreate(bufferOffset,typeof(T),obj);
            return obj;
        }

        public T GetReference<T>(int bufferOffset,object obj=null) {
            if (bufferOffset == 0) {
                return default;
            }

            T result = (T)GetOrCreate(bufferOffset, typeof(T), obj);
            return result;
        }

         
        public T GetRoot<T>(T obj=default) where T : class,IFBSerializable2, new() {
            int offset = GetRootOffset();
            T data = obj ?? new T();
            return GetOrCreate<T>(offset,ref data);
        }

        public int GetRootOffset() {
            int offset = bb.Length - bb.GetInt(bb.Position) + bb.Position;
            return offset;
        }

        public ExtendedTable GetRootTable() {
            int offset = GetRootOffset();
            ExtendedTable tbl = new ExtendedTable(offset, bb);
            return tbl;
        }

        public void Cleanup() {
            ClearTables();
            pos2obj.Clear();
            Invalidate();
        }
    }










    public class SerializationContext : IFB2Context
    {
#if TESTING
//        Service.PerformanceTest.IPerformanceTestService perfTest;
        Dictionary<Type, int> lateRefCalls = new Dictionary<Type, int>();
        String debugOutput = "";

        public void AddRefCall(object obj) {
            lateRefCalls.TryGetValue(obj.GetType(), out int amount);
            lateRefCalls[obj.GetType()] = ++amount;
        }

        private void OutputDebugInfo(string top="") {
            StringBuilder stb = new StringBuilder();
            int all = 0;
            var keys = lateRefCalls.Keys.OrderBy(a => a.ToString());
            foreach (var key in keys) {
                var value = lateRefCalls[key];
                all += value;
                stb.Append($"{key}=>{value}\n");
            }
            stb.Append($"\n {debugOutput}\n");
            //stb.Append($"\n {perfTest.PerfTestOutputAsString()}");
            stb.Insert(0, $"all calls:{all}\n\n");
            stb.Insert(0, $"{top}\n");
            var fs = Kernel.Instance.Container.Resolve<Service.FileSystem.IFileSystemService>();

            fs.WriteStringToFileAtDomain(FileSystem.FSDomain.Debugging, "ser2_lateref_calls_" + DateTime.Now.ToString("yyyyMMdd_H_mm_ss") + ".txt",stb.ToString());
            lateRefCalls.Clear();
            debugOutput = "";
        }

        public void AddDebugOutput(String output, bool outputToConsoleImmediately=false) {
            debugOutput += $"[{name}]: {output}\n";
            if (outputToConsoleImmediately)
            {
                Debug.Log(output);
            }
        }
#endif


        ///// <summary>
        ///// A mapping object 2 offset in FlatBuffer
        ///// </summary>
        //public readonly Dictionary<IFBSerializable2, int> obj2offset = new Dictionary<IFBSerializable2, int>();

        /*public struct LateReference
        {
            public LateReference(object obj,bool storeAsTyped = false) {
                this.obj = obj;
                this.storeAsTyped = storeAsTyped;
            }
            public override bool Equals(object _compareWith) {
                if (!(_compareWith is LateReference)) return false;
                return obj.Equals(((LateReference)_compareWith).obj);
            }

            public object obj;
            public bool storeAsTyped;
        }*/
        /// <summary>
        /// mappings to objects that are not serialized,yet
        /// </summary>

        public string name = ""; // optional just for debugging purpose
        public static int name_counter = 1;
        /// <summary>
        /// If merged into another context this becomes the parent
        /// </summary>
        public SerializationContext parentContext = null; 

        public readonly Dictionary<object, List<int>> lateReferences = new Dictionary<object, List<int>>();
        //public readonly List<object> lateReferenceList = new List<object>();
        public Queue<object> lateReferenceQueue = new Queue<object>();
        public Queue<object> tempReferenceQueue = new Queue<object>();

        public readonly FlatBufferBuilder builder;

        private HashSet<Type> whiteList = null;
        private HashSet<Type> blackList = null;
        private Func<object, bool> customFilter = null;
        private Dictionary<object, int> obj2offsetMapping = new Dictionary<object, int>(); // mapping to offset of objects != ifbserializable2

        /// <summary>
        /// The amount of bytes to add to the offset of those bytebuffers
        /// </summary>
        private Dictionary<IFB2Context, int> offsetMapping = null;
        
        private bool isValid = true;

        public bool IsValid() {
            return isValid;
        }

        public void Invalidate() {
            isValid = false;
        }

        public SerializationContext(int initialBuilderCapacity,string _name=null) {
            builder = new FlatBufferBuilder(initialBuilderCapacity);
#if TESTING
            //perfTest = Kernel.Instance.Container.Resolve<Service.PerformanceTest.IPerformanceTestService>();
#endif 
            name = _name ?? "sctx-" + (name_counter++);
        }

        public SerializationContext(ByteBuffer bb, string _name = null) {
            builder = new FlatBufferBuilder(bb);
            name = _name ?? "sctx-" + (name_counter++);
        }

        private void ClearTables() {
            foreach (KeyValuePair<object, int> kv in obj2offsetMapping) {
                if (kv.Key is IFBSerializable2) {
                    ((IFBSerializable2)kv.Key).Ser2Clear();
                }
            }
            Type2IntMapper.instance.Ser2Clear();
        }

        public void AddObj2OffsetMapping(object obj,int offset) {
            obj2offsetMapping[obj] = offset;
        }

        public int AddObjectFromMergedCtx(SerializationContext mergedCtx, object obj) {
            if (offsetMapping!=null && offsetMapping.TryGetValue(mergedCtx,out int ctxOffset)) {
                int offset = mergedCtx.GetCachedOffset(obj);
                int newOffset = offset + ctxOffset;
                AddObj2OffsetMapping(obj, newOffset);
                return newOffset;
            } else {
                UnityEngine.Debug.LogError($"[{name}] Tried to use AddObjectFromMergedCtx for obj {obj}[{obj.GetType()}]! Did not work!");
                return 0;
            }
        }

        public int GetOrCreateLocked(object obj) {
            lock (obj) {
                return GetOrCreate(obj);
            }
        }

        public int GetOrCreate(object obj) {
            int cachedOffset = GetCachedOffset(obj);

            if (cachedOffset >= 0) {
#if TESTING
                //Debug.Log($"[{name}]: Reused cached object {obj}[{obj.GetType()}]|{obj.GetHashCode()}");
#endif
                return cachedOffset;
            }

            if (cachedOffset == -2) {
                throw new Exception($"[{name}] Get or create on object that is already serialized by other context:{obj} [{obj.GetType()}]");
            }

#if TESTING
            String watchname = obj.GetType().ToString();
            //perfTest.StartWatch(watchname);
            try {
#endif
                if (obj is IFBSerializable2 iFBSer2Obj) {
                    if (iFBSer2Obj is IFBSerializeOnMainThread) {
                    // serialize this on mainthread
                        ECS.Future serializeOnMain = new ECS.Future(ECS.FutureExecutionMode.onMainThread, () => {
                            int _newOffset = iFBSer2Obj.Ser2Serialize(this);
                            return _newOffset;
                        });
                        // wait for the result
                        int newOffsetFromMainThread = serializeOnMain.WaitForResult<int>();
                        obj2offsetMapping[obj] = newOffsetFromMainThread;
                        iFBSer2Obj.Ser2Context = this;
                        return newOffsetFromMainThread;
                    }
                    int newOffset = iFBSer2Obj.Ser2Serialize(this);
                    iFBSer2Obj.Ser2Context = this;
                    obj2offsetMapping[obj] = newOffset;
                    return newOffset;
                } else if (obj is IList) {
                    int newOffset = builder.CreateList((IList)obj, this);
                    obj2offsetMapping[obj] = newOffset;
                    return newOffset;
                } else if (obj is IDictionary) {
                    int newOffset = builder.CreateIDictionary((IDictionary)obj, this);
                    obj2offsetMapping[obj] = newOffset;
                    return newOffset;
                } else if (obj is String) {
                    int newOffset = builder.CreateString((string)obj).Value;
                    obj2offsetMapping[obj] = newOffset;
                    return newOffset;
                }
#if TESTING
            }
            finally {
                //perfTest.StopWatch(watchname);
            }
#endif

            return 0;
        }


        /// <summary>
        /// Serialize an object and keep the c#-type 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="offsetTypeName"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        //public int AddTypedObject(int o, int offsetTypeName, IFBSerializable2 obj) {
        //    AddReferenceOffset(-1, obj);
        //    builder.AddOffset(offsetTypeName);
        //    int offset = builder.Offset;
        //    builder.AddStruct(o, offset, 0);
        //    return offset;
        //}

        public int AddTypedObject(IFBSerializable2 obj) {
//            int offsetTypeName = builder.CreateSharedString(builder.GetTypeName(obj), true).Value;
            int typeId = Type2IntMapper.instance.GetIdFromType(obj.GetType());
            builder.AddInt(typeId);
            AddReferenceOffset(-1, obj);
            int offset = builder.Offset;
            return offset;
        }

        public int AddTypedObject(int o, IFBSerializable2 obj) {
            int typeObjectStruct = AddTypedObject(obj);
            builder.AddStruct(o, typeObjectStruct, 0);
            return typeObjectStruct;
        }

        private int GetCachedOffset(object obj) {
            if (obj is IFBSerializable2 ifbObj) {
                if (ifbObj.Ser2HasValidContext) {
                    if (ifbObj.Ser2Context == this) {
                        return ifbObj.Ser2Offset;
                    } else {
                        // not serialized by this serializer
                        if (offsetMapping!=null && offsetMapping.TryGetValue(ifbObj.Ser2Context, out int bufOffset) ) {
                            // this object is merged into this serializer, so we can use this by adding the corresponding offset
                            // plus we will add it to our obj2offsetMapping
                            ifbObj.Ser2Context = this;
                            int newOffset = ifbObj.Ser2Offset += bufOffset;
                            obj2offsetMapping[obj] = newOffset;
                            return newOffset;
                        } else {
                            return -2; // someone-else did already handled this object. We actually should never come to this point, as it should be prevented in the calls before(!?)
                        }
                    }
                }
            }
            else if (obj2offsetMapping.TryGetValue(obj,out int offset)) {
                return offset;
            } 
            return -1;
        }

        public void AddReferenceOffset(object obj) {
            if (obj == null) {
                builder.AddInt(0);
                return;
            }
            AddReferenceOffset(-1, obj);
        }
        public void AddReferenceOffset(int o, object obj) {
            if (obj == null) {
                return;
            }

            if (obj is IObservableDictionary) {
                AddReferenceOffset(o, ((IObservableDictionary)obj).InnerIDict);
                return;
            }
            else if (obj is IObservableList) {
                AddReferenceOffset(o, ((IObservableList)obj).InnerIList);
                return;
            }

            int cacheOffset = GetCachedOffset(obj);
            
            if (obj is IFBSerializeAsTypedObject) {
                int typeID = Type2IntMapper.instance.GetIdFromType(obj.GetType());
                builder.AddInt(typeID);
            }

            if (cacheOffset >= 0) { // if the obj has an offset(already serialized) but only if it is part of the same buffer
                // the object is already serialized
                builder._AddOffset(cacheOffset);
                if (o!=-1) builder.Slot(o);
            } else {
                // the object is not referenced,yet. Write a dummy int,that will be replaced later with the real offset
                builder.AddInt(255);

                if (o != -1) builder.Slot(o);
                AddLateReference(builder.Offset, obj);
            }
        }

        public void AddLateReference(int offset,object obj) {
            // TODO: check for cache and set immediately
            if (lateReferences.TryGetValue(obj, out List<int> offsetDummies)) {
                offsetDummies.Add(offset);
            } else {
                lateReferences[obj] = new List<int>() { offset };
                lateReferenceQueue.Enqueue(obj);
            }
        }





        /// <summary>
        /// Merge multiple contexts together into this one. 
        /// </summary>
        /// <param name="mergeCtx"></param>
        public void Merge(params SerializationContext[] mergeCtxs) {
            Merge(true, mergeCtxs);
        }
        public void Merge(bool resolveLateReferenceOnMain,params SerializationContext[] mergeCtxs) {

            // first resolve our local late references (TODO: multithreading)
            if (resolveLateReferenceOnMain) ResolveLateReferences();

            if (offsetMapping == null) {
                offsetMapping = new Dictionary<IFB2Context, int>();
            }

            foreach (var mergeCtx in mergeCtxs) {
                // first, resolve the buffer-local references
                mergeCtx.ResolveLateReferences(); // <-- TODO: call this foreach in a thread of its own

                // second, write mergeCtx's buffer to the current one
                var mergeBuilder = mergeCtx.builder;
                var mergeBB = mergeCtx.builder.DataBuffer;
                byte[] buf = mergeBB.ToArray<byte>(mergeBuilder._space, mergeBB.Length - mergeBuilder._space);

                // the ending for merged buffer:
                int newBufEnd = builder.Offset;
                builder.Add<byte>(buf);

                // keep the new ending for later adjustment
                // serialized objects know where they are serialized, if you try to serialize again you can query the ByteBuffer and map with this offset
                offsetMapping[mergeCtx] = newBufEnd;
                
                // add offsetmapping to new ctx and change their offset to match the new data-position
                if (mergeCtx.offsetMapping != null) {
                    foreach(var om in mergeCtx.offsetMapping) {
                        offsetMapping[om.Key] = om.Value + newBufEnd;
                    }
                }



                foreach (var kv in mergeCtx.lateReferences) {
                    var obj = kv.Key;
                    if (!lateReferenceQueue.Contains(obj)) {
                        lateReferenceQueue.Enqueue(obj);
                    }

                    if (!lateReferences.ContainsKey(obj)) {
                        lateReferences[obj] = new List<int>();
                    }

                    // write the locationOffsets of the merged context and add the new buffer-ending to offset
                    var lateRefsForObjects = lateReferences[obj];
                    foreach (int locationOffset in kv.Value) {
                        lateRefsForObjects.Add(locationOffset + newBufEnd);
                    }
                }

#if TESTING
                debugOutput += mergeCtx.debugOutput;
#endif
            }

            

            whiteList = null;
            blackList = null;
            customFilter = null;

            // now that we merged all the lateReferences and adjusted it to the new buffer, solve the last
            if (resolveLateReferenceOnMain) ResolveLateReferences();

        }

        /// <summary>
        /// Set filter for reference(!)-types that should be serialzied by this SerializationContext.
        /// If the filter return false, this type is not serialized, only marked for later serialization
        /// </summary>
        /// <param name="customFilter"></param>
        public void SetCustomFilter(Func<object,bool> customFilter) {
            this.customFilter = customFilter;
        }

        public void AddTypeToWhiteList(Type type) {
            if (whiteList == null) {
                whiteList = new HashSet<Type>();
            }
            whiteList.Add(type);
        }

        public void AddTypeToBlackList(Type type) {
            if (blackList == null) {
                blackList = new HashSet<Type>();
            }
            blackList.Add(type);
        }

        
        public void ResolveLateReferences(bool forceAll=false) {
            int calls = 0; 
            
            while (lateReferenceQueue.Count > 0) {
                calls++;
                //                var lateReference = lateReferenceList[checkIdx];
                var lateReference = lateReferenceQueue.Dequeue();

                var lateRefType = lateReference.GetType();
                if ( !forceAll 
                    && ( (customFilter!=null && !customFilter(lateReference))
                    || (whiteList != null && !whiteList.Contains(lateRefType))
                     || (blackList != null && blackList.Contains(lateRefType)) )
                ) {
                    // ignore all types that are not on whitelist (if using whitelist at all)
                    // ignore all types that are on the blacklist (if using blacklist)
#if TESTING
                    //UnityEngine.Debug.Log($"[{name}] Ignore lateref of {lateReference.GetType()}|{lateReference.GetHashCode()}. Keeping it for later mapping");
#endif                    
                    tempReferenceQueue.Enqueue(lateReference);
                    continue;
                }

                if (lateReference is IFBSerializable2) {
                    var ifbObj = (IFBSerializable2)lateReference;
                    if (ifbObj.Ser2HasValidContext && ifbObj.Ser2Context != this && (offsetMapping == null || !offsetMapping.ContainsKey(ifbObj.Ser2Context))) {
                        // ignore objects that are create within another builder and that is not merged in,yet
                        tempReferenceQueue.Enqueue(lateReference);
                        continue;
                    }
                }
                ResolveLateReference(lateReference);
            }
            var _tempQueue = lateReferenceQueue;
            lateReferenceQueue = tempReferenceQueue;
            tempReferenceQueue = _tempQueue;
#if TESTING
            //UnityEngine.Debug.Log($"[{name}]: ResolveLateReference-Calls: {calls}");
#endif
        }

        public void ResolveLateReference(object obj) {
#if TESTING
            AddRefCall(obj);
#endif
            int offset = GetOrCreate(obj);
            if (offset < 0) {
                throw new Exception($"Offset == {offset} for type:"+obj.GetType());
            }
            if (lateReferences.TryGetValue(obj, out List<int> referenceLocations)) {
                foreach (int referenceLoc in referenceLocations) {
                    int offsetAdjustment = 0;
                    if (offsetMapping != null  && obj is IFBSerializable2) {
                        var ifbObj = (IFBSerializable2)obj;
                        IFB2Context objCtx = ifbObj.Ser2Context;
                        
                        offsetMapping.TryGetValue(objCtx, out offsetAdjustment);
                    }

                    int relativeOffset = referenceLoc - (offset + offsetAdjustment);
                    builder.DataBuffer.PutInt(builder.DataBuffer.Length - referenceLoc, relativeOffset);
                }
                lateReferences.Remove(obj);
                //bool found = lateReferenceList.Remove(obj);
            }
        }

        public byte[] CreateSizedByteArray(int main, bool writeTypedList = true) {
            ResolveLateReferences();
            if (writeTypedList) {
                int offsetTypes2Int = Type2IntMapper.instance.Ser2Serialize(this);
                builder.AddOffset(offsetTypes2Int);
            }
            builder.Finish(main);
            byte[] result = builder.SizedByteArray();
#if TESTING
            AddDebugOutput($"buffersize:{result.Length}");
#endif
            return result;
        }


        public void Cleanup(params IFB2Context[] contexts) {
#if TESTING
            OutputDebugInfo();
#endif
            ClearTables();
            lateReferences.Clear();
            builder.Clear();
            obj2offsetMapping.Clear();
            Invalidate();
            if (offsetMapping != null)
            {
                foreach (var ctx in offsetMapping.Keys) {
                    ctx.Invalidate();
                }
            }
            foreach (IFB2Context ctx in contexts) {
                ctx.Invalidate();
            }

        }

    }



    public class FlatBufferSerializer2
    {

        public static ListPool<int> poolListInt = new ListPool<int>(10, 10);

        public enum Mode
        {
            serializing, deserializing
        }


        public static byte[] SerializeToBytes(IFBSerializable root, int initialBufferSize = 5000000) {
            byte[] buf = null;

            return buf;
        }

        public static T DeepCopy<T>(T original) where T : IFBSerializable, new() {
            byte[] buf = SerializeToBytes(original, 2048);
            T result = DeserializeFromBytes<T>(buf, default(T), original.GetType());
            return result;
        }

        public static void SerializeToFileDomain(FileSystem.FSDomain domain, String filename, IFBSerializable root) {
            byte[] buf = SerializeToBytes(root);
            FileSystem.IFileSystemService fs = Kernel.Instance.Container.Resolve<Service.FileSystem.IFileSystemService>();
            fs.WriteBytesToFileAtDomain(domain, filename, buf);
        }

        public static T DeserializeFromBytes<T>(byte[] buf, T dataRoot = default(T), Type type = null) where T : IFBSerializable, new() {
            return dataRoot;
        }




    }


}
