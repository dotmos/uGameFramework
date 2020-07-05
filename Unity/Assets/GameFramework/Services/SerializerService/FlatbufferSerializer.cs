using System.Collections.Generic;
using System;
using FlatBuffers;
using System.Linq;
using System.Collections;
using System.Text;
using System.Threading;

namespace Service.Serializer {
    public class FlatBufferSerializer {
        
        public enum Mode {
            serializing, deserializing
        }

        public class ObjMapping {
            public int objId;
            public int bufPos;
        }

        public static ListPool<int> poolListInt = new ListPool<int>(10,10);
        public static ListPool<object> poolListObject = new ListPool<object>(10, 10);
        public static ArrayPool<int> poolArrayInt = new ArrayPool<int>();
        public static ArrayPool<bool> poolArrayBool = new ArrayPool<bool>();
        public static ArrayPool<short> poolArrayShort = new ArrayPool<short>();
        public static ArrayPool<float> poolArrayFloat = new ArrayPool<float>();

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

        public static Dictionary<string, List<ECS.UID>> uidlists = new Dictionary<string, List<ECS.UID>>();

        private static FlatBufferBuilder fbBuilder;

        public static Dictionary<Type, Func<object, FlatBufferBuilder, int>> serializeObjConverters = new Dictionary<Type, Func<object, FlatBufferBuilder, int>>();
        public static Dictionary<Type, Func<object, object>> deserializeObjConverters = new Dictionary<Type, Func<object, object>>();

        private static int currentDataFormatVersion = 1;
        public static int CurrentDataFormatVersion { get => currentDataFormatVersion; set => currentDataFormatVersion = value; }

        private static int currentDeserializingDataFormatVersion = 1;
        public static int CurrentDeserializingDataFormatVersion { get => currentDeserializingDataFormatVersion; set => currentDeserializingDataFormatVersion = value; }

        public static bool ThreadedExecution { get; set; }

        /// <summary>
        /// Is the version of the serialized data different to the current one
        /// </summary>
        public static bool DeserializationVersionMismatch {
            get => CurrentDataFormatVersion != CurrentDeserializingDataFormatVersion;
        }

        // objects that are serializing atm
        public static HashSet<object> serializingATM = new HashSet<object>();

