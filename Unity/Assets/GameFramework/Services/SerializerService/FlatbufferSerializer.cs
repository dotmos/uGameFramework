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
        private static FlatBufferBuilder fbBuilder;

        public static Dictionary<Type, Func<object, FlatBufferBuilder, int>> serializeObjConverters = new Dictionary<Type, Func<object, FlatBufferBuilder, int>>();
        public static Dictionary<Type, Func<object, FlatBufferBuilder, object>> deserializeObjConverters = new Dictionary<Type, Func<object, FlatBufferBuilder, object>>();

        public static bool convertersActivated = false;

        public static int? Convert(FlatBufferBuilder builder, object obj)  {
            if (!convertersActivated) {
                ActivateConverters();
                convertersActivated = true;
            }

            if (serializeObjConverters.TryGetValue(obj.GetType(),out Func<object, FlatBufferBuilder, int> converter)){
                var bufPos = converter(obj, builder);
                return bufPos;
            } else {
                return null;
            }
        }

        public static void ActivateConverters() {
            serializeObjConverters[typeof(UnityEngine.Vector2)] = (data,builder) => {
                var vec2 = (UnityEngine.Vector2)data;
                Serial.FBVector2.StartFBVector2(builder);
                Serial.FBVector2.AddX(builder, vec2.x);
                Serial.FBVector2.AddY(builder, vec2.y);
                return Serial.FBVector2.EndFBVector2(builder).Value;
            };
            serializeObjConverters[typeof(UnityEngine.Vector3)] = (data, builder) => {
                var vec3 = (UnityEngine.Vector3)data;
                Serial.FBVector3.StartFBVector3(builder);
                Serial.FBVector3.AddX(builder, vec3.x);
                Serial.FBVector3.AddY(builder, vec3.y);
                Serial.FBVector3.AddZ(builder, vec3.z);
                return Serial.FBVector3.EndFBVector3(builder).Value;
            };
            serializeObjConverters[typeof(UnityEngine.Vector4)] = (data, builder) => {
                var vec4 = (UnityEngine.Vector4)data;
                Serial.FBVector4.StartFBVector4(builder);
                Serial.FBVector4.AddX(builder, vec4.x);
                Serial.FBVector4.AddY(builder, vec4.y);
                Serial.FBVector4.AddZ(builder, vec4.z);
                Serial.FBVector4.AddW(builder, vec4.w);
                return Serial.FBVector4.EndFBVector4(builder).Value;
            };
            serializeObjConverters[typeof(UnityEngine.Quaternion)] = (data, builder) => {
                var q = (UnityEngine.Quaternion)data;
                Serial.FBQuaternion.StartFBQuaternion(builder);
                Serial.FBQuaternion.AddX(builder, q.x);
                Serial.FBQuaternion.AddY(builder, q.y);
                Serial.FBQuaternion.AddZ(builder, q.z);
                Serial.FBQuaternion.AddW(builder, q.w);
                return Serial.FBQuaternion.EndFBQuaternion(builder).Value;
            };
            serializeObjConverters[typeof(UnityEngine.Material)] = (data, builder) => {
                var material = (UnityEngine.Material)data;
                var fbMatName = builder.CreateString(material.name);

                Serial.FBMaterial.StartFBMaterial(builder);
                Serial.FBMaterial.AddMaterialName(builder, fbMatName);
                return Serial.FBMaterial.EndFBMaterial(builder).Value;
            };
        }

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
                        var listElemOffset = FlatbufferSerializer.GetOrCreateSerialize(builder, (IFBSerializable)list[i]);
                        if (listElemOffset != null) {
                            tempArray[i] = new FlatBuffers.Offset<S>((int)listElemOffset);
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
            /*if (list == null || typeof(T).IsPrimitive) {
                return null;
            }*/

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


        /// <summary>
        /// Deserialized a list with NON-Primitive content
        /// </summary>
        /// <typeparam name="T">The result-type e.g. UID,... that implements IFBSerializable</typeparam>
        /// <typeparam name="S">The FBType that contained in the fb-list</typeparam>
        /// <param name="bufferPos">The bufferposition of this list</param>
        /// <param name="amount">The amount of elements this list contains</param>
        /// <param name="getItem">The function that returns the corresponding list-item wrapped into a to the parameter</param>
        /// <returns></returns>
        public static List<T> DeserializeList<T,S>(int bufferPos, int amount,List<object> items) where S : IFlatbufferObject where T : IFBSerializable,new() {
            if (bufferPos == 0) {
                return null;
            }

            if (fb2objMapping.TryGetValue(bufferPos,out object value)) {
                // we already deserialized this list give back the already created version to keep the reference
                return (List<T>)value;
            } else {
                var result = new List<T>();
                for (int i = 0; i < amount; i++) {
                    var obj = items[i];
                    if (obj != null) {
                        var deserializedElement = FlatbufferSerializer.GetOrCreateDeserialize<T>((S)obj);
                        result.Add(deserializedElement);
                    }
                }
                fb2objMapping[bufferPos] = result;
                return result;
            }
        }

        
        /// <summary>
        /// Deserialized a list with PRIMITIVE content (int,float,...)
        /// </summary>
        /// <typeparam name="T">The result-type e.g. UID,... that implements IFBSerializable</typeparam>
        /// <typeparam name="S">The FBType that contained in the fb-list</typeparam>
        /// <param name="pos">The postion of this list</param>
        /// <param name="getArray">Get the primitve array</param>
        /// <param name="getItem">The function that returns the corresponding item to the parameter</param>
        /// <returns></returns>
        public static List<T> DeserializeList<T>(int bufferPos, Func<T[]> getArray) {
            if (bufferPos == 0) {
                return null;
            }

            if (!typeof(T).IsPrimitive) {
                UnityEngine.Debug.LogError("Trying to use primitiveList deserializer on non primitive object:" + typeof(T));
            }

            if (fb2objMapping.TryGetValue(bufferPos, out object value)) {
                // we already deserialized this list give back the already created version to keep the reference
                return (List<T>)value;
            } else {
                var result = new List<T>(getArray());
                fb2objMapping[bufferPos] = result;
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

        public static int? GetOrCreateSerialize(FlatBufferBuilder builder, object serializableObj)  {
            if (serializableObj == null) {
                return 0;
            }

            // check if we have this Object already serialized and if yes grab the
            // location in the buffer and pass it as offset, so it can be pointed to this location again
            if (obj2FSMapping.TryGetValue(serializableObj, out int result)) {
                return result;
            }

            if (serializableObj is IFBSerializable) {
                // first time, so serialize it with flatbuffers
                var serialized = ((IFBSerializable)serializableObj).Serialize(builder);
                obj2FSMapping[serializableObj] = serialized;
                return serialized;
            }
            else {
                // try to convert
                var serialized = Convert(builder, serializableObj);
                if (serializableObj != null) {
                    obj2FSMapping[serializableObj] = serialized.Value;
                }
                return serialized;
            }


        }

        public static T GetOrCreateDeserialize<T>(IFlatbufferObject incoming) where T : IFBSerializable, new() {
            return (T)GetOrCreateDeserialize(incoming, typeof(T));
        }

        public static object GetOrCreateDeserialize(IFlatbufferObject incoming,Type type) {
                if (incoming == null) {
                return null;
            }
            object result;
            // first check if we already deserialize the object at this position
            fb2objMapping.TryGetValue(incoming.BufferPosition, out result);
            if (result != null) {
                UnityEngine.Debug.Log("Incoming-Type:"+incoming.GetType()+" Casting to :"+type.ToString());
                // yeah, we found it. no need to create a new object we got it already
                return result;
            }

            // not deserialized, yet. Create a new object and call the deserialize method with the flatbuffers object
            var newObject = (IFBSerializable)Activator.CreateInstance(type);
            newObject.Deserialize(incoming);
            fb2objMapping[incoming.BufferPosition] = newObject;
            return newObject;
        }

        public static void ClearCache() {
            obj2FSMapping.Clear();
            fb2objMapping.Clear();
        }

        public static byte[] SerializeToBytes(IFBSerializable root)  {
            serializing = true;
            ClearCache();
            fbBuilder = new FlatBufferBuilder(5000000);

            var rootResult = root.Serialize(fbBuilder);

            fbBuilder.Finish(rootResult);
            // TODO: Check: Is this the whole buffer? Or is it even more?
            var buf = fbBuilder.DataBuffer.ToSizedArray();

            serializing = false;
            return buf;
        }

        public static void SerializeToFileDomain(FileSystem.FSDomain domain, String filename, IFBSerializable root)  {
            var buf = SerializeToBytes(root);
            var fs = Kernel.Instance.Container.Resolve<Service.FileSystem.IFileSystemService>();
            fs.WriteBytesToFileAtDomain(domain, filename, buf);
        }

        public static T DeserializeFromBytes<T>(byte[] buf) where T : IFBSerializable,new() {
            ClearCache();
            var fbByteBuffer = new ByteBuffer(buf);
            var dataRoot = new T();
            dataRoot.Deserialize(fbByteBuffer);
            return dataRoot;
        }
    }
}
