using System.Collections.Generic;
using System;
using FlatBuffers;
using System.Linq;

namespace Service.Serializer {
    public class FlatbufferSerializer {

        public class ObjMapping {
            public int objId;
            public int bufPos;
        }

        public static bool serializing = false;
        /// <summary>
        /// For deserializing mappings
        /// </summary>
        public static Dictionary<int, object> fb2objMapping = new Dictionary<int, object>();
        /// <summary>
        /// For serializing mappings
        /// </summary>
        public static Dictionary<object, int> obj2FSMapping = new Dictionary<object, int>();

        public static Dictionary<int, object> serializeBufferposCheck = new Dictionary<int, object>();

        private static FlatBufferBuilder fbBuilder;

        public static Dictionary<Type, Func<object, FlatBufferBuilder, int>> serializeObjConverters = new Dictionary<Type, Func<object, FlatBufferBuilder, int>>();
        public static Dictionary<Type, Func<object, FlatBufferBuilder, object>> deserializeObjConverters = new Dictionary<Type, Func<object, FlatBufferBuilder, object>>();

        // objects that are serializing atm
        public static HashSet<object> serializingATM = new HashSet<object>();
        public static List<Action> afterSerializationAction = new List<Action>();
        // objects are deserializing atm
        public static HashSet<int> deserializingATM = new HashSet<int>();
        public static List<Action> afterDeserializationAction = new List<Action>();

        public static bool convertersActivated = false;

        public static void AddAfterSerializationAction(Action act) {
            afterSerializationAction.Add(act);
        }
        public static void AddAfterDeserializationAction(Action act) {
            afterDeserializationAction.Add(act);
        }

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

