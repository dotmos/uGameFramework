#define FLATBUFFER_CHECK_TYPES

using System.Collections.Generic;
using System;
using FlatBuffers;
using System.Linq;
using System.Collections;
using System.Text;
using System.Threading;

namespace Service.Serializer
{
    public interface IFBSerializable2 {
        int Ser2Serialize(SerializationContext ctx);
        void Ser2CreateTable(SerializationContext ctx, FlatBuffers.FlatBufferBuilder builder);
        void Ser2UpdateTable(SerializationContext ctx, FlatBuffers.FlatBufferBuilder builder);
        void Ser2Deserialize(int tblOffset, DeserializationContext ctx);
        void Ser2Deserialize(DeserializationContext ctx);


        bool Ser2IsDirty { get; set; }
        ExtendedTable Ser2Table { get; }
        bool Ser2HasOffset { get; }
        int Ser2Offset { get; }
    }

    public interface IFBSerializable2Struct
    {
        int Put(FlatBuffers.FlatBufferBuilder builder);
        void Get(ExtendedTable table, int fbPos);

        int ByteSize { get; }
    }

    public class DefaultSerializable2 : IFBSerializable2
    {
        public ExtendedTable ser2table = ExtendedTable.NULL;

        public ExtendedTable Ser2Table => ser2table;

        public bool Ser2IsDirty { get; set; }

        public bool Ser2HasOffset => !ser2table.IsNULL();

        public int Ser2Offset => ser2table.offset;

        private object lock_state = new object();

        public void Ser2Deserialize(DeserializationContext ctx) {
            int offset = ctx.bb.Length - ctx.bb.GetInt(ctx.bb.Position) + ctx.bb.Position;
            Ser2Deserialize(offset, ctx);
        }



        public virtual int Ser2Serialize(SerializationContext ctx) {
            if (!Ser2HasOffset) {
                Ser2CreateTable(ctx, ctx.builder);
            } else {
                Ser2UpdateTable(ctx, ctx.builder);
            }
            return ser2table.offset;
        }

        public virtual void Ser2CreateTable(SerializationContext ctx, FlatBufferBuilder builder) { }
        public virtual void Ser2UpdateTable(SerializationContext ctx, FlatBufferBuilder builder) { }
        public virtual void Ser2Deserialize(int tblOffset, DeserializationContext ctx) {
            ser2table = new ExtendedTable(tblOffset, ctx.bb);
        }
    }

    public class DeserializationContext
    {
        private readonly Dictionary<int, object> pos2obj = new Dictionary<int, object>();

        public ByteBuffer bb;

        private ExtendedTable extTbl; // make helper calls available. (obviously only offset-based calls makes sense here)

        public DeserializationContext(ByteBuffer bb) {
            this.bb = bb;
            extTbl = new ExtendedTable(0, bb);
        }
        public DeserializationContext(byte[] buf) {
            this.bb = new ByteBuffer(buf);
            extTbl = new ExtendedTable(0, bb);
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
            object cachedObject = _GetCachedObject(bufferOffset,objectType);
            if (cachedObject != null) {
                return cachedObject;
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
                if (result.GetType() != objType) {
                    UnityEngine.Debug.LogError($"Got unexpected type from cached object! expected:{objType} in_cache:{result.GetType()}");
                }                
#endif
                return result;
            }
            return null;
        }


