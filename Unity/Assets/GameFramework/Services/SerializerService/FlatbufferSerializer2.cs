using System.Collections.Generic;
using System;
using FlatBuffers;
using System.Linq;
using System.Collections;
using System.Text;
using System.Threading;

namespace Service.Serializer {
    
    public class DeserializationContext
    {
        private readonly Dictionary<int, IFBSerializable2> pos2obj = new Dictionary<int, IFBSerializable2>();

        public ByteBuffer bb;

        public DeserializationContext(ByteBuffer bb) {
            this.bb = bb;
        }
        public DeserializationContext(byte[] buf) {
            this.bb = new ByteBuffer(buf);
        }

        public T GetOrCreate<T>(int bufferOffset) where T:IFBSerializable2,new() {
            if (bufferOffset == 0) return default(T);

            if (pos2obj.TryGetValue(bufferOffset,out IFBSerializable2 result)) {
                return (T)result;
            } else {
                var newObject = new T();
                pos2obj[bufferOffset] = newObject;
                newObject.Deserialize2(bufferOffset,this);
                return newObject;
            }
        }

        public T GetReference<T>(int bufferOffset) where T : IFBSerializable2, new() {
            if (bufferOffset == 0) {
                return default(T);
            }

            var result = GetOrCreate<T>(bufferOffset);
            return result;
        }


        public T GetRoot<T>() where T : IFBSerializable2, new() {
            int offset = bb.Length - bb.GetInt(0);
            return GetOrCreate<T>(offset);
        }
    }

    public class SerializationContext {
        ///// <summary>
        ///// A mapping object 2 offset in FlatBuffer
        ///// </summary>
        //public readonly Dictionary<IFBSerializable2, int> obj2offset = new Dictionary<IFBSerializable2, int>();
        
        /// <summary>
        /// mappings to objects that are not serialized,yet
        /// </summary>
        public readonly Dictionary<DefaultSerializable2, List<int>> lateReferences = new Dictionary<DefaultSerializable2, List<int>>();
        public readonly List<DefaultSerializable2> lateReferenceList = new List<DefaultSerializable2>();

        public readonly FlatBufferBuilder builder;

        private HashSet<Type> whiteList = null;
        private HashSet<Type> blackList = null;

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

        public int GetOrCreate(DefaultSerializable2 obj) {
            if (obj.HasOffset) {
                // this object was already serialized. just output 
                return obj.Offset;
            }
            int newOffset = obj.Serialize2(this);
//            obj2offset[obj] = newOffset;
            return newOffset;
        }

        public void AddReferenceOffset(int o, DefaultSerializable2 obj) {
            if (obj == null) {
                return;
            }

            if (obj.HasOffset && obj.table.bb==builder.DataBuffer) { // if the obj has an offset(already serialized) but only if it is part of the same buffer
                // the object is already serialized
                builder.AddOffset(o, obj.Offset, 0);
            } else {
                // the object is not referenced,yet. Write a dummy int,that will be replaced later with the real offset
                builder.AddInt(0);
                builder.Slot(o);
                if (lateReferences.TryGetValue(obj, out List<int> offsetDummies)) {
                    offsetDummies.Add(builder.Offset);
                } else {
                    lateReferences[obj] = new List<int>() { builder.Offset };
                    lateReferenceList.Add(obj);
                }
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
                byte[] buf =  mergeBB.ToArray<byte>(mergeBuilder._space, mergeBB.Length - mergeBuilder._space);

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

            while (lateReferenceList.Count > checkIdx) {
                var refObj = lateReferenceList[checkIdx]; 

                if (   (whiteList!=null && !whiteList.Contains(refObj.GetType()))
                     ||(blackList!=null && blackList.Contains(refObj.GetType()))
                ) {
                    // ignore all types that are not on whitelist (if using whitelist at all)
                    // ignore all types that are on the blacklist (if using blacklist)
                    checkIdx++;
                    continue;
                }


                if (refObj.HasOffset && refObj.table.bb!=myBB && (offsetMapping==null || !offsetMapping.ContainsKey(refObj.table.bb))){
                    // ignore objects that are create within another builder and that is not merged in,yet
                    checkIdx++;
                    continue;
                }

                ResolveLateReference(refObj);
                lateReferences.Remove(refObj);
                lateReferenceList.RemoveAt(checkIdx);
            }
        }

        public void ResolveLateReference(DefaultSerializable2 obj) {
            int offset = GetOrCreate(obj);

            if (lateReferences.TryGetValue(obj,out List<int> referenceLocations)) {
                foreach (int referenceLoc in referenceLocations) {
                    int offsetAdjustment = 0;
                    if (offsetMapping != null) {
                        offsetMapping.TryGetValue(obj.table.bb, out offsetAdjustment);
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
        }

    }

    public interface IFBSerializable2 {
        int Serialize2(SerializationContext ctx);
        void _CreateTable(SerializationContext ctx, FlatBuffers.FlatBufferBuilder builder);
        void _UpdateTable(SerializationContext ctx, FlatBuffers.FlatBufferBuilder builder);
        void Deserialize2(int tblOffset, DeserializationContext ctx);
        void Deserialize2(DeserializationContext ctx);


        bool IsDirty { get; set; }

        bool HasOffset { get; }
        int Offset { get; }
    }

    public abstract class DefaultSerializable2 : IFBSerializable2
    {
        public ExtendedTable table = ExtendedTable.NULL;

        public bool IsDirty { get; set; }

        public bool HasOffset => !table.IsNULL();

        public int Offset => table.offset;

        private object lock_state = new object();

        public void Deserialize2(DeserializationContext ctx) {
            int offset = ctx.bb.Length - ctx.bb.GetInt(ctx.bb.Position) + ctx.bb.Position;
            Deserialize2(offset, ctx);
        }



        public virtual int Serialize2(SerializationContext ctx) {
            if (!HasOffset) {
                _CreateTable(ctx, ctx.builder);
            } else {
                _UpdateTable(ctx, ctx.builder);
            }
            return table.offset;
        }

        public abstract void _CreateTable(SerializationContext ctx, FlatBufferBuilder builder);
        public abstract void _UpdateTable(SerializationContext ctx, FlatBufferBuilder builder);
        public virtual void Deserialize2(int tblOffset, DeserializationContext ctx) {
            table = new ExtendedTable(tblOffset,ctx.bb);
        }
    }

    public class FlatBufferSerializer2 {

        public static ListPool<int> poolListInt = new ListPool<int>(10, 10);

        public enum Mode {
            serializing, deserializing
        }


        public static byte[] SerializeToBytes(IFBSerializable root,int initialBufferSize=5000000)  {
            byte[] buf = null;

            return buf;
        }

        public static T DeepCopy<T>(T original) where T : IFBSerializable,new() {
            byte[] buf = SerializeToBytes(original, 2048);
            T result = DeserializeFromBytes<T>(buf, default(T), original.GetType());
            return result;
        }

        public static void SerializeToFileDomain(FileSystem.FSDomain domain, String filename, IFBSerializable root)  {
            byte[] buf = SerializeToBytes(root);
            FileSystem.IFileSystemService fs = Kernel.Instance.Container.Resolve<Service.FileSystem.IFileSystemService>();
            fs.WriteBytesToFileAtDomain(domain, filename, buf);
        }

        public static T DeserializeFromBytes<T>(byte[] buf,T dataRoot=default(T), Type type = null) where T : IFBSerializable,new() {
            return dataRoot;
        }




    }


}
