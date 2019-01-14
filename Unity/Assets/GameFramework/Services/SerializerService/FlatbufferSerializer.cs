using System.Collections.Generic;
using System;
using FlatBuffers;

namespace Service.Serializer {
    public class FlatbufferSerializer {

        public static bool serializing = false;
        /// <summary>
        /// For deserializing mappings
        /// </summary>
        public static Dictionary<int, object> fb2objMapping = new Dictionary<int, object>();
        /// <summary>
        /// For serializing mappings
        /// </summary>
        public static Dictionary<object, int> obj2FSMapping = new Dictionary<object, int>();
        private FlatBufferBuilder fbBuilder;

        /// <summary>
        /// Create VectorOffset from List
        /// </summary>
        /// <typeparam name="T">IFBSerializable</typeparam>
        /// <typeparam name="S">FlatbufferType</typeparam>
        /// <param name="builder"></param>
        /// <param name="list"></param>
        /// <param name="fbCreateList"></param>
        /// <returns></returns>
        public static FlatBuffers.VectorOffset? CreateList<T, S>(FlatBuffers.FlatBufferBuilder builder
                                        , List<T> list, Func<FlatBufferBuilder, Offset<S>[], VectorOffset> fbCreateList)
                                        where S : struct, FlatBuffers.IFlatbufferObject where T : IFBSerializable {
            if (list == null) {
                return null;
            }

            if (FlatbufferSerializer.obj2FSMapping.TryGetValue(list, out int bufPos)) {
                // the list was already serialized so we need to use this VectorOffset in order to keep the reference
                var result = new FlatBuffers.VectorOffset(bufPos);
                return result;
            } else {
                if (typeof(IFBSerializable).IsAssignableFrom(typeof(T))) {
                    var tempArray = new FlatBuffers.Offset<S>[list.Count];
                    for (int i = 0; i < list.Count; i++) {
                        var listElemOffset = FlatbufferSerializer.GetOrCreateSerialize<S>(builder, (IFBSerializable)list[i]);
                        if (listElemOffset != null) {
                            tempArray[i] = (FlatBuffers.Offset<S>)listElemOffset;
                        }
                    }
                    var result = fbCreateList(builder, tempArray);
                    FlatbufferSerializer.obj2FSMapping[list] = result.Value;
                    return result;
                }
                return null;
            }
        }

        public static FlatBuffers.VectorOffset? CreateList<T>(FlatBuffers.FlatBufferBuilder builder
                                    , List<T> list, Func<FlatBufferBuilder, T[], VectorOffset> fbCreateList) {
            if (list == null || typeof(T).IsPrimitive) {
                return null;
            }

            if (FlatbufferSerializer.obj2FSMapping.TryGetValue(list, out int bufPos)) {
                // the list was already serialized so we need to use this VectorOffset in order to keep the reference
                var result = new FlatBuffers.VectorOffset(bufPos);
                return result;
            } else {

                var tempArray = list.ToArray();
                // call the createFunction with the array
                var result = fbCreateList(builder, tempArray);
                FlatbufferSerializer.obj2FSMapping[list] = result.Value;
                return result;
            }
        }

        public static StringOffset? GetOrCreateSerialize(FlatBufferBuilder builder, string serializableObj) {
            if (serializableObj == null) {
                return null;
            }

            // check if we have this Object already serialized and if yes grab the
            // location in the buffer and pass it as offset, so it can be pointed to this location again
            if (obj2FSMapping.TryGetValue(serializableObj, out int result)) {
                return new StringOffset(result);
            }

            var serializedString = builder.CreateString(serializableObj);
            obj2FSMapping[serializableObj] = serializedString.Value;
            return serializedString;
        }

        public static Offset<T>? GetOrCreateSerialize<T>(FlatBufferBuilder builder, IFBSerializable serializableObj) where T:struct,IFlatbufferObject {
            if (serializableObj == null) {
                return null;
            }

            // check if we have this Object already serialized and if yes grab the
            // location in the buffer and pass it as offset, so it can be pointed to this location again
            if (obj2FSMapping.TryGetValue(serializableObj, out int result)) {
                return new Offset<T>(result);
            }
            // first time, so serialize it with flatbuffers
            var serialized = serializableObj.Serialize(builder);
            obj2FSMapping[serializableObj] = serialized;
            return new Offset<T>(serialized);
        }

        public static T GetOrCreateDeserialize<T>(IFlatbufferObject incoming) where T : IFBSerializable,new() {
            if (incoming == null) {
                return default(T);
            }
            object result;
            // first check if we already deserialize the object at this position
            fb2objMapping.TryGetValue(incoming.BufferPosition, out result);
            if (result != null) {
                // yeah, we found it. no need to create a new object we got it already
                return (T)result;
            }

            // not deserialized, yet. Create a new object and call the deserialize method with the flatbuffers object
            var newObject = new T();
            newObject.Deserialize(incoming);
            fb2objMapping[incoming.BufferPosition] = newObject;
            return newObject;
        }

        public byte[] SerializeToBytes<T>(IFBSerializable root) where T : struct, FlatBuffers.IFlatbufferObject {
            serializing = true;

            fbBuilder = new FlatBufferBuilder(1024);

            var rootResult = root.Serialize(fbBuilder);

            fbBuilder.Finish(rootResult);
            // TODO: Check: Is this the whole buffer? Or is it even more?
            var buf = fbBuilder.DataBuffer.ToSizedArray();

            serializing = false;
            return buf;
        }

        public void SerializeToFileDomain<T>(FileSystem.FSDomain domain, String filename, IFBSerializable root) where T : struct, FlatBuffers.IFlatbufferObject {
            var buf = SerializeToBytes<T>(root);
            var fs = Kernel.Instance.Container.Resolve<Service.FileSystem.IFileSystemService>();
            fs.WriteBytesToFileAtDomain(domain, filename, buf);
        }

        public T DeserializeFromBytes<T>(byte[] buf) where T : IFBSerializable,new() {
            var fbByteBuffer = new ByteBuffer(buf);
            var dataRoot = new T();
            dataRoot.Deserialize(fbByteBuffer);
            return dataRoot;
        }
    }
}
