using System.Collections.Generic;
using System;
using FlatBuffers;
using System.Linq;
using System.Collections;

namespace Service.Serializer {
    public class FlatBufferSerializer {

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

        public static Dictionary<Type, List<object>> postProcessObjects = new Dictionary<Type, List<object>>();

        public static Dictionary<int, object> serializeBufferposCheck = new Dictionary<int, object>();

        private static FlatBufferBuilder fbBuilder;

        public static Dictionary<Type, Func<object, FlatBufferBuilder, int>> serializeObjConverters = new Dictionary<Type, Func<object, FlatBufferBuilder, int>>();
        public static Dictionary<Type, Func<object, object>> deserializeObjConverters = new Dictionary<Type, Func<object, object>>();

        private static int currentDataFormatVersion = 1;
        public static int CurrentDataFormatVersion { get => currentDataFormatVersion; set => currentDataFormatVersion = value; }

        private static int currentDeserializingDataFormatVersion = 1;
        public static int CurrentDeserializingDataFormatVersion { get => currentDeserializingDataFormatVersion; set => currentDeserializingDataFormatVersion = value; }

        /// <summary>
        /// Is the version of the serialized data different to the current one
        /// </summary>
        public static bool DeserializationVersionMismatch {
            get => CurrentDataFormatVersion != CurrentDeserializingDataFormatVersion;
        }

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