        public static bool convertersActivated = false;


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
                int bufPos = converter(obj, builder);
                return bufPos;
            } else {
                return null;
            }
        }

        public static void Initialize() {
            if (!convertersActivated) {
                ActivateConverters();
                convertersActivated = true;
            }
        }

        public static void AddEntityToUIDList(string uidList, ECS.UID uid) {
            if (uidlists.TryGetValue(uidList,out List<ECS.UID> lists)){
                lists.Add(uid);
            } else {
                lists = new List<ECS.UID>() { uid };
                uidlists[uidList] = lists;
            }
        }

        /// <summary>
        /// Try to deserialize and IFlatbufferObject via converters only to the ResultType
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="resultType"></param>
        /// <returns></returns>
        public static object ConvertToObject(object incoming,Type resultType) {
            if (resultType == null) {
                //int a = 0;
                return null;
            }

           // UnityEngine.Debug.Log("Try to convertToObject:" + incoming.GetType().FullName + " =TO=> " + resultType.FullName);
            if (deserializeObjConverters.TryGetValue(resultType, out Func<object, object> converter)) {
                object result = converter(incoming);
                return result;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Build-in Converters (Vector2,3,4,Quaternion)
        /// Add own converters via:
        /// FlatBufferSerializer.serializeObjConverters[typeof(...)] = (data,builder)=>{...}
        /// </summary>
        public static void ActivateConverters() {
            // ------------------------- Vector2 --------------------------------------
            serializeObjConverters[typeof(UnityEngine.Vector2)] = (data,builder) => {
                UnityEngine.Vector2 vec2 = (UnityEngine.Vector2)data;
                Serial.FBVector2.StartFBVector2(builder);
                Serial.FBVector2.AddX(builder, vec2.x);
                Serial.FBVector2.AddY(builder, vec2.y);
                return Serial.FBVector2.EndFBVector2(builder).Value;
            };
            deserializeObjConverters[typeof(UnityEngine.Vector2)] = (incoming) => {
                Serial.FBVector2 fbVec2 = (Serial.FBVector2)incoming;
                UnityEngine.Vector2 vec2 = new UnityEngine.Vector2(fbVec2.X, fbVec2.Y);
                return vec2;
            };
            // ------------------------- Vector3 --------------------------------------
            serializeObjConverters[typeof(UnityEngine.Vector3)] = (data, builder) => {
                UnityEngine.Vector3 vec3 = (UnityEngine.Vector3)data;

                Serial.FBVector3.StartFBVector3(builder);
                Serial.FBVector3.AddX(builder, vec3.x);
                Serial.FBVector3.AddY(builder, vec3.y);
                Serial.FBVector3.AddZ(builder, vec3.z);
                return Serial.FBVector3.EndFBVector3(builder).Value;
            };
            deserializeObjConverters[typeof(UnityEngine.Vector3)] = (incoming) => {
                Serial.FBVector3 fbVec3 = (Serial.FBVector3)incoming;
                UnityEngine.Vector3 vec3 = new UnityEngine.Vector3(fbVec3.X, fbVec3.Y, fbVec3.Z);
                return vec3;
            };
            // ------------------------- Vector4 --------------------------------------
            serializeObjConverters[typeof(UnityEngine.Vector4)] = (data, builder) => {
                UnityEngine.Vector4 vec4 = (UnityEngine.Vector4)data;
                Serial.FBVector4.StartFBVector4(builder);
                Serial.FBVector4.AddX(builder, vec4.x);
                Serial.FBVector4.AddY(builder, vec4.y);
                Serial.FBVector4.AddZ(builder, vec4.z);
                Serial.FBVector4.AddW(builder, vec4.w);
                return Serial.FBVector4.EndFBVector4(builder).Value;
            };
            deserializeObjConverters[typeof(UnityEngine.Vector4)] = (incoming) => {
                Serial.FBVector4 fbVec4 = (Serial.FBVector4)incoming;
                UnityEngine.Vector4 vec4 = new UnityEngine.Vector4(fbVec4.X, fbVec4.Y, fbVec4.Z, fbVec4.W);
                return vec4;
            };
            // ------------------------- Quaternion --------------------------------------
            serializeObjConverters[typeof(UnityEngine.Quaternion)] = (data, builder) => {
                UnityEngine.Quaternion q = (UnityEngine.Quaternion)data;
                Serial.FBQuaternion.StartFBQuaternion(builder);
                Serial.FBQuaternion.AddX(builder, q.x);
                Serial.FBQuaternion.AddY(builder, q.y);
                Serial.FBQuaternion.AddZ(builder, q.z);
                Serial.FBQuaternion.AddW(builder, q.w);
                return Serial.FBQuaternion.EndFBQuaternion(builder).Value;
            };
            deserializeObjConverters[typeof(UnityEngine.Quaternion)] = (incoming) => {
                Serial.FBQuaternion fbQuaternion = (Serial.FBQuaternion)incoming;
                UnityEngine.Quaternion quat = new UnityEngine.Quaternion(fbQuaternion.X, fbQuaternion.Y, fbQuaternion.Z, fbQuaternion.W);
                return quat;
            };
            // ------------------------- StringBuilder --------------------------------------
            serializeObjConverters[typeof(System.Text.StringBuilder)] = (data, builder) => {
                StringOffset stbData = builder.CreateString(((StringBuilder)data).ToString());
                Serial.FBStringBuilder.StartFBStringBuilder(builder);
                Serial.FBStringBuilder.AddStringData(builder,stbData);            
                return Serial.FBStringBuilder.EndFBStringBuilder(builder).Value;
            };
            deserializeObjConverters[typeof(System.Text.StringBuilder)] = (incoming) => {
                Serial.FBStringBuilder fbStringBuilder = (Serial.FBStringBuilder)incoming;
                return new StringBuilder(fbStringBuilder.StringData);
            };
        }

        private static FlatBuffers.VectorOffset SerializeTempOffsetArray<S>(FlatBufferBuilder builder, FlatBuffers.Offset<S>[] tempArray) where S : struct, FlatBuffers.IFlatbufferObject {
            builder.StartVector(4, tempArray.Length, 4); builder.Add(tempArray);
            VectorOffset result = builder.EndVector();
            return result;
        }
        /// <summary>
        /// Serializes a dictionary 
        /// </summary>
        /// <typeparam name="TKey">Key of the initalDict</typeparam>
        /// <typeparam name="TValue">ValueType of the initalDict</typeparam>
        /// <typeparam name="FBKey">Flatbuffer-Keytype</typeparam>
        /// <typeparam name="FBValue">Flatbuffer-ValueType</typeparam>
        /// <typeparam name="S">Flatbuffer-Type in the (internal) result-list e.g. Serial.DT_int_int</typeparam>
        /// <param name="builder">flatbuffer-builder</param>
        /// <param name="dict">the dictionary</param>
        /// <param name="fbCreateElement">flatbuffer-generated method to create an element</param>
        /// <param name="fbCreateList">flatbuffer-generated method to create the whole list</param>
        /// <returns></returns>
        public static FlatBuffers.VectorOffset? CreateDictionary<TKey, TValue,FBKey,FBValue,S>(FlatBuffers.FlatBufferBuilder builder
                                , IDictionary<TKey, TValue> dict
                                , Func<FlatBufferBuilder, FBKey,FBValue, Offset<S>> fbCreateElement
                                , Func<FlatBufferBuilder, Offset<S>[], VectorOffset> fbCreateList=null
                                , bool ignoreCache=true, bool keyTyped=false, bool valueTyped=false)
                                where S : struct, FlatBuffers.IFlatbufferObject where FBValue : struct {
            if (dict == null) {
                return null;
            }


            int? bufferPos = ignoreCache?null:FindInSerializeCache(dict);
            if (bufferPos.HasValue) {
                return new VectorOffset(bufferPos.Value);
            }
            //var tempArray = new FlatBuffers.Offset<S>[dict.Count];
            lock (dict) {
                List<int> offsetList = poolListInt.GetList(dict.Count);
                int amount = dict.Count;

                bool keyPrimOrEnum = typeof(TKey).IsPrimitive || typeof(TKey).IsEnum;
                bool valuePrimOrEnum = typeof(TValue).IsPrimitive || typeof(TValue).IsEnum;

                if (keyPrimOrEnum && valuePrimOrEnum) {
                    SetSerializingFlag(dict);
                    // a pure primitive dictionary
                    foreach (KeyValuePair<TKey, TValue> dictElem in dict) {
                        offsetList.Add(fbCreateElement(builder, (FBKey)((object)dictElem.Key), (FBValue)((object)dictElem.Value)).Value);
                    }
                    //var result = fbCreateList != null
                    //                ? fbCreateList(builder, tempArray)
                    //                : SerializeTempOffsetArray(builder, tempArray);
                    VectorOffset result =new VectorOffset(builder.CreateOffsetVector(offsetList));

                    if (!ignoreCache) PutInSerializeCache(dict, result.Value);
                    ClearSerializingFlag(dict);
                    poolListInt.Release(offsetList);
                    return result;
                } else if (keyPrimOrEnum && !valuePrimOrEnum) {
                    SetSerializingFlag(dict);
                    foreach (KeyValuePair<TKey, TValue> dictElem in dict) {

                        FBValue valueElemOffset;
                        if (typeof(TValue) == typeof(string)) {
                            valueElemOffset = (FBValue)(object)builder.CreateString((string)(object)dictElem.Value);
                        } else if (dictElem.Value is IList && dictElem.Value.GetType().IsGenericType) {
                            Type dictElemValueType = dictElem.Value.GetType();
                            Type listType = dictElemValueType.GetGenericTypeDefinition();
                            valueElemOffset = (FBValue)(object)FlatBufferSerializer.CreateManualList(builder, (IList)dictElem.Value, listType);
                        } else if (dictElem.Value is IObservableList) {
                            IObservableList observableList = (IObservableList)dictElem.Value;
                            Type listType = observableList.GetListType();
                            valueElemOffset = (FBValue)(object)FlatBufferSerializer.CreateManualList(builder, observableList.InnerIList, listType);
                        } else {
                            int? offset = valueTyped ? FlatBufferSerializer.SerializeTypedObject(builder, dictElem.Value)
                                                    : FlatBufferSerializer.GetOrCreateSerialize(builder, dictElem.Value);
                            valueElemOffset = (FBValue)Activator.CreateInstance(typeof(FBValue), offset);
                        }
                        offsetList.Add(fbCreateElement(builder, (FBKey)((object)dictElem.Key), valueElemOffset).Value);
                    }
                    //var result = fbCreateList != null
                    //                ? fbCreateList(builder, tempArray)
                    //                : SerializeTempOffsetArray(builder, tempArray);
                    VectorOffset result = new VectorOffset(builder.CreateOffsetVector(offsetList));

                    if (!ignoreCache) PutInSerializeCache(dict, result.Value);
                    ClearSerializingFlag(dict);
                    poolListInt.Release(offsetList);
                    return result;
                } else if (!keyPrimOrEnum && valuePrimOrEnum) {
                    SetSerializingFlag(dict);
                    foreach (KeyValuePair<TKey, TValue> dictElem in dict) {

                        FBKey offsetKey;
                        if (typeof(TKey) == typeof(string)) {
                            offsetKey = (FBKey)(object)builder.CreateString((string)(object)dictElem.Key);
                        } else {
                            int? keyElemOffset = keyTyped ? FlatBufferSerializer.SerializeTypedObject(builder, dictElem.Key)
                                                         : FlatBufferSerializer.GetOrCreateSerialize(builder, dictElem.Key);
                            offsetKey = (FBKey)Activator.CreateInstance(typeof(FBKey), keyElemOffset);
                        }

                        offsetList.Add(fbCreateElement(builder, offsetKey, (FBValue)((object)dictElem.Value)).Value);
                    }
                    //var result = fbCreateList != null
                    //                ? fbCreateList(builder, tempArray)
                    //                : SerializeTempOffsetArray(builder, tempArray);
                    VectorOffset result = new VectorOffset(builder.CreateOffsetVector(offsetList));

                    if (!ignoreCache) PutInSerializeCache(dict, result.Value);
                    ClearSerializingFlag(dict);
                    poolListInt.Release(offsetList);
                    return result;
                } else if (!keyPrimOrEnum && !valuePrimOrEnum) {
                    SetSerializingFlag(dict);
                    foreach (KeyValuePair<TKey, TValue> dictElem in dict) {
                        FBKey offsetKey;
                        if (typeof(TKey) == typeof(string)) {
                            offsetKey = (FBKey)(object)builder.CreateString((string)(object)dictElem.Key);
                        } else {
                            int? keyElemOffset = keyTyped ? FlatBufferSerializer.SerializeTypedObject(builder, dictElem.Key)
                                                         : FlatBufferSerializer.GetOrCreateSerialize(builder, dictElem.Key);
                            offsetKey = (FBKey)Activator.CreateInstance(typeof(FBKey), keyElemOffset);
                        }

                        FBValue valueElemOffset;
                        if (typeof(TValue) == typeof(string)) {
                            valueElemOffset = (FBValue)(object)builder.CreateString((string)(object)dictElem.Key);
                        } else {
                            int? offset = valueTyped ? FlatBufferSerializer.SerializeTypedObject(builder, dictElem.Value)
                                                    : FlatBufferSerializer.GetOrCreateSerialize(builder, dictElem.Value);

                            valueElemOffset = (FBValue)Activator.CreateInstance(typeof(FBValue), offset);
                        }
                        offsetList.Add(fbCreateElement(builder, offsetKey, valueElemOffset).Value);
                    }
                    //var result = fbCreateList != null
                    //                ? fbCreateList(builder, tempArray)
                    //                : SerializeTempOffsetArray(builder, tempArray);
                    VectorOffset result = new VectorOffset(builder.CreateOffsetVector(offsetList));

                    if (!ignoreCache) PutInSerializeCache(dict, result.Value);
                    ClearSerializingFlag(dict);
                    poolListInt.Release(offsetList);
                    return result;
                }
                return null;
            }
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
                                        , IList<T> list, Func<FlatBufferBuilder, Offset<S>[], VectorOffset> fbCreateList,bool ignoreCache=true)
                                        where S : struct, FlatBuffers.IFlatbufferObject where T : IFBSerializable {
            if (list == null) {
                return new VectorOffset(0);
            }

            int? bufferPos = ignoreCache?null:FindInSerializeCache(list);
            if (bufferPos.HasValue) {
                return new VectorOffset(bufferPos.Value);
            }

            if (typeof(IFBSerializable).IsAssignableFrom(typeof(T))) {
                SetSerializingFlag(list);
                //var tempArray = new FlatBuffers.Offset<S>[list.Count];
                List<int> offsetList = poolListInt.GetList(list.Count);
                for (int i = 0; i < list.Count; i++) {
                    int? listElemOffset = FlatBufferSerializer.GetOrCreateSerialize(builder, (IFBSerializable)list[i]);
                    if (listElemOffset != null) {
                        offsetList.Add(listElemOffset.Value);
                    }
                }
                VectorOffset result = new VectorOffset(builder.CreateOffsetVector(offsetList));
                //var result = fbCreateList(builder, tempArray);
                if (!ignoreCache) PutInSerializeCache(list, result.Value);
                ClearSerializingFlag(list);
                poolListInt.Release(offsetList);
                return result;
            }

            return null;
        }


        /// <summary>
        /// Get typename including Assembly-Name
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        public static String GetTypeName(object elem) {
            Type type = elem.GetType();
            return GetTypeName(type);
        }

        static Dictionary<Type, String> typeNameLookupTable = new Dictionary<Type, string>();

        static StringBuilder stb = new StringBuilder();

        public static String GetTypeName(Type type) {
            if (typeNameLookupTable.TryGetValue(type,out string value)) {
                return value;
            } else {
                stb.Clear();
                string assemblyName = type.Assembly.FullName;
                    
                stb.Append(type.FullName).Append(", ").Append(assemblyName.Substring(0, assemblyName.IndexOf(',')));
                string typeName = stb.ToString();
                typeNameLookupTable[type] = typeName;
                return typeName;
            }
        }

        /// <summary>
        /// Create a list and store the objects types to make a deserialization of list with inherited types possible
        /// (e.g. List<ITask>...)
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="dataList"></param>
        /// <returns></returns>
        public static VectorOffset CreateTypedList<T>(FlatBufferBuilder builder, IList<T> dataList,bool ignoreCache=false) {
            if (dataList == null) {
                return new VectorOffset(0);
            }

            int? bufferPos = ignoreCache ? null : FindInSerializeCache(dataList);
            if (bufferPos.HasValue) {
                return new VectorOffset(bufferPos.Value);
            }
            if (!ignoreCache) SetSerializingFlag(dataList);


            //                List<int> listOfOffsets = new List<int>(dataList.Count * 2);
            List<int> listOfOffsets = poolListInt.GetList(dataList.Count * 2);
            int amount = dataList.Count;
            for (int i=0;i<amount;i++) {
                T elem = dataList[i];
                if (elem == null) {
                    listOfOffsets.Add(0);
                    listOfOffsets.Add(0);
                    continue;
                }
                string typeName = GetTypeName(elem);
                StringOffset offsetTypeName = builder.CreateSharedString(typeName);
                int? offsetData = FlatBufferSerializer.GetOrCreateSerialize(builder, elem);
                listOfOffsets.Add(offsetTypeName.Value);
                listOfOffsets.Add(offsetData.Value);
            }
            builder.StartVector(4, listOfOffsets.Count, 4);
            for (int i = listOfOffsets.Count - 1; i >= 0; i--) builder.AddOffset(listOfOffsets[i]);

            VectorOffset result = builder.EndVector();
            poolListInt.Release(listOfOffsets);

            if (!ignoreCache) {
                PutInSerializeCache(dataList, result.Value);
                ClearSerializingFlag(dataList);
            }

            return result;
        }

        /// <summary>
        /// Serialize manual byte-array
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static FlatBuffers.VectorOffset CreateManualArray(FlatBuffers.FlatBufferBuilder builder, byte[] data) {
            builder.StartVector(1, data.Length, 1);
            builder.Add<byte>(data);
            return builder.EndVector();
        }

        /// <summary>
        /// Serializes array  (supported types: bool,float,int,long,string,IFBSerializable-Objects)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static FlatBuffers.VectorOffset CreateManualArray<T>(FlatBuffers.FlatBufferBuilder builder, T[] data,bool ignoreList=true)  {
            if (data == null) {
                return new VectorOffset(0);
            }

            int? bufferPos = ignoreList?null:FindInSerializeCache(data);
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
            } else if (typeof(T) == typeof(short)) {
                builder.StartVector(2, data.Length, 2); for (int i = data.Length - 1; i >= 0; i--) builder.AddShort((short)(object)data[i]);
            } else if (typeof(T) == typeof(long)) {
                builder.StartVector(8, data.Length, 8); for (int i = data.Length - 1; i >= 0; i--) builder.AddLong((long)(object)data[i]);
            } else if (typeof(T) == typeof(string)) {
                int amount = data.Length;
                List<int> stOffsetList = poolListInt.GetList(amount);
                for (int i = 0; i < amount; i++) stOffsetList.Add(builder.CreateString((string)(object)data[i]).Value);
                builder.StartVector(4, data.Length, 4); for (int i = amount - 1; i >= 0; i--) builder.AddOffset(stOffsetList[i]);
                poolListInt.Release(stOffsetList);
            } else if (typeof(T) == typeof(byte)) {
                UnityEngine.Debug.LogError("CreateManual-Array not supported for byte! Use CreateManualByteArray");
            } else {
                int amount = data.Length;
                //List<int> stOffsetList = new List<int>(amount);
                List<int> stOffsetList = poolListInt.GetList(amount);
                for (int i = 0; i < amount; i++) {
                    int? elem = GetOrCreateSerialize(builder, data[i]);
                    stOffsetList.Add(elem.Value);
                }
                builder.StartVector(4, data.Length, 4); for (int i = amount - 1; i >= 0; i--) builder.AddOffset(stOffsetList[i]);
                poolListInt.Release(stOffsetList);
            }
            VectorOffset result = builder.EndVector();
            if (!ignoreList) PutInSerializeCache(data, result.Value);
            ClearSerializingFlag(data);
            return result;
        }


        /// <summary>
        /// Serializes a list (supported types: bool,float,int,long,string,IFBSerializable-Objects)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static FlatBuffers.VectorOffset CreateManualList<T>(FlatBuffers.FlatBufferBuilder builder, IList<T> data, bool ignoreCache=true) {
            if (data is ObservableList<T>) {
                VectorOffset result = CreateManualList(builder, (IList)(((ObservableList<T>)data).InnerList), typeof(T),ignoreCache);
                return result;
            } else {
                VectorOffset result = CreateManualList(builder, (IList)data, typeof(T),ignoreCache);
                return result;
            }
            
        }

        public static FlatBuffers.VectorOffset CreateNonPrimManualList<T>(FlatBuffers.FlatBufferBuilder builder, HashSet<T> data, bool ignoreCache=false) {
            if (data == null) {
                return new VectorOffset(0);
            }

            int? bufferPos = ignoreCache?null:FindInSerializeCache(data);
            if (bufferPos.HasValue) {
                return new VectorOffset(bufferPos.Value);
            }
            if (!ignoreCache) SetSerializingFlag(data);
            int amount = data.Count;
            // List<int> stOffsetList = new List<int>(amount);
            List<int> stOffsetList = poolListInt.GetList(amount);
                
            foreach (T dataElem in data) {
                int? elem = GetOrCreateSerialize(builder, dataElem);
                stOffsetList.Add(elem.Value);
            }
            builder.StartVector(4, data.Count, 4); for (int i = amount - 1; i >= 0; i--) builder.AddOffset(stOffsetList[i]);
            poolListInt.Release(stOffsetList);

            VectorOffset result = builder.EndVector();

            if (!ignoreCache) {
                PutInSerializeCache(data, result.Value);
                ClearSerializingFlag(data);
            }
            return result;
        }


        public static FlatBuffers.VectorOffset CreateManualList(FlatBuffers.FlatBufferBuilder builder,IList data,Type type,bool ignoreCache=true) {
            if (data == null) {
                return new VectorOffset(0);
            }

            int? bufferPos = ignoreCache?null:FindInSerializeCache(data);
            if (bufferPos.HasValue) {
                return new VectorOffset(bufferPos.Value);
            }
            if (!ignoreCache) SetSerializingFlag(data);
            if (type == typeof(bool)) {
                builder.StartVector(1, data.Count, 1); for (int i = data.Count - 1; i >= 0; i--) builder.AddBool((bool)(object)data[i]);
            } else if (type == typeof(short)) {
                builder.StartVector(2, data.Count, 2); for (int i = data.Count - 1; i >= 0; i--) builder.AddShort((short)(object)data[i]);
            } else if (type == typeof(float)) {
                builder.StartVector(4, data.Count, 4); for (int i = data.Count - 1; i >= 0; i--) builder.AddFloat((float)(object)data[i]);
            } else if (type == typeof(int) || type.IsEnum) {
                builder.StartVector(4, data.Count, 4); for (int i = data.Count - 1; i >= 0; i--) builder.AddInt((int)(object)data[i]);
            } else if (type == typeof(long)) {
                builder.StartVector(8, data.Count, 8); for (int i = data.Count - 1; i >= 0; i--) builder.AddLong((long)(object)data[i]);
            } else if (type == typeof(string)) {
                int amount = data.Count;
                //                    List<StringOffset> stOffsetList = new List<StringOffset>(amount);
                List<int> stOffsetList = poolListInt.GetList(amount);
                for (int i = 0; i < amount; i++) stOffsetList.Add(builder.CreateString((string)(object)data[i]).Value);
                builder.StartVector(4, data.Count, 4); for (int i = amount - 1; i >= 0; i--) builder.AddOffset(stOffsetList[i]);
                poolListInt.Release(stOffsetList);
            } else {
                int amount = data.Count;
                // List<int> stOffsetList = new List<int>(amount);
                List<int> stOffsetList = poolListInt.GetList(amount);
                for (int i = 0; i < amount; i++) {
                    int? elem = GetOrCreateSerialize(builder, data[i]);
                    stOffsetList.Add(elem.Value);
                }
                builder.StartVector(4, data.Count, 4); for (int i = amount - 1; i >= 0; i--) builder.AddOffset(stOffsetList[i]);
                poolListInt.Release(stOffsetList);
            }
            VectorOffset result = builder.EndVector();
            if (!ignoreCache) {
                PutInSerializeCache(data, result.Value);
                ClearSerializingFlag(data);
            }
            return result;
        }

        /// <summary>
        /// Creates a string list
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="list"></param>
        /// <param name="fbCreateList"></param>
        /// <returns></returns>
        public static FlatBuffers.VectorOffset? CreateStringList(FlatBuffers.FlatBufferBuilder builder
                                    , List<string> list, Func<FlatBufferBuilder, StringOffset[],VectorOffset> fbCreateList,bool ignoreCache=true) {
            if (list == null) {
                return new VectorOffset(0);
            }


            int? bufferPos = ignoreCache?null:FindInSerializeCache(list);
            if (bufferPos.HasValue) {
                return new VectorOffset(bufferPos.Value);
            }
            StringOffset[] tempArray = list.Select(st => builder.CreateString(st)).ToArray();
            // call the createFunction with the array
            VectorOffset result = fbCreateList(builder, tempArray);
            if (!ignoreCache) PutInSerializeCache(list, result.Value);
            return result;
        }


        /// <summary>
        /// Create list with help of FB-generated methods (better use CreateManualList)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="list"></param>
        /// <param name="fbCreateList"></param>
        /// <returns></returns>
        public static FlatBuffers.VectorOffset? CreateList<T>(FlatBuffers.FlatBufferBuilder builder
                                    , List<T> list, Func<FlatBufferBuilder, T[], VectorOffset> fbCreateList, bool ignoreCache=true) {
            /*if (list == null || typeof(T).IsPrimitive) {
                return null;
            }*/
            if (list == null) {
                return new VectorOffset(0);
            }

            int? bufferPos = ignoreCache?null:FindInSerializeCache(list);
            if (bufferPos.HasValue) {
                return new VectorOffset(bufferPos.Value);
            }
            if (!ignoreCache) SetSerializingFlag(list);
            T[] tempArray = list.ToArray();
            // call the createFunction with the array
            VectorOffset result = fbCreateList(builder, tempArray);

            if (!ignoreCache) {
                PutInSerializeCache(list, result.Value);
                ClearSerializingFlag(list);
            }
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
        public static ICollection<T> DeserializeList<T,S>(int bufferPos, int amount,List<object> items,ICollection<T> result=null,bool isObservableList=false) where S : IFlatbufferObject where T : new() {
            if (bufferPos == 0) {
                return null;
            }
            object cachedResult = null;
            //var cachedResult = FindInDeserializeCache<List<T>>(bufferPos);
            if (cachedResult != null) {
                try {
                    return (List<T>)cachedResult;
                }
                catch (Exception e) {
                    UnityEngine.Debug.LogException(e);
                    UnityEngine.Debug.Log("T:" + typeof(T) + " S:" + typeof(S) + " bufferpos:" + bufferPos);
                    UnityEngine.Debug.Log("Cached-ResultType:" + cachedResult.GetType() + "\n");
                    return null;
                }
            } else {
            //    SetDeserializingFlag(bufferPos);
                if (result == null) {
                    result = isObservableList ? new ObservableList<T>(amount) : (IList<T>)new List<T>(amount);
                }
                //PutIntoDeserializeCache(bufferPos, result);
                for (int i = 0; i < amount; i++) {
                    object obj = items[i];
                    if (obj != null) {
                        T deserializedElement = FlatBufferSerializer.GetOrCreateDeserialize<T>((S)obj);
                        result.Add(deserializedElement);
                    }
                }
                // ClearDeserializingFlag(bufferPos);
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
        public static object FindInDeserializeCache<T>(int bufferpos)  {
            if (bufferpos == 0 || typeof(T).IsValueType) return null;

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
            if (obj is IFBDontCache || obj.GetType().IsValueType) {
                return;
            }

            if (checkIfExists && FindInDeserializeCache<object>(bufferpos) != null) {
                object beforeObj = FindInDeserializeCache<object>(bufferpos);
                UnityEngine.Debug.LogError("WAAARNING: you are overwriting an existing object in deserialize-cache! before:" + beforeObj.GetType() + " new:" + obj.GetType());
            }
            fb2objMapping[bufferpos] = obj;
            UnityEngine.Profiling.Profiler.BeginSample("IFBPostDeserialization");
            if (!(obj is Service.IService)) {
                if (obj is IFBPostDeserialization) {
                    if (postProcessObjects.TryGetValue(obj.GetType(), out List<object> objList)) {
                        objList.Add(obj);
                    } else {
                        postProcessObjects[obj.GetType()] = new List<object>() { obj };
                    }
                }

            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

        /// <summary>
        /// Triggers to call all IFBPostDeserialzation-Object's OnPostDeserialization-Method that where just deserialized
        /// </summary>
        /// <param name="userobject"></param>
        public static void ProcessPostProcessing(object userobject) {
            ECS.IEntityManager entityManager = Kernel.Instance.Resolve<ECS.IEntityManager>();
            // post-process objects that are marked via FlatbufferSerializer.AddPostProcessType(type)
            int savedFormat = FlatBufferSerializer.CurrentDeserializingDataFormatVersion; // the format of the file just loaded
            int currentFormat = FlatBufferSerializer.CurrentDataFormatVersion; // the current format of this codebase            

            foreach (KeyValuePair<Type, List<object>> postProcessObj in FlatBufferSerializer.postProcessObjects) {
                List<object> postProcessObjList = postProcessObj.Value;
                for (int j = 0; j < postProcessObjList.Count; j++) {
                    object elem = postProcessObjList[j];
                    if (elem is IFBPostDeserialization) {
                        ((IFBPostDeserialization)elem).OnPostDeserialization(entityManager, userobject,savedFormat, currentFormat,false);
                    }
                }
            }
        }

        public static int? FindInSerializeCache(object obj){
            if (obj == null || obj.GetType().IsValueType) {
                return null;
            }
            if (obj2FSMapping.TryGetValue(obj, out int value)) {
                return value;
            }
            return null;
        }


        public static int putInDiscardValueTypes = 0;

        public static void PutInSerializeCache(object obj, int bufferpos, bool checkIfExists = true)  {
            if (obj.GetType().IsValueType || obj is IFBDontCache) {
                putInDiscardValueTypes++;
                return;
            }
            if (obj == null) {
                UnityEngine.Debug.LogWarning("Tried to put null-object at pos:" + bufferpos);
                return;
            }
            if (checkIfExists && serializeBufferposCheck.ContainsKey(bufferpos) && serializeBufferposCheck[bufferpos] != (object)obj) {
                object beforeObj = serializeBufferposCheck[bufferpos];
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

            // var result = FindInDeserializeCache<List<T>>(bufferPos);
            object result = null;
            if (result != null) {
                // we already deserialized this list give back the already created version to keep the reference
                return (List<T>)result;
            } else {
                List<T> newList = new List<T>(getArray());
                //PutIntoDeserializeCache(bufferPos, newList);
                return newList;
            }
        }

        /// <summary>
        /// Serializes a string
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="serializableObj"></param>
        /// <returns></returns>
        public static StringOffset GetOrCreateSerialize(FlatBufferBuilder builder, string serializableObj, bool ignoreCache=false) {
            if (serializableObj == null) {
                return new StringOffset(0);
            }

            // check if we have this Object already serialized and if yes grab the
            // location in the buffer and pass it as offset, so it can be pointed to this location again
            int? bufferPos = ignoreCache?null:FindInSerializeCache(serializableObj);
            if (bufferPos.HasValue) {
                return new StringOffset(bufferPos.Value);
            }

            StringOffset serializedString = builder.CreateString(serializableObj);
            if (!ignoreCache) PutInSerializeCache(serializedString, serializedString.Value);
            return serializedString;
        }

        /// <summary>
        /// Creates a manual object with which you can access the table of the current IFlatbufferObject
        /// </summary>
        /// <param name="incoming"></param>
        /// <returns></returns>
        public static FBManualObject GetManualObject(object incoming) {
            FBManualObject fbManual = new FBManualObject();
            fbManual.__initFromRef((IFlatbufferObject)incoming);
            return fbManual;
        }

        /// <summary>
        /// Put another Serialization-Layer on top of the incoming's buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="incoming">Must be IFlatbufferObject</param>
        /// <returns></returns>
        public static T CastSerialObject<T>(object incoming) where T : IFlatbufferObject, new() {
            IFlatbufferObject fbObj = (IFlatbufferObject)incoming;
            T t = new T();
            t.__init(fbObj.BufferPosition, fbObj.ByteBuffer);
            return t;
        }

        /// <summary>
        /// Seríalize an object. If serializableObj is IFBSerializable using its Serialize-Method, otherwise it tries to
        /// use provided converters to create a serialization (e.g. Vector3)
        /// If the serialized object has already been serialized it will return the cached offset
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="serializableObj"></param>
        /// <returns></returns>
        public static int? GetOrCreateSerialize(FlatBufferBuilder builder, object serializableObj, bool ignoreCache=false)  {
            if (serializableObj == null) {
                return 0;
            }

            int? bufferPos = ignoreCache?null:FindInSerializeCache(serializableObj);
            if (bufferPos.HasValue) {
                return bufferPos;
            }

            if (HasSerializingFlag(serializableObj)) {
                // this object is serializing atm and this seems to be a cyclic dependency.
                // add this object to the cyclic resolver and let it the actual position been written after the 
                // whole serialization-process did finish
                return builder.AddToCyclicResolver(serializableObj);
            }

            if (serializableObj is IFBSerializable) {
                SetSerializingFlag(serializableObj);
                // first time, so serialize it with flatbuffers
                int serialized = ((IFBSerializable)serializableObj).Serialize(builder);
                if (!ignoreCache) PutInSerializeCache(serializableObj, serialized);
                ClearSerializingFlag(serializableObj);
                return serialized;
            } else {
                // try to convert
                SetSerializingFlag(serializableObj);
                int? serialized = ConvertToFlatbuffer(builder, serializableObj);
                if (serializableObj != null && serialized.HasValue) {
                    if (!ignoreCache) PutInSerializeCache(serializableObj, serialized.Value);
                }
                ClearSerializingFlag(serializableObj);
                return serialized.HasValue ? serialized : 0;
            }
        }

        //private static bool HasDeserializingFlag(int bufferPos) {
        //    return deserializingATM.Contains(bufferPos);
        //}


        //private static void SetDeserializingFlag(int bufferPos) {
        //    try {
        //        UnityEngine.Profiling.Profiler.BeginSample("SetDeserializingFlag");
        //        if (deserializingATM.Contains(bufferPos)) {
        //            UnityEngine.Debug.LogError("Try to set a bufferpos to deserialize that is already in! bPos:" + bufferPos);
        //        }
        //        deserializingATM.Add(bufferPos);
        //    }
        //    finally {
        //        UnityEngine.Profiling.Profiler.EndSample();
        //    }
        //}

        //private static void ClearDeserializingFlag(int bufferPos) {
        //    try {
        //        UnityEngine.Profiling.Profiler.BeginSample("ClearDeserializingFlag");
        //        deserializingATM.Remove(bufferPos);
        //    }
        //    finally {
        //        UnityEngine.Profiling.Profiler.EndSample();
        //    }
        //}

        public static int discardSetSerializingFlagValueType = 0;

        private static void SetSerializingFlag(object serializeObj) {
            if (serializeObj.GetType().IsValueType) {
                discardSetSerializingFlagValueType++;
                return;
            }

            if (serializingATM.Contains(serializeObj)) {
                UnityEngine.Debug.LogError("Try to set an object to serializeFlag that is already in! type:" + serializeObj.GetType());
            }
            serializingATM.Add(serializeObj);
        }

        private static bool HasSerializingFlag(object obj) {
            if (obj.GetType().IsValueType) return false;
            return serializingATM.Contains(obj);
        }

        private static void ClearSerializingFlag(object obj) {
            if (obj.GetType().IsValueType) return;
            serializingATM.Remove(obj);
        }

        private static void UpgradeObjectIfNeeded(object deserializedObject,object incomingData) {
            if (deserializedObject is IFBUpgradeable) {
                // the data we just deserialized has another version. Try to convert it to have valid data
                ((IFBUpgradeable)deserializedObject).Upgrade(CurrentDeserializingDataFormatVersion, CurrentDataFormatVersion, incomingData,false);
            }
        }

        /// <summary>
        /// Deserializes a flatbufferobject to the type and (reuses newObject if provided)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="incoming"></param>
        /// <param name="newObject"></param>
        /// <returns></returns>
        public static T GetOrCreateDeserialize<T>(IFlatbufferObject incoming, IFBSerializable newObject = null) where T : new() {
            if (incoming == null || incoming.BufferPosition == 0) {
                return default(T);
            }
            object result = GetOrCreateDeserialize(incoming, typeof(T), newObject);
            return (T)result;
        }

        /// <summary>
        /// Deserializes a flatbufferobject to the type and (reuses newObject if provided)
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="type"></param>
        /// <param name="newObject"></param>
        /// <returns></returns>
        /// 

        private static readonly Type IFBSerializableType = typeof(IFBSerializable);
        private static readonly Type ScriptableObjectType = typeof(UnityEngine.ScriptableObject);

        public static object GetOrCreateDeserialize(IFlatbufferObject incoming, Type type, IFBSerializable newObject = null, bool forceSet = false) {
            if (incoming == null || incoming.BufferPosition == 0) {
                return null;
            }

            if (IFBSerializableType.IsAssignableFrom(type)) {
                // first check if we already deserialize the object at this position
                    
                lock (fb2objMapping) {
                    object result = type.IsValueType ? null : FindInDeserializeCache<object>(incoming.BufferPosition);
                    if (!forceSet && result != null) {
                        //UnityEngine.Debug.Log("Incoming-Type:"+incoming.GetType()+" Casting to :"+type.ToString());
                        // yeah, we found it. no need to create a new object we got it already
                        return result;
                    } else {
                        newObject = newObject ?? (IFBSerializable)result;
                    }

                    if (newObject == null) {
                        if (type.IsSubclassOf(ScriptableObjectType)) {
                            //Create scriptable object instance
                            newObject = (IFBSerializable)UnityEngine.ScriptableObject.CreateInstance(type);
                        } else {
                            //Generic class instancing
                            newObject = (IFBSerializable)Activator.CreateInstance(type);
                        }    
                    }
                    
                    PutIntoDeserializeCache(incoming.BufferPosition, newObject, !forceSet);
                }


                newObject.Deserialize(incoming);
                // upgrade the object if there was a version mismatch
                UpgradeObjectIfNeeded(newObject, incoming);

                return newObject;
            } else {
                try {
                    UnityEngine.Profiling.Profiler.BeginSample("Convert");
                    //  SetDeserializingFlag(incoming.BufferPosition);
                    UnityEngine.Profiling.Profiler.BeginSample("conv-deserialize");
                    object convResult = ConvertToObject(incoming, type);
                    UnityEngine.Profiling.Profiler.EndSample();
                    if (convResult == null) {
                        UnityEngine.Debug.LogError("There is no deserializer for " + type);
                        return null;
                    }
                    return convResult;
                }
                finally {
                    // ClearDeserializingFlag(incoming.BufferPosition);
                    UnityEngine.Profiling.Profiler.EndSample();
                }
            }
        }

        /// <summary>
        /// Serialize an object accompanied with its c#-Type as (shared)string (every type is 'physically' only serialized once)
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int? SerializeTypedObject(FlatBufferBuilder builder,object obj) {
            if (obj == null) return 0;


            string typeName = GetTypeName(obj);
            StringOffset offsetTypeName = builder.CreateSharedString(typeName);
            int? offsetData = FlatBufferSerializer.GetOrCreateSerialize(builder, obj);
            builder.StartTable(2);
            builder.AddOffset(0, offsetTypeName.Value, 0);
            builder.AddOffset(1, offsetData.Value, 0);
            int result = builder.EndTable();

            return result;
        }

        /// <summary>
        /// Deserialize a typed object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="incoming"></param>
        /// <returns></returns>
        public static T DeserializeTypedObject<T>(object incoming) {
            FBManualObject manual = GetManualObject(incoming);
            string typeName = manual.GetString(0);
            if (typeName == null) {
                return default(T);
            }
            Type type = Type.GetType(typeName);
            Serial.FBRef fbObj = manual.CreateSerialObject<Serial.FBRef>(1);
            object result = FlatBufferSerializer.GetOrCreateDeserialize(fbObj, type);
            return (T)result;
        }

        public static void ClearCache() {
            obj2FSMapping.Clear();
            fb2objMapping.Clear();
            serializeBufferposCheck.Clear();
            serializingATM.Clear();
            foreach (KeyValuePair<Type, List<object>> objList in postProcessObjects) {
                objList.Value.Clear();
            }
        }

        public static byte[] SerializeToBytes(IFBSerializable root,int initialBufferSize=5000000)  {
            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            st.Start();

            serializing = true;
            ClearCache();
            fbBuilder = new FlatBufferBuilder(initialBufferSize);

            int rootResult = root.Serialize(fbBuilder);
            fbBuilder.FlushCyclicResolver();
            fbBuilder.Finish(rootResult);
            // TODO: Check: Is this the whole buffer? Or is it even more?
            byte[] buf = fbBuilder.DataBuffer.ToSizedArray();

            serializing = false;
            st.Stop();
            UnityEngine.Debug.Log("FINISHED SERIALIZATION inner:" + st.Elapsed.TotalMilliseconds / 1000.0f + "s");
            fbBuilder.Clear();
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
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            ClearCache();
            ByteBuffer fbByteBuffer = new ByteBuffer(buf);
            if (dataRoot == null) {
                if(type == null) {
                    dataRoot = new T();
                } else {
                    if (type.IsSubclassOf(ScriptableObjectType)) {
                        //Create scriptable object instance
                        object _obj = UnityEngine.ScriptableObject.CreateInstance(type);
                        dataRoot = (T)_obj;// (T)UnityEngine.ScriptableObject.CreateInstance(type);
                    }
                    else {
                        //Generic class instancing
                        dataRoot = (T)Activator.CreateInstance(type);
                    }
                    //dataRoot = (T)Activator.CreateInstance(type);
                }
                
            }
            dataRoot.Deserialize(fbByteBuffer);
            stopwatch.Stop();
            UnityEngine.Debug.Log("Deserialize final took:" + stopwatch.Elapsed.TotalMilliseconds / 1000.0f);
            return dataRoot;
        }




    }


}