        private object _GetOrCreate(int bufferOffset, Type objType, object obj=null) {
            if (ExtendedTable.typeIFBSerializable2.IsAssignableFrom(objType)) {
                var newIFBSer2obj = (IFBSerializable2)obj ?? (IFBSerializable2)Activator.CreateInstance(objType);
                pos2obj[bufferOffset] = newIFBSer2obj;
                newIFBSer2obj.Ser2Deserialize(bufferOffset, this);
                return newIFBSer2obj;
            }
            else if (ExtendedTable.typeIList.IsAssignableFrom(objType)) {
                var newList = (IList)obj ?? (IList)Activator.CreateInstance(objType);
                pos2obj[bufferOffset] = newList;
                newList = extTbl.GetListFromOffset(bufferOffset, objType, this, newList, false);
                return newList;
            } 
            else if (ExtendedTable.typeIDictionary.IsAssignableFrom(objType)) {
                var newDict = (IDictionary)obj ?? (IDictionary)Activator.CreateInstance(objType);
                pos2obj[bufferOffset] = newDict;
                newDict = extTbl.GetDictionaryFromOffset(bufferOffset, newDict, this, false);
                return newDict;
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

            if (obj == null) {
                obj = new T();
            }

            obj = (T)GetOrCreate(bufferOffset,typeof(T),obj);
            return obj;
        }


        public T GetRoot<T>() where T : IFBSerializable2, new() {
            int offset = GetRootOffset();
            T data = new T();
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
    }

    public class SerializationContext
    {
        ///// <summary>
        ///// A mapping object 2 offset in FlatBuffer
        ///// </summary>
        //public readonly Dictionary<IFBSerializable2, int> obj2offset = new Dictionary<IFBSerializable2, int>();

        /// <summary>
        /// mappings to objects that are not serialized,yet
        /// </summary>
        public readonly Dictionary<object, List<int>> lateReferences = new Dictionary<object, List<int>>();
        public readonly List<object> lateReferenceList = new List<object>();

        public readonly FlatBufferBuilder builder;

        private HashSet<Type> whiteList = null;
        private HashSet<Type> blackList = null;
        private Dictionary<object, int> obj2offsetMapping = new Dictionary<object, int>(); // mapping to offset of objects != ifbserializable2

        /// <summary>
        /// The amount of bytes to add to the offset of those bytebuffers
        /// </summary>
        private Dictionary<ByteBuffer, int> offsetMapping = null;

        public SerializationContext(int initialBuilderCapacity) {
            builder = new FlatBufferBuilder(initialBuilderCapacity);
        }

        public SerializationContext(ByteBuffer bb) {
            builder = new FlatBufferBuilder(bb);
        }

        private void AdObj2OffsetMapping(object obj,int offset) {
            obj2offsetMapping[obj] = offset;
        }

        public int GetOrCreate(object obj) {
            int cachedOffset = GetCachedOffset(obj);
            
            if (cachedOffset != -1) {
                return cachedOffset;
            }

            if (obj is IFBSerializable2) {
                var iFBSer2Obj = (IFBSerializable2)obj;
                int newOffset = iFBSer2Obj.Ser2Serialize(this);
                return newOffset;
            } 
            else if (obj is IList) {
                int newOffset = builder.CreateList((IList)obj, this);
                obj2offsetMapping[obj] = newOffset;
                return newOffset;
            } 
            else if (obj is IDictionary) {
                int newOffset = builder.CreateIDictionary((IDictionary)obj, this);
                obj2offsetMapping[obj] = newOffset;
                return newOffset;
            }
            return 0;
        }


        /// <summary>
        /// Serialize an object and keep the c#-type 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="offsetTypeName"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int AddTypedObject(int o, int offsetTypeName, IFBSerializable2 obj) {
            AddReferenceOffset(-1, obj);
            builder.AddOffset(offsetTypeName);
            int offset = builder.Offset;
            builder.AddStruct(o, offset, 0);
            return offset;
        }


        public int AddTypedObject(int o, IFBSerializable2 obj) {
            int offsetTypeName = builder.CreateSharedString(builder.GetTypeName(obj), true).Value;

            AddReferenceOffset(-1, obj);
            builder.AddOffset(offsetTypeName);
            int offset = builder.Offset;
            builder.AddStruct(o, offset, 0);
            return offset;
        }

