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
            fb2objMapping.TryGetValue(incoming.pos, out result);
            if (result != null) {
                // yeah, we found it. no need to create a new object we got it already
                return (T)result;
            }

            // not deserialized, yet. Create a new object and call the deserialize method with the flatbuffers object
            var newObject = new T();
            newObject.Deserialize(incoming);
            fb2objMapping[incoming.pos] = newObject;
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