        /*
         var sIntDestcDict = FlatbufferSerializer.CreateDictionary<int,DestructionCosts,int, Offset<Serial.FBDestructionCosts>, Serial.DT_int_FBDestructionCosts>(builder, intDestcDict,  Serial.DT_int_FBDestructionCosts.CreateDT_int_FBDestructionCosts, Serial.FBTestComponent.CreateIntDestcDictVector);
         */
        public static FlatBuffers.VectorOffset? CreateDictionary<TKey, TValue,FBKey,FBValue,S>(FlatBuffers.FlatBufferBuilder builder
                                , Dictionary<TKey, TValue> dict
                                , Func<FlatBufferBuilder, FBKey,FBValue, Offset<S>> fbCreateElement
                                , Func<FlatBufferBuilder, Offset<S>[], VectorOffset> fbCreateList
                                )
                                where S : struct, FlatBuffers.IFlatbufferObject where FBValue : struct {
            if (dict == null) {
                return null;
            }


            int? bufferPos = FindInSerializeCache(dict);
            if (bufferPos.HasValue) {
                return new VectorOffset(bufferPos.Value);
            }
            var tempArray = new FlatBuffers.Offset<S>[dict.Count];
            int amount = dict.Count;
            if (typeof(TKey).IsPrimitive && typeof(TValue).IsPrimitive) {
                SetSerializingFlag(dict);
                // a pure primitive dictionary
                for (int i = 0; i < amount; i++) {
                    var dictElem = dict.ElementAt(i);
                    tempArray[i] = fbCreateElement(builder, (FBKey)((object)dictElem.Key), (FBValue)((object)dictElem.Value));
                }
                var result = fbCreateList(builder, tempArray);
                PutInSerializeCache(dict, result.Value);
                ClearSerializingFlag(dict);
                return result;
            }
            else if (typeof(TKey).IsPrimitive && !typeof(TValue).IsPrimitive) {
                SetSerializingFlag(dict);
                for (int i = 0; i < amount; i++) {
                    var dictElem = dict.ElementAt(i);

                    FBValue valueElemOffset;
                    if (typeof(TValue) == typeof(string)) {
                        valueElemOffset = (FBValue)(object)builder.CreateString((string)(object)dictElem.Value);
                    } else {
                        var offset = FlatbufferSerializer.GetOrCreateSerialize(builder, (IFBSerializable)dictElem.Value);
                        valueElemOffset = (FBValue)Activator.CreateInstance(typeof(FBValue), offset);
                    }
                    tempArray[i] = fbCreateElement(builder, (FBKey)((object)dictElem.Key), valueElemOffset);
                }
                var result = fbCreateList(builder, tempArray);
                PutInSerializeCache(dict, result.Value);
                ClearSerializingFlag(dict);
                return result;
            } else if (!typeof(TKey).IsPrimitive && typeof(TValue).IsPrimitive) {
                SetSerializingFlag(dict);
                for (int i = 0; i < amount; i++) {
                    var dictElem = dict.ElementAt(i);

                    FBKey offsetKey;
                    if (typeof(TKey) == typeof(string)) {
                        offsetKey = (FBKey)(object)builder.CreateString((string)(object)dictElem.Key);
                    }else {
                        var keyElemOffset = FlatbufferSerializer.GetOrCreateSerialize(builder, (IFBSerializable)dictElem.Key);
                        offsetKey = (FBKey)Activator.CreateInstance(typeof(FBKey), keyElemOffset);
                    }

                    tempArray[i] = fbCreateElement(builder, offsetKey, (FBValue)((object)dictElem.Value));
                }
                var result = fbCreateList(builder, tempArray);
                PutInSerializeCache(dict, result.Value);
                ClearSerializingFlag(dict);
                return result;
            } else if (!typeof(TKey).IsPrimitive && !typeof(TValue).IsPrimitive) {
                SetSerializingFlag(dict);
                for (int i = 0; i < amount; i++) {
                    var dictElem = dict.ElementAt(i);



                    FBKey offsetKey;
                    if (typeof(TKey) == typeof(string)) {
                        offsetKey = (FBKey)(object)builder.CreateString((string)(object)dictElem.Key);
                    } else {
                        var keyElemOffset = FlatbufferSerializer.GetOrCreateSerialize(builder, (IFBSerializable)dictElem.Key);
                        offsetKey = (FBKey)Activator.CreateInstance(typeof(FBKey), keyElemOffset);
                    }

                    FBValue valueElemOffset;
                    if (typeof(TValue) == typeof(string)) {
                        valueElemOffset = (FBValue)(object)builder.CreateString((string)(object)dictElem.Key);
                    } else {
                        var offset = FlatbufferSerializer.GetOrCreateSerialize(builder, (IFBSerializable)dictElem.Value);
                        valueElemOffset = (FBValue)Activator.CreateInstance(typeof(FBValue), offset);
                    }
                    tempArray[i] = fbCreateElement(builder, offsetKey, valueElemOffset); 
                }
                var result = fbCreateList(builder, tempArray);
                PutInSerializeCache(dict, result.Value);
                ClearSerializingFlag(dict);
                return result;
            }
            return null;
            /*if(typeof(IFBSerializable).IsAssignableFrom(typeof(T))) {
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
            return null;*/
        

    /*
    var intIntList = new Offset<Serial.DT_int_int>[this.intIntDict.Count];
    for (int i = 0; i < this.intIntDict.Count; i++) {
        var elem = this.intIntDict.ElementAt(i);
        intIntList[i] = Serial.DT_int_int.CreateDT_int_int(builder, elem.Key, elem.Value);
    }
    Serial.FBTestComponent.CreateIntIntDictVector(builder, intIntList);*/
    }

            public static FlatBuffers.VectorOffset? CreateDictionary<TKey, TValue,S>(FlatBuffers.FlatBufferBuilder builder
                                        , Dictionary<TKey,TValue> dict
                                        , Func<FlatBufferBuilder, TKey, TValue, Offset<S>> fbCreateElement
                                        , Func<FlatBufferBuilder, Offset<S>[], VectorOffset> fbCreateList
                                        )
                                        where S : struct, FlatBuffers.IFlatbufferObject {
            UnityEngine.Debug.LogError("You are using unimplemented CreateDictionary<TKey, TValue,S>(FlatBuffers.FlatBufferBuilder builder,dict,...)");
            return null;
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

            int? bufferPos = FindInSerializeCache(list);
            if (bufferPos.HasValue) {
                return new VectorOffset(bufferPos.Value);
            }


            if (typeof(IFBSerializable).IsAssignableFrom(typeof(T))) {
                SetSerializingFlag(list);
                var tempArray = new FlatBuffers.Offset<S>[list.Count];
                for (int i = 0; i < list.Count; i++) {
                    var listElemOffset = FlatbufferSerializer.GetOrCreateSerialize(builder, (IFBSerializable)list[i]);
                    if (listElemOffset != null) {
                        tempArray[i] = new FlatBuffers.Offset<S>((int)listElemOffset);
                    }
                }
                var result = fbCreateList(builder, tempArray);
                PutInSerializeCache(list, result.Value);
                ClearSerializingFlag(list);
                return result;
            }

            return null;
        }
        public static int CreateObjectReference(FlatBuffers.FlatBufferBuilder builder, object obj) {
            if (!FlatbufferSerializer.HasSerializingFlag(obj)) {
                var _innerTest = FlatbufferSerializer.GetOrCreateSerialize(builder, obj);
                return _innerTest.Value;
            }
            return -1;
        }