        private int GetCachedOffset(object obj) {
            if (obj is IFBSerializable2) {
                var ifbObj = (IFBSerializable2)obj;
                // only return offset for already serialized object from our buffers (offset from other buffers will change)
                return (ifbObj.Ser2HasOffset && ifbObj.Ser2Table.bb == builder.DataBuffer) ? ifbObj.Ser2Offset : -1;
            }
            else {
                if (obj2offsetMapping.TryGetValue(obj,out int offset)) {
                    return offset;
                }
                return -1;
            }
        }

        public void AddReferenceOffset(object obj) {
            AddReferenceOffset(-1, obj);
        }
        public void AddReferenceOffset(int o, object obj) {
            if (obj == null) {
                return;
            }

            int cacheOffset = GetCachedOffset(obj);

            if (cacheOffset != -1) { // if the obj has an offset(already serialized) but only if it is part of the same buffer
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
                lateReferenceList.Add(obj);
            }
        }





        /// <summary>
        /// Merge multiple contexts together into this one. 
        /// </summary>
        /// <param name="mergeCtx"></param>
        public void Merge(params SerializationContext[] mergeCtxs) {

            // first resolve our local late references (TODO: multithreading)
            ResolveLateReferences();

            if (offsetMapping == null) {
                offsetMapping = new Dictionary<ByteBuffer, int>();
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
                offsetMapping[mergeBB] = newBufEnd;

                foreach (var kv in mergeCtx.lateReferences) {
                    var obj = kv.Key;
                    if (!lateReferenceList.Contains(obj)) {
                        lateReferenceList.Add(obj);
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
            }

            whiteList = null;
            blackList = null;

            // now that we merged all the lateReferences and adjusted it to the new buffer, solve the last
            ResolveLateReferences();

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

        public void ResolveLateReferences() {
            var myBB = builder.DataBuffer;

            int checkIdx = 0;
            int calls = 0;
            while (lateReferenceList.Count > checkIdx) {
                calls++;
                var refObj = lateReferenceList[checkIdx];

                if ((whiteList != null && !whiteList.Contains(refObj.GetType()))
                     || (blackList != null && blackList.Contains(refObj.GetType()))
                ) {
                    // ignore all types that are not on whitelist (if using whitelist at all)
                    // ignore all types that are on the blacklist (if using blacklist)
                    checkIdx++;
                    continue;
                }

                if (refObj is IFBSerializable2) {
                    var ifbObj = (IFBSerializable2)refObj;
                    if (ifbObj.Ser2HasOffset && ifbObj.Ser2Table.bb != myBB && (offsetMapping == null || !offsetMapping.ContainsKey(ifbObj.Ser2Table.bb))) {
                        // ignore objects that are create within another builder and that is not merged in,yet
                        checkIdx++;
                        continue;
                    }
                }

                ResolveLateReference(refObj);
                lateReferences.Remove(refObj);
                lateReferenceList.RemoveAt(checkIdx);
            }
            UnityEngine.Debug.Log($"ResolveLateReference-Calls: {calls}");
        }

        public void ResolveLateReference(object obj) {
            int offset = GetOrCreate(obj);

            if (lateReferences.TryGetValue(obj, out List<int> referenceLocations)) {
                foreach (int referenceLoc in referenceLocations) {
                    int offsetAdjustment = 0;
                    if (offsetMapping != null  && obj is IFBSerializable2) {
                        var ifbObj = (IFBSerializable2)obj;
                        offsetMapping.TryGetValue(ifbObj.Ser2Table.bb, out offsetAdjustment);
                    }

                    int relativeOffset = referenceLoc - (offset + offsetAdjustment);
                    builder.DataBuffer.PutInt(builder.DataBuffer.Length - referenceLoc, relativeOffset);
                }
            }
        }

        public byte[] CreateSizedByteArray(int main) {
            builder.Finish(main);
            return builder.SizedByteArray();
        }

        public void Cleanup() {
            lateReferences.Clear();
            builder.Clear();
            obj2offsetMapping.Clear();
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