        /// <summary>
        /// Take a non IFlatbufferObject and try to create an offset via the registered converters
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int? ConvertToFlatbuffer(FlatBufferBuilder builder, object obj)  {
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

        public static object ConvertToObject(object incoming,Type resultType) {
            if (!convertersActivated) {
                ActivateConverters();
                convertersActivated = true;
            }

            if (resultType == null) {
                int a = 0;
                return null;
            }

           // UnityEngine.Debug.Log("Try to convertToObject:" + incoming.GetType().FullName + " =TO=> " + resultType.FullName);
            if (deserializeObjConverters.TryGetValue(resultType, out Func<object, object> converter)) {
                var result = converter(incoming);
                return result;
            } else {
                return null;
            }
        }

        public static void ActivateConverters() {
            // ------------------------- Vector2 --------------------------------------
            serializeObjConverters[typeof(UnityEngine.Vector2)] = (data,builder) => {
                var vec2 = (UnityEngine.Vector2)data;
                Serial.FBVector2.StartFBVector2(builder);
                Serial.FBVector2.AddX(builder, vec2.x);
                Serial.FBVector2.AddY(builder, vec2.y);
                return Serial.FBVector2.EndFBVector2(builder).Value;
            };
            deserializeObjConverters[typeof(UnityEngine.Vector2)] = (incoming) => {
                var fbVec2 = (Serial.FBVector2)incoming;
                var vec2 = new UnityEngine.Vector3(fbVec2.X, fbVec2.Y);
                return vec2;
            };
            // ------------------------- Vector3 --------------------------------------
            serializeObjConverters[typeof(UnityEngine.Vector3)] = (data, builder) => {
                var vec3 = (UnityEngine.Vector3)data;
                Serial.FBVector3.StartFBVector3(builder);
                Serial.FBVector3.AddX(builder, vec3.x);
                Serial.FBVector3.AddY(builder, vec3.y);
                Serial.FBVector3.AddZ(builder, vec3.z);
                return Serial.FBVector3.EndFBVector3(builder).Value;
            };
            deserializeObjConverters[typeof(UnityEngine.Vector3)] = (incoming) => {
                var fbVec3 = (Serial.FBVector3)incoming;
                var vec3 = new UnityEngine.Vector3(fbVec3.X, fbVec3.Y, fbVec3.Z);
                return vec3;
            };
            // ------------------------- Vector4 --------------------------------------
            serializeObjConverters[typeof(UnityEngine.Vector4)] = (data, builder) => {
                var vec4 = (UnityEngine.Vector4)data;
                Serial.FBVector4.StartFBVector4(builder);
                Serial.FBVector4.AddX(builder, vec4.x);
                Serial.FBVector4.AddY(builder, vec4.y);
                Serial.FBVector4.AddZ(builder, vec4.z);
                Serial.FBVector4.AddW(builder, vec4.w);
                return Serial.FBVector4.EndFBVector4(builder).Value;
            };
            deserializeObjConverters[typeof(UnityEngine.Vector4)] = (incoming) => {
                var fbVec4 = (Serial.FBVector4)incoming;
                var vec4 = new UnityEngine.Vector4(fbVec4.X, fbVec4.Y, fbVec4.Z, fbVec4.W);
                return vec4;
            };
            // ------------------------- Quaternion --------------------------------------
            serializeObjConverters[typeof(UnityEngine.Quaternion)] = (data, builder) => {
                var q = (UnityEngine.Quaternion)data;
                Serial.FBQuaternion.StartFBQuaternion(builder);
                Serial.FBQuaternion.AddX(builder, q.x);
                Serial.FBQuaternion.AddY(builder, q.y);
                Serial.FBQuaternion.AddZ(builder, q.z);
                Serial.FBQuaternion.AddW(builder, q.w);
                return Serial.FBQuaternion.EndFBQuaternion(builder).Value;
            };
            deserializeObjConverters[typeof(UnityEngine.Quaternion)] = (incoming) => {
                var fbQuaternion = (Serial.FBQuaternion)incoming;
                var quat = new UnityEngine.Quaternion(fbQuaternion.X, fbQuaternion.Y, fbQuaternion.Z, fbQuaternion.W);
                return quat;
            };
            // ------------------------- Material --------------------------------------
            //serializeObjConverters[typeof(UnityEngine.Material)] = (data, builder) => {
            //    var material = (UnityEngine.Material)data;
            //    var fbMatName = builder.CreateString(material.name);

            //    Serial.FBMaterial.StartFBMaterial(builder);
            //    Serial.FBMaterial.AddMaterialName(builder, fbMatName);
            //    return Serial.FBMaterial.EndFBMaterial(builder).Value;
            //};
            //deserializeObjConverters[typeof(UnityEngine.Quaternion)] = (incoming) => {
            //    return null;
            //};
        }

        /*
         var sIntDestcDict = FlatbufferSerializer.CreateDictionary<int,DestructionCosts,int, Offset<Serial.FBDestructionCosts>, Serial.DT_int_FBDestructionCosts>(builder, intDestcDict,  Serial.DT_int_FBDestructionCosts.CreateDT_int_FBDestructionCosts, Serial.FBTestComponent.CreateIntDestcDictVector);
         */
        public static FlatBuffers.VectorOffset? CreateDictionary<TKey, TValue,FBKey,FBValue,S>(FlatBuffers.FlatBufferBuilder builder
                                , IDictionary<TKey, TValue> dict
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

            var keyPrimOrEnum = typeof(TKey).IsPrimitive || typeof(TKey).IsEnum;
            var valuePrimOrEnum = typeof(TValue).IsPrimitive || typeof(TValue).IsEnum;

            if (keyPrimOrEnum && valuePrimOrEnum) {
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
            else if (keyPrimOrEnum && !valuePrimOrEnum) {
                SetSerializingFlag(dict);
                for (int i = 0; i < amount; i++) {
                    var dictElem = dict.ElementAt(i);

                    FBValue valueElemOffset;
                    if (typeof(TValue) == typeof(string)) {
                        valueElemOffset = (FBValue)(object)builder.CreateString((string)(object)dictElem.Value);
                    } 
                    else {
                        var offset = FlatBufferSerializer.GetOrCreateSerialize(builder, dictElem.Value);
                        valueElemOffset = (FBValue)Activator.CreateInstance(typeof(FBValue), offset);
                    }
                    tempArray[i] = fbCreateElement(builder, (FBKey)((object)dictElem.Key), valueElemOffset);
                }
                var result = fbCreateList(builder, tempArray);
                PutInSerializeCache(dict, result.Value);
                ClearSerializingFlag(dict);
                return result;
            } else if (!keyPrimOrEnum && valuePrimOrEnum) {
                SetSerializingFlag(dict);
                for (int i = 0; i < amount; i++) {
                    var dictElem = dict.ElementAt(i);

                    FBKey offsetKey;
                    if (typeof(TKey) == typeof(string)) {
                        offsetKey = (FBKey)(object)builder.CreateString((string)(object)dictElem.Key);
                    }else {
                        var keyElemOffset = FlatBufferSerializer.GetOrCreateSerialize(builder, (IFBSerializable)dictElem.Key);
                        offsetKey = (FBKey)Activator.CreateInstance(typeof(FBKey), keyElemOffset);
                    }

                    tempArray[i] = fbCreateElement(builder, offsetKey, (FBValue)((object)dictElem.Value));
                }
                var result = fbCreateList(builder, tempArray);
                PutInSerializeCache(dict, result.Value);
                ClearSerializingFlag(dict);
                return result;
            } else if (!keyPrimOrEnum && !valuePrimOrEnum) {
                SetSerializingFlag(dict);
                for (int i = 0; i < amount; i++) {
                    var dictElem = dict.ElementAt(i);



                    FBKey offsetKey;
                    if (typeof(TKey) == typeof(string)) {
                        offsetKey = (FBKey)(object)builder.CreateString((string)(object)dictElem.Key);
                    } else {
                        var keyElemOffset = FlatBufferSerializer.GetOrCreateSerialize(builder, (IFBSerializable)dictElem.Key);
                        offsetKey = (FBKey)Activator.CreateInstance(typeof(FBKey), keyElemOffset);
                    }

                    FBValue valueElemOffset;
                    if (typeof(TValue) == typeof(string)) {
                        valueElemOffset = (FBValue)(object)builder.CreateString((string)(object)dictElem.Key);
                    } else {
                        var offset = FlatBufferSerializer.GetOrCreateSerialize(builder, (IFBSerializable)dictElem.Value);
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
                                        , IList<T> list, Func<FlatBufferBuilder, Offset<S>[], VectorOffset> fbCreateList)
                                        where S : struct, FlatBuffers.IFlatbufferObject where T : IFBSerializable {
            if (list == null) {
                return new VectorOffset(0);
            }

            int? bufferPos = FindInSerializeCache(list);
            if (bufferPos.HasValue) {
                return new VectorOffset(bufferPos.Value);
            }


            if (typeof(IFBSerializable).IsAssignableFrom(typeof(T))) {
                SetSerializingFlag(list);
                var tempArray = new FlatBuffers.Offset<S>[list.Count];
                for (int i = 0; i < list.Count; i++) {
                    var listElemOffset = FlatBufferSerializer.GetOrCreateSerialize(builder, (IFBSerializable)list[i]);
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
            if (!FlatBufferSerializer.HasSerializingFlag(obj)) {
                var _innerTest = FlatBufferSerializer.GetOrCreateSerialize(builder, obj);
                return _innerTest.Value;
            }
            return -1;
        }

        public static String GetTypeName(object elem) {
            var type = elem.GetType();
            var assemblyName = type.Assembly.FullName;
            var typeName = type.FullName + ", " + assemblyName.Substring(0, assemblyName.IndexOf(','));
            return typeName;
        }

        /// <summary>
        /// Create a list and store the objects types to make a deserialization of list with inherited type possible
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="dataList"></param>
        /// <returns></returns>
        public static VectorOffset CreateTypedList(FlatBufferBuilder builder, IList dataList) {
            List<int> listOfOffsets = new List<int>(dataList.Count * 2);
            foreach (var elem in dataList) {
                if (elem == null) {
                    listOfOffsets.Add(0);
                    listOfOffsets.Add(0);
                    continue;
                } 
                var typeName = GetTypeName(elem);
                var offsetTypeName = builder.CreateSharedString(typeName);
                var offsetData = FlatBufferSerializer.GetOrCreateSerialize(builder, elem);
                listOfOffsets.Add(offsetTypeName.Value);
                listOfOffsets.Add(offsetData.Value);
            }
            builder.StartVector(4, listOfOffsets.Count, 4);
            for (int i = listOfOffsets.Count - 1; i >= 0; i--) builder.AddOffset(listOfOffsets[i]);
            return builder.EndVector();
        }

        public static FlatBuffers.VectorOffset CreateManualArray(FlatBuffers.FlatBufferBuilder builder, byte[] data) {
            builder.StartVector(1, data.Length, 1);
            builder.Add<byte>(data);
            return builder.EndVector();
        }

        public static FlatBuffers.VectorOffset CreateManualArray<T>(FlatBuffers.FlatBufferBuilder builder, T[] data)  {
            if (data == null) {
                return new VectorOffset(0);
            }

            int? bufferPos = FindInSerializeCache(data);
            if (bufferPos.HasValue) {
                return new VectorOffset(bufferPos.Value);
            }

            SetSerializingFlag(data);
            if (typeof(T) == typeof(bool)) {
                builder.StartVector(1, data.Length, 1); for (int i = data.Length - 1; i >= 0; i--) builder.AddBool((bool)(object)data[i]);
            } else if (typeof(T) == typeof(float)) {
                builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddFloat((float)(object)data[i]);
            } else if (typeof(T) == typeof(int) || typeof(T).IsEnum) {
                builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddInt((int)(object)data[i]);
            } else if (typeof(T) == typeof(long)) {
                builder.StartVector(8, data.Length, 8); for (int i = data.Length - 1; i >= 0; i--) builder.AddLong((long)(object)data[i]);
            } else if (typeof(T) == typeof(string)) {
                int amount = data.Length;
                List<StringOffset> stOffsetList = new List<StringOffset>(amount);
                for (int i = 0; i < amount; i++) stOffsetList.Add(builder.CreateString((string)(object)data[i]));
                builder.StartVector(4, data.Length, 4); for (int i = amount - 1; i >= 0; i--) builder.AddOffset(stOffsetList[i].Value);
            } else if (typeof(T) == typeof(byte)) {
                UnityEngine.Debug.LogError("CreateManual-Array not supported for byte! Use CreateManualByteArray");
            }
            else {
                int amount = data.Length;
                List<int> stOffsetList = new List<int>(amount);
                for (int i = 0; i < amount; i++) {
                    var elem = GetOrCreateSerialize(builder, data[i]);
                    stOffsetList.Add(elem.Value);
                }
                builder.StartVector(4, data.Length, 4); for (int i = amount - 1; i >= 0; i--) builder.AddOffset(stOffsetList[i]);
            }
            var result = builder.EndVector();
            PutInSerializeCache(data, result.Value);
            ClearSerializingFlag(data);
            return result;
        }



        public static FlatBuffers.VectorOffset CreateManualList<T>(FlatBuffers.FlatBufferBuilder builder,IList<T> data) {
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
            } else if (typeof(T) == typeof(int) || typeof(T).IsEnum) {
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
        public static IList<T> DeserializeList<T,S>(int bufferPos, int amount,List<object> items,bool isObservableList=false) where S : IFlatbufferObject where T : new() {
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
                var result = isObservableList? new ObservableList<T>() : (IList<T>) new List<T>();
                PutIntoDeserializeCache(bufferPos, result);
                for (int i = 0; i < amount; i++) {
                    var obj = items[i];
                    if (obj != null) {
                        var deserializedElement = FlatBufferSerializer.GetOrCreateDeserialize<T>((S)obj);
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

            if (obj.GetType().IsValueType) {
                return;
            }

            if (checkIfExists && FindInDeserializeCache(bufferpos)!=null) {
                var beforeObj = FindInDeserializeCache(bufferpos);
                UnityEngine.Debug.LogError("WAAARNING: you are overwriting an existing object in deserialize-cache! before:" + beforeObj.GetType() + " new:" + obj.GetType());
            }
            fb2objMapping[bufferpos] = obj;
            if (obj is IFBPostDeserialization) {
                if (postProcessObjects.TryGetValue(obj.GetType(), out List<object> objList)) {
                    objList.Add(obj);
                } else {
                    postProcessObjects[obj.GetType()] = new List<object>() { obj };
                }
            }
        }

        public static void ProcessPostProcessing(object userobject) {
            var entityManager = Kernel.Instance.Resolve<ECS.IEntityManager>();
            // post-process objects that are marked via FlatbufferSerializer.AddPostProcessType(type)
            for (int i = 0; i < FlatBufferSerializer.postProcessObjects.Count; i++) {
                var postProcessObjList = FlatBufferSerializer.postProcessObjects.ElementAt(i).Value;
                for (int j = 0; j < postProcessObjList.Count; j++) {
                    var elem = postProcessObjList[j];
                    if (elem is IFBPostDeserialization) {
                        ((IFBPostDeserialization)elem).OnPostDeserialization(entityManager,userobject);
                    }
                }
            }
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
                return new StringOffset(0);
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

        public static FBManualObject GetManualObject(object incoming) {
            var fbManual = new FBManualObject();
            fbManual.__initFromRef((IFlatbufferObject)incoming);
            return fbManual;
        }

        public static T CastSerialObject<T>(object incoming) where T : IFlatbufferObject, new() {
            var fbObj = (IFlatbufferObject)incoming;
            var t = new T();
            t.__init(fbObj.BufferPosition, fbObj.ByteBuffer);
            return t;
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
                var serialized = ConvertToFlatbuffer(builder, serializableObj);
                if (serializableObj != null && serialized.HasValue) {
                    PutInSerializeCache(serializableObj, serialized.Value);
                }
                ClearSerializingFlag(serializableObj);
                return serialized.HasValue?serialized:0;
            }


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

        public static void UpgradeObjectIfNeeded(object deserializedObject,object incomingData) {
            if (FlatBufferSerializer.DeserializationVersionMismatch
                && deserializedObject is IFBUpgradeable) {
                // the data we just deserialized has another version. Try to convert it to have valid data
                ((IFBUpgradeable)deserializedObject).Upgrade(CurrentDeserializingDataFormatVersion, CurrentDataFormatVersion, incomingData);
            }
        }

        public static T GetOrCreateDeserialize<T>(IFlatbufferObject incoming, IFBSerializable newObject = null) where T : new() {
            if (incoming == null || incoming.BufferPosition == 0) {
                return default(T);
            }
            var result = GetOrCreateDeserialize(incoming, typeof(T), newObject);
            return (T)result;
        }

        public static object GetOrCreateDeserialize(IFlatbufferObject incoming, Type type, IFBSerializable newObject = null) {
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

            if (typeof(IFBSerializable).IsAssignableFrom(type)) {
                // not deserialized, yet. Create a new object and call the deserialize method with the flatbuffers object
                SetDeserializingFlag(incoming.BufferPosition);
                try {
                    newObject = newObject==null?(IFBSerializable)Activator.CreateInstance(type) : newObject;
                    PutIntoDeserializeCache(incoming.BufferPosition, newObject);
                    newObject.Deserialize(incoming);
                    // upgrade the object if there was a version mismatch
                    UpgradeObjectIfNeeded(newObject, incoming);

                    return newObject;
                }
                finally {
                    ClearDeserializingFlag(incoming.BufferPosition);
                }
            } else {
                try {
                    SetDeserializingFlag(incoming.BufferPosition);
                    var convResult = ConvertToObject(incoming, type);
                    if (convResult == null) {
                        UnityEngine.Debug.LogError("There is no deserializer for " + type);
                        return null;
                    }
                    return convResult;
                }
                finally {
                    ClearDeserializingFlag(incoming.BufferPosition);
                }
            }

        }

        public static void ClearCache() {
            obj2FSMapping.Clear();
            fb2objMapping.Clear();
            serializeBufferposCheck.Clear();
            deserializingATM.Clear();
            serializingATM.Clear();
            afterDeserializationAction.Clear();
            afterSerializationAction.Clear();
            foreach (var objList in postProcessObjects) {
                objList.Value.Clear();
            }
        }

        public static byte[] SerializeToBytes(IFBSerializable root)  {
            var st = new System.Diagnostics.Stopwatch();
            st.Start();

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
            st.Stop();
            UnityEngine.Debug.Log("FINISHED SERIALIZATION inner:" + st.Elapsed.TotalMilliseconds / 1000.0f + "s");
            return buf;
        }

        public static void SerializeToFileDomain(FileSystem.FSDomain domain, String filename, IFBSerializable root)  {
            var buf = SerializeToBytes(root);
            var fs = Kernel.Instance.Container.Resolve<Service.FileSystem.IFileSystemService>();
            fs.WriteBytesToFileAtDomain(domain, filename, buf);
        }

        public static T DeserializeFromBytes<T>(byte[] buf,T dataRoot=default(T)) where T : IFBSerializable,new() {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            ClearCache();
            var fbByteBuffer = new ByteBuffer(buf);
            if (dataRoot == null) {
                dataRoot = new T();
            }
            dataRoot.Deserialize(fbByteBuffer);
            foreach (var act in afterDeserializationAction) {
                act();
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log("Deserialize final took:" + stopwatch.Elapsed.TotalMilliseconds / 1000.0f);
            return dataRoot;
        }
    }
}