        public static FlatBuffers.VectorOffset CreateManualList<T>(FlatBuffers.FlatBufferBuilder builder,List<T> data) {
            if (data == null) {
                return new VectorOffset(0);
            }

            int? bufferPos = FindInSerializeCache(data);
            if (bufferPos.HasValue) {
                return new VectorOffset(bufferPos.Value);
            }

            SetSerializingFlag(data);
            if (typeof(T) == typeof(bool)) {
                builder.StartVector(1, data.Count, 1); for (int i = data.Count - 1; i >= 0; i--) builder.AddBool((bool)(object)data[i]); 
            } else if (typeof(T) == typeof(float)) {
                builder.StartVector(4, data.Count, 4); for (int i = data.Count - 1; i >= 0; i--) builder.AddFloat((float)(object)data[i]); 
            } else if (typeof(T) == typeof(int)) {
                builder.StartVector(4, data.Count, 4); for (int i = data.Count - 1; i >= 0; i--) builder.AddInt((int)(object)data[i]); 
            } else if (typeof(T) == typeof(long)) {
                builder.StartVector(8, data.Count, 8); for (int i = data.Count - 1; i >= 0; i--) builder.AddLong((long)(object)data[i]); 
            } else if (typeof(T) == typeof(string)) {
                int amount = data.Count;
                List<StringOffset> stOffsetList = new List<StringOffset>(amount);
                for (int i = 0; i < amount; i++) stOffsetList.Add(builder.CreateString((string)(object)data[i]));
                builder.StartVector(4, data.Count, 4); for (int i = amount - 1; i >= 0; i--) builder.AddOffset(stOffsetList[i].Value); 
            } else {
                int amount = data.Count;
                List<int> stOffsetList = new List<int>(amount);
                for (int i = 0; i < amount; i++) {
                    var elem = GetOrCreateSerialize(builder, data[i]);
                    stOffsetList.Add(elem.Value);
                }
                builder.StartVector(4, data.Count, 4); for (int i = amount - 1; i >= 0; i--) builder.AddOffset(stOffsetList[i]); 
            }
            var result = builder.EndVector();
            PutInSerializeCache(data, result.Value);
            ClearSerializingFlag(data);
            return result;
        }

        public static FlatBuffers.VectorOffset? CreateStringList(FlatBuffers.FlatBufferBuilder builder
                                    , List<string> list, Func<FlatBufferBuilder, StringOffset[],VectorOffset> fbCreateList) {
            if (list == null) {
                return new VectorOffset(0);
            }


            int? bufferPos = FindInSerializeCache(list);
            if (bufferPos.HasValue) {
                return new VectorOffset(bufferPos.Value);
            }

            var tempArray = list.Select(st => builder.CreateString(st)).ToArray();
            // call the createFunction with the array
            var result = fbCreateList(builder, tempArray);
            PutInSerializeCache(list, result.Value);
            return result;
        }


        public static FlatBuffers.VectorOffset? CreateList<T>(FlatBuffers.FlatBufferBuilder builder
                                    , List<T> list, Func<FlatBufferBuilder, T[], VectorOffset> fbCreateList) {
            /*if (list == null || typeof(T).IsPrimitive) {
                return null;
            }*/
            if (list == null) {
                return new VectorOffset(0);
            }

            int? bufferPos = FindInSerializeCache(list);
            if (bufferPos.HasValue) {
                return new VectorOffset(bufferPos.Value);
            }

            SetSerializingFlag(list);
            var tempArray = list.ToArray();
            // call the createFunction with the array
            var result = fbCreateList(builder, tempArray);

            PutInSerializeCache(list, result.Value);
            ClearSerializingFlag(list);
            return result;
        }


        /// <summary>
        /// Deserialized a list with NON-Primitive content
        /// </summary>
        /// <typeparam name="T">The result-type e.g. UID,... that implements IFBSerializable</typeparam>
        /// <typeparam name="S">The FBType that contained in the fb-list</typeparam>
        /// <param name="bufferPos">The bufferposition of this list</param>
        /// <param name="amount">The amount of elements this list contains</param>
        /// <param name="items">objects list with all objects to be converted into T-List</param>
        /// <returns></returns>
        public static List<T> DeserializeList<T,S>(int bufferPos, int amount,List<object> items) where S : IFlatbufferObject where T : IFBSerializable,new() {
            if (bufferPos == 0) {
                return null;
            }

            var cachedResult = FindInDeserializeCache(bufferPos);
            if (cachedResult != null) {
                try {
                    return (List<T>)cachedResult;
                }
                catch (Exception e) {
                    UnityEngine.Debug.LogException(e);
                    UnityEngine.Debug.Log("T:" + typeof(T) + " S:" + typeof(S) + " bufferpos:" + bufferPos);
                    UnityEngine.Debug.Log("Cached-ResultType:" + cachedResult.GetType()+"\n");
                    return null;
                }
            } else {
                SetDeserializingFlag(bufferPos);
                var result = new List<T>();
                PutIntoDeserializeCache(bufferPos, result);
                for (int i = 0; i < amount; i++) {
                    var obj = items[i];
                    if (obj != null) {
                        var deserializedElement = FlatbufferSerializer.GetOrCreateDeserialize<T>((S)obj);
                        result.Add(deserializedElement);
                    }
                }
                ClearDeserializingFlag(bufferPos);
                return result;
            }
        }

        /*
         *         
        intDestcDict = new Dictionary<int, DestructionCosts>();
        for (int i = 0; i < input.IntDestcDictLength; i++) {
            var e = input.IntDestcDict(i);
            if (e.HasValue) {
                var elem = e.Value;
                intDestcDict[elem.Key] = FlatbufferSerializer.GetOrCreateDeserialize<DestructionCosts>(elem.Value);
            }
        } 
        */

        /// <summary>
        /// Checks if an object already created for this position
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bufferpos"></param>
        /// <returns></returns>
        public static object FindInDeserializeCache(int bufferpos)  {
            if (bufferpos == 0) return null;

            if (fb2objMapping.TryGetValue(bufferpos, out object value)) {
                return value;
            }
            return null;
        }

        public static void PutIntoDeserializeCache(int bufferpos,object obj,bool checkIfExists=true) {
            if (obj == null) {
                UnityEngine.Debug.LogWarning("Tried to put null-object at pos:" + bufferpos);
                return;
            }
            if (checkIfExists && FindInDeserializeCache(bufferpos)!=null) {
                var beforeObj = FindInDeserializeCache(bufferpos);
                UnityEngine.Debug.LogError("WAAARNING: you are overwriting an existing object in deserialize-cache! before:" + beforeObj.GetType() + " new:" + obj.GetType());
            }
            fb2objMapping[bufferpos] = obj;
        }


        public static int? FindInSerializeCache(object obj){
            if (obj == null) {
                return null;
            }
            if (obj2FSMapping.TryGetValue(obj, out int value)) {
                return value;
            }
            return null;
        }


        public static void PutInSerializeCache(object obj, int bufferpos, bool checkIfExists = true)  {
            if (obj == null) {
                UnityEngine.Debug.LogWarning("Tried to put null-object at pos:" + bufferpos);
                return;
            }
            if (checkIfExists && serializeBufferposCheck.ContainsKey(bufferpos) && serializeBufferposCheck[bufferpos]!=(object)obj) {
                var beforeObj = serializeBufferposCheck[bufferpos];
                UnityEngine.Debug.LogError("WAAARNING: you are reusing an position in serialize-cache! before:" + beforeObj + " new:" + obj);
            }
            obj2FSMapping[obj] = bufferpos;
            if (checkIfExists) {
                serializeBufferposCheck[bufferpos] = obj;
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

            var result = FindInDeserializeCache(bufferPos);
            if (result!=null) {
                // we already deserialized this list give back the already created version to keep the reference
                return (List<T>)result;
            } else {
                List<T> newList = new List<T>(getArray());
                PutIntoDeserializeCache(bufferPos, newList);
                return newList;
            }
        }

        public static StringOffset? GetOrCreateSerialize(FlatBufferBuilder builder, string serializableObj) {
            if (serializableObj == null) {
                return null;
            }

            // check if we have this Object already serialized and if yes grab the
            // location in the buffer and pass it as offset, so it can be pointed to this location again
            int? bufferPos = FindInSerializeCache(serializableObj);
            if (bufferPos.HasValue) {
                return new StringOffset(bufferPos.Value);
            }

            var serializedString = builder.CreateString(serializableObj);
            PutInSerializeCache(serializedString, serializedString.Value);
            return serializedString;
        }

        public static int? GetOrCreateSerialize(FlatBufferBuilder builder, object serializableObj)  {
            if (serializableObj == null ) {
                return 0;
            }

            int? bufferPos = FindInSerializeCache(serializableObj);
            if (bufferPos.HasValue) {
                return bufferPos;
            }

            if (serializableObj is IFBSerializable) {
                SetSerializingFlag(serializableObj);
                // first time, so serialize it with flatbuffers
                var serialized = ((IFBSerializable)serializableObj).Serialize(builder);
                PutInSerializeCache(serializableObj, serialized);
                ClearSerializingFlag(serializableObj);
                return serialized;
            }
            else {
                // try to convert
                SetSerializingFlag(serializableObj);
                var serialized = Convert(builder, serializableObj);
                if (serializableObj != null) {
                    PutInSerializeCache(serializableObj, serialized.Value);
                }
                ClearSerializingFlag(serializableObj);
                return serialized;
            }


        }

        public static T GetOrCreateDeserialize<T>(IFlatbufferObject incoming,IFBSerializable newObject=null) where T : IFBSerializable, new() {
            return (T)GetOrCreateDeserialize(incoming, typeof(T),newObject);
        }

        public static bool HasDeserializingFlag(int bufferPos) {
            return deserializingATM.Contains(bufferPos);
        }


        public static void SetDeserializingFlag(int bufferPos) {
            if (deserializingATM.Contains(bufferPos)) {
                UnityEngine.Debug.LogError("Try to set a bufferpos to deserialize that is already in! bPos:"+bufferPos);
            }
            deserializingATM.Add(bufferPos);
        }

        public static void ClearDeserializingFlag(int bufferPos) {
            deserializingATM.Remove(bufferPos);
        }

        public static void SetSerializingFlag(object serializeObj) {
            if (serializingATM.Contains(serializeObj)) {
                UnityEngine.Debug.LogError("Try to set an object to serializeFlag that is already in! type:" + serializeObj.GetType());
            }
            serializingATM.Add(serializeObj);
        }
        public static bool HasSerializingFlag(object obj) {
            return serializingATM.Contains(obj);
        }

        public static void ClearSerializingFlag(object obj) {
            serializingATM.Remove(obj);
        }

        public static object GetOrCreateDeserialize(IFlatbufferObject incoming,Type type, IFBSerializable newObject=null) {
            if (incoming == null || incoming.BufferPosition==0) {
                return null;
            }
            // first check if we already deserialize the object at this position
            object result = FindInDeserializeCache(incoming.BufferPosition);
            if (result != null) {
                //UnityEngine.Debug.Log("Incoming-Type:"+incoming.GetType()+" Casting to :"+type.ToString());
                // yeah, we found it. no need to create a new object we got it already
                return result;
            }

            SetDeserializingFlag(incoming.BufferPosition);
            // not deserialized, yet. Create a new object and call the deserialize method with the flatbuffers object
            if (newObject == null) {
                newObject = (IFBSerializable)Activator.CreateInstance(type);
            }
            PutIntoDeserializeCache(incoming.BufferPosition, newObject);
            newObject.Deserialize(incoming);
            ClearDeserializingFlag(incoming.BufferPosition);
            return newObject;
        }

        public static void ClearCache() {
            obj2FSMapping.Clear();
            fb2objMapping.Clear();
            serializeBufferposCheck.Clear();
            deserializingATM.Clear();
            serializingATM.Clear();
            afterDeserializationAction.Clear();
            afterSerializationAction.Clear();
        }

        public static byte[] SerializeToBytes(IFBSerializable root)  {
            serializing = true;
            ClearCache();
            fbBuilder = new FlatBufferBuilder(5000000);

            var rootResult = root.Serialize(fbBuilder);

            foreach (var act in afterSerializationAction) {
                act();
            }

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
            foreach (var act in afterDeserializationAction) {
                act();
            }
            return dataRoot;
        }
    }
}
