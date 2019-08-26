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

        public static bool ThreadedExecution { get; set; }

        /// <summary>
        /// Is the version of the serialized data different to the current one
        /// </summary>
        public static bool DeserializationVersionMismatch {
            get => CurrentDataFormatVersion != CurrentDeserializingDataFormatVersion;
        }

        // objects that are serializing atm
        public static HashSet<object> serializingATM = new HashSet<object>();
        // objects are deserializing atm
        public static HashSet<int> deserializingATM = new HashSet<int>();

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
                var bufPos = converter(obj, builder);
                return bufPos;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Try to deserialize and IFlatbufferObject via converters only to the ResultType
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="resultType"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Build-in Converters (Vector2,3,4,Quaternion)
        /// Add own converters via:
        /// FlatBufferSerializer.serializeObjConverters[typeof(...)] = (data,builder)=>{...}
        /// </summary>
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
                var vec2 = new UnityEngine.Vector2(fbVec2.X, fbVec2.Y);
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
        }

        private static FlatBuffers.VectorOffset SerializeTempOffsetArray<S>(FlatBufferBuilder builder, FlatBuffers.Offset<S>[] tempArray) where S : struct, FlatBuffers.IFlatbufferObject {
            builder.StartVector(4, tempArray.Length, 4); builder.Add(tempArray);
            var result = builder.EndVector();
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
                                , bool ignoreCache=true)
                                where S : struct, FlatBuffers.IFlatbufferObject where FBValue : struct {
            if (dict == null) {
                return null;
            }


            int? bufferPos = ignoreCache?null:FindInSerializeCache(dict);
            if (bufferPos.HasValue) {
                return new VectorOffset(bufferPos.Value);
            }
            UnityEngine.Profiling.Profiler.BeginSample("CreateDictionary");
            try {
                //var tempArray = new FlatBuffers.Offset<S>[dict.Count];

                var offsetList = poolListInt.GetList(dict.Count);
                int amount = dict.Count;

                var keyPrimOrEnum = typeof(TKey).IsPrimitive || typeof(TKey).IsEnum;
                var valuePrimOrEnum = typeof(TValue).IsPrimitive || typeof(TValue).IsEnum;

                if (keyPrimOrEnum && valuePrimOrEnum) {
                    SetSerializingFlag(dict);
                    // a pure primitive dictionary
                    foreach (var dictElem in dict) {
                        offsetList.Add(fbCreateElement(builder, (FBKey)((object)dictElem.Key), (FBValue)((object)dictElem.Value)).Value);
                    }
                    //var result = fbCreateList != null
                    //                ? fbCreateList(builder, tempArray)
                    //                : SerializeTempOffsetArray(builder, tempArray);
                    var result = builder.CreateOffsetVector(offsetList);

                    if (!ignoreCache) PutInSerializeCache(dict, result.Value);
                    ClearSerializingFlag(dict);
                    poolListInt.Release(offsetList);
                    return result;
                } else if (keyPrimOrEnum && !valuePrimOrEnum) {
                    SetSerializingFlag(dict);
                    foreach (var dictElem in dict) {

                        FBValue valueElemOffset;
                        if (typeof(TValue) == typeof(string)) {
                            valueElemOffset = (FBValue)(object)builder.CreateString((string)(object)dictElem.Value);
                        } else if (dictElem.Value is IList && dictElem.Value.GetType().IsGenericType) {
                            var dictElemValueType = dictElem.Value.GetType();
                            var listType = dictElemValueType.GetGenericTypeDefinition();
                            valueElemOffset = (FBValue)(object)FlatBufferSerializer.CreateManualList(builder, (IList)dictElem.Value, listType);
                        } else if (dictElem.Value is IObservableList) {
                            var observableList = (IObservableList)dictElem.Value;
                            var listType = observableList.GetListType();
                            valueElemOffset = (FBValue)(object)FlatBufferSerializer.CreateManualList(builder, observableList.InnerIList, listType);
                        } else {
                            var offset = FlatBufferSerializer.GetOrCreateSerialize(builder, dictElem.Value);
                            valueElemOffset = (FBValue)Activator.CreateInstance(typeof(FBValue), offset);
                        }
                        offsetList.Add(fbCreateElement(builder, (FBKey)((object)dictElem.Key), valueElemOffset).Value);
                    }
                    //var result = fbCreateList != null
                    //                ? fbCreateList(builder, tempArray)
                    //                : SerializeTempOffsetArray(builder, tempArray);
                    var result = builder.CreateOffsetVector(offsetList);

                    if (!ignoreCache) PutInSerializeCache(dict, result.Value);
                    ClearSerializingFlag(dict);
                    poolListInt.Release(offsetList);
                    return result;
                } else if (!keyPrimOrEnum && valuePrimOrEnum) {
                    SetSerializingFlag(dict);
                    foreach (var dictElem in dict) {

                        FBKey offsetKey;
                        if (typeof(TKey) == typeof(string)) {
                            offsetKey = (FBKey)(object)builder.CreateString((string)(object)dictElem.Key);
                        } else {
                            var keyElemOffset = FlatBufferSerializer.GetOrCreateSerialize(builder, (IFBSerializable)dictElem.Key);
                            offsetKey = (FBKey)Activator.CreateInstance(typeof(FBKey), keyElemOffset);
                        }

                        offsetList.Add(fbCreateElement(builder, offsetKey, (FBValue)((object)dictElem.Value)).Value);
                    }
                    //var result = fbCreateList != null
                    //                ? fbCreateList(builder, tempArray)
                    //                : SerializeTempOffsetArray(builder, tempArray);
                    var result = builder.CreateOffsetVector(offsetList);

                    if (!ignoreCache) PutInSerializeCache(dict, result.Value);
                    ClearSerializingFlag(dict);
                    poolListInt.Release(offsetList);
                    return result;
                } else if (!keyPrimOrEnum && !valuePrimOrEnum) {
                    SetSerializingFlag(dict);
                    foreach (var dictElem in dict) {



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
                        offsetList.Add(fbCreateElement(builder, offsetKey, valueElemOffset).Value);
                    }
                    //var result = fbCreateList != null
                    //                ? fbCreateList(builder, tempArray)
                    //                : SerializeTempOffsetArray(builder, tempArray);
                    var result = builder.CreateOffsetVector(offsetList);

                    if (!ignoreCache) PutInSerializeCache(dict, result.Value);
                    ClearSerializingFlag(dict);
                    poolListInt.Release(offsetList);
                    return result;
                }
                return null;
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
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
            UnityEngine.Profiling.Profiler.BeginSample("CreateList");

            try {
                if (typeof(IFBSerializable).IsAssignableFrom(typeof(T))) {
                    SetSerializingFlag(list);
                    //var tempArray = new FlatBuffers.Offset<S>[list.Count];
                    var offsetList = poolListInt.GetList(list.Count);
                    for (int i = 0; i < list.Count; i++) {
                        var listElemOffset = FlatBufferSerializer.GetOrCreateSerialize(builder, (IFBSerializable)list[i]);
                        if (listElemOffset != null) {
                            offsetList.Add(listElemOffset.Value);
                        }
                    }
                    var result = builder.CreateOffsetVector(offsetList);
                    //var result = fbCreateList(builder, tempArray);
                    if (!ignoreCache) PutInSerializeCache(list, result.Value);
                    ClearSerializingFlag(list);
                    poolListInt.Release(offsetList);
                    return result;
                }

                return null;
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }


        /// <summary>
        /// Get typename including Assembly-Name
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        public static String GetTypeName(object elem) {
            var type = elem.GetType();
            return GetTypeName(type);
        }

        static Dictionary<Type, String> typeNameLookupTable = new Dictionary<Type, string>();

        static StringBuilder stb = new StringBuilder();

        public static String GetTypeName(Type type) {
            UnityEngine.Profiling.Profiler.BeginSample("GetTypeName");
            try {
                if (typeNameLookupTable.TryGetValue(type,out string value)) {
                    return value;
                } else {
                    stb.Clear();
                    var assemblyName = type.Assembly.FullName;
                    
                    stb.Append(type.FullName).Append(", ").Append(assemblyName.Substring(0, assemblyName.IndexOf(',')));
                    var typeName = stb.ToString();
                    typeNameLookupTable[type] = typeName;
                    return typeName;
                }
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
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
            UnityEngine.Profiling.Profiler.BeginSample("CreateTypedList");
            try {
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
                    var elem = dataList[i];
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

                var result = builder.EndVector();
                poolListInt.Release(listOfOffsets);

                if (!ignoreCache) {
                    PutInSerializeCache(dataList, result.Value);
                    ClearSerializingFlag(dataList);
                }

                return result;
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        /// <summary>
        /// Serialize manual byte-array
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static FlatBuffers.VectorOffset CreateManualArray(FlatBuffers.FlatBufferBuilder builder, byte[] data) {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("CreateManualArray");
                builder.StartVector(1, data.Length, 1);
                builder.Add<byte>(data);
                return builder.EndVector();
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
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
            UnityEngine.Profiling.Profiler.BeginSample("CreateManualArray");
            try {
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
                        var elem = GetOrCreateSerialize(builder, data[i]);
                        stOffsetList.Add(elem.Value);
                    }
                    builder.StartVector(4, data.Length, 4); for (int i = amount - 1; i >= 0; i--) builder.AddOffset(stOffsetList[i]);
                    poolListInt.Release(stOffsetList);
                }
                var result = builder.EndVector();
                if (!ignoreList) PutInSerializeCache(data, result.Value);
                ClearSerializingFlag(data);
                return result;
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
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
                var result = CreateManualList(builder, (IList)(((ObservableList<T>)data).InnerList), typeof(T),ignoreCache);
                return result;
            } else {
                var result = CreateManualList(builder, (IList)data, typeof(T),ignoreCache);
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
            try {
                UnityEngine.Profiling.Profiler.BeginSample("CreateNonPrimManualList");

                if (!ignoreCache) SetSerializingFlag(data);
                int amount = data.Count;
                // List<int> stOffsetList = new List<int>(amount);
                List<int> stOffsetList = poolListInt.GetList(amount);
                
                foreach (var dataElem in data) {
                    var elem = GetOrCreateSerialize(builder, dataElem);
                    stOffsetList.Add(elem.Value);
                }
                builder.StartVector(4, data.Count, 4); for (int i = amount - 1; i >= 0; i--) builder.AddOffset(stOffsetList[i]);
                poolListInt.Release(stOffsetList);

                var result = builder.EndVector();

                if (!ignoreCache) {
                    PutInSerializeCache(data, result.Value);
                    ClearSerializingFlag(data);
                }
                return result;
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }

        }


        public static FlatBuffers.VectorOffset CreateManualList(FlatBuffers.FlatBufferBuilder builder,IList data,Type type,bool ignoreCache=true) {
            if (data == null) {
                return new VectorOffset(0);
            }

            int? bufferPos = ignoreCache?null:FindInSerializeCache(data);
            if (bufferPos.HasValue) {
                return new VectorOffset(bufferPos.Value);
            }
            try {
                UnityEngine.Profiling.Profiler.BeginSample("CreateManualList");
                if (!ignoreCache) SetSerializingFlag(data);
                if (type == typeof(bool)) {
                    builder.StartVector(1, data.Count, 1); for (int i = data.Count - 1; i >= 0; i--) builder.AddBool((bool)(object)data[i]);
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
                        var elem = GetOrCreateSerialize(builder, data[i]);
                        stOffsetList.Add(elem.Value);
                    }
                    builder.StartVector(4, data.Count, 4); for (int i = amount - 1; i >= 0; i--) builder.AddOffset(stOffsetList[i]);
                    poolListInt.Release(stOffsetList);
                }
                var result = builder.EndVector();
                if (!ignoreCache) {
                    PutInSerializeCache(data, result.Value);
                    ClearSerializingFlag(data);
                }
                return result;
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }

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
            try {
                UnityEngine.Profiling.Profiler.BeginSample("CreateStringList");
                var tempArray = list.Select(st => builder.CreateString(st)).ToArray();
                // call the createFunction with the array
                var result = fbCreateList(builder, tempArray);
                if (!ignoreCache) PutInSerializeCache(list, result.Value);
                return result;
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
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
            UnityEngine.Profiling.Profiler.BeginSample("CreateList");
            try {
                if (!ignoreCache) SetSerializingFlag(list);
                var tempArray = list.ToArray();
                // call the createFunction with the array
                var result = fbCreateList(builder, tempArray);

                if (!ignoreCache) {
                    PutInSerializeCache(list, result.Value);
                    ClearSerializingFlag(list);
                }
                return result;
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
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
            UnityEngine.Profiling.Profiler.BeginSample("DeserializeList<T,S>");
            try {
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
                    SetDeserializingFlag(bufferPos);
                    if (result == null) {
                        result = isObservableList ? new ObservableList<T>(amount) : (IList<T>)new List<T>(amount);
                    }
                    //PutIntoDeserializeCache(bufferPos, result);
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
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
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
            try {
                UnityEngine.Profiling.Profiler.BeginSample("FindInDeserializeCache");

                if (bufferpos == 0 || typeof(T).IsValueType) return null;

                if (fb2objMapping.TryGetValue(bufferpos, out object value)) {
                    return value;
                }
                return null;
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }


        public static void PutIntoDeserializeCache(int bufferpos,object obj,bool checkIfExists=true) {
            if (obj == null) {
                UnityEngine.Debug.LogWarning("Tried to put null-object at pos:" + bufferpos);
                return;
            }
            if (obj is IFBDontCache || obj.GetType().IsValueType) {
                return;
            }

            try {
              //  stb.Clear();
//                stb.Append("PutIntoDeserializeCache-").Append(obj.GetType().ToString());
                UnityEngine.Profiling.Profiler.BeginSample("PutIntoDeserializeCache");
                if (checkIfExists && FindInDeserializeCache<object>(bufferpos) != null) {
                    var beforeObj = FindInDeserializeCache<object>(bufferpos);
                    UnityEngine.Debug.LogError("WAAARNING: you are overwriting an existing object in deserialize-cache! before:" + beforeObj.GetType() + " new:" + obj.GetType());
                }
                fb2objMapping[bufferpos] = obj;
                UnityEngine.Profiling.Profiler.BeginSample("IFBPostDeserialization");
                if (obj is IFBPostDeserialization) {
                    if (postProcessObjects.TryGetValue(obj.GetType(), out List<object> objList)) {
                        objList.Add(obj);
                    } else {
                        postProcessObjects[obj.GetType()] = new List<object>() { obj };
                    }
                }
                UnityEngine.Profiling.Profiler.EndSample();
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }


        }

        /// <summary>
        /// Triggers to call all IFBPostDeserialzation-Object's OnPostDeserialization-Method that where just deserialized
        /// </summary>
        /// <param name="userobject"></param>
        public static void ProcessPostProcessing(object userobject) {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("ProcessPostProcessing");

                var entityManager = Kernel.Instance.Resolve<ECS.IEntityManager>();
                // post-process objects that are marked via FlatbufferSerializer.AddPostProcessType(type)
                foreach (var postProcessObj in FlatBufferSerializer.postProcessObjects) {
                    var postProcessObjList = postProcessObj.Value;
                    for (int j = 0; j < postProcessObjList.Count; j++) {
                        var elem = postProcessObjList[j];
                        if (elem is IFBPostDeserialization) {
                            ((IFBPostDeserialization)elem).OnPostDeserialization(entityManager, userobject);
                        }
                    }
                }
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
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
            try {
                UnityEngine.Profiling.Profiler.BeginSample("PutInSerializeCache");
                if (obj == null) {
                    UnityEngine.Debug.LogWarning("Tried to put null-object at pos:" + bufferpos);
                    return;
                }
                if (checkIfExists && serializeBufferposCheck.ContainsKey(bufferpos) && serializeBufferposCheck[bufferpos] != (object)obj) {
                    var beforeObj = serializeBufferposCheck[bufferpos];
                    UnityEngine.Debug.LogError("WAAARNING: you are reusing an position in serialize-cache! before:" + beforeObj + " new:" + obj);
                }
                obj2FSMapping[obj] = bufferpos;
                if (checkIfExists) {
                    serializeBufferposCheck[bufferpos] = obj;
                }
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
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
            try {
                UnityEngine.Profiling.Profiler.BeginSample("DeserializeList<T>");
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
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        /// <summary>
        /// Serializes a string
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="serializableObj"></param>
        /// <returns></returns>
        public static StringOffset? GetOrCreateSerialize(FlatBufferBuilder builder, string serializableObj, bool ignoreCache=false) {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("GetOrCreateSerialize");
                if (serializableObj == null) {
                    return new StringOffset(0);
                }

                // check if we have this Object already serialized and if yes grab the
                // location in the buffer and pass it as offset, so it can be pointed to this location again
                int? bufferPos = ignoreCache?null:FindInSerializeCache(serializableObj);
                if (bufferPos.HasValue) {
                    return new StringOffset(bufferPos.Value);
                }

                var serializedString = builder.CreateString(serializableObj);
                if (!ignoreCache) PutInSerializeCache(serializedString, serializedString.Value);
                return serializedString;

            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        /// <summary>
        /// Creates a manual object with which you can access the table of the current IFlatbufferObject
        /// </summary>
        /// <param name="incoming"></param>
        /// <returns></returns>
        public static FBManualObject GetManualObject(object incoming) {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("GetManualObject");
                var fbManual = new FBManualObject();
                fbManual.__initFromRef((IFlatbufferObject)incoming);
                return fbManual;
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        /// <summary>
        /// Put another Serialization-Layer on top of the incoming's buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="incoming">Must be IFlatbufferObject</param>
        /// <returns></returns>
        public static T CastSerialObject<T>(object incoming) where T : IFlatbufferObject, new() {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("CastSerialObject");
                var fbObj = (IFlatbufferObject)incoming;
                var t = new T();
                t.__init(fbObj.BufferPosition, fbObj.ByteBuffer);
                return t;
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
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
            try {
                UnityEngine.Profiling.Profiler.BeginSample("GetOrCreateSerialize");
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
                    var serialized = ((IFBSerializable)serializableObj).Serialize(builder);
                    if (!ignoreCache) PutInSerializeCache(serializableObj, serialized);
                    ClearSerializingFlag(serializableObj);
                    return serialized;
                } else {
                    // try to convert
                    SetSerializingFlag(serializableObj);
                    var serialized = ConvertToFlatbuffer(builder, serializableObj);
                    if (serializableObj != null && serialized.HasValue) {
                        if (!ignoreCache) PutInSerializeCache(serializableObj, serialized.Value);
                    }
                    ClearSerializingFlag(serializableObj);
                    return serialized.HasValue ? serialized : 0;
                }



            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        private static bool HasDeserializingFlag(int bufferPos) {
            return deserializingATM.Contains(bufferPos);
        }


        private static void SetDeserializingFlag(int bufferPos) {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("SetDeserializingFlag");
                if (deserializingATM.Contains(bufferPos)) {
                    UnityEngine.Debug.LogError("Try to set a bufferpos to deserialize that is already in! bPos:" + bufferPos);
                }
                deserializingATM.Add(bufferPos);
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        private static void ClearDeserializingFlag(int bufferPos) {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("ClearDeserializingFlag");
                deserializingATM.Remove(bufferPos);
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public static int discardSetSerializingFlagValueType = 0;

        private static void SetSerializingFlag(object serializeObj) {
            if (serializeObj.GetType().IsValueType) {
                discardSetSerializingFlagValueType++;
                return;
            }

            try {
                UnityEngine.Profiling.Profiler.BeginSample("SetSerializingFlag");
                if (serializingATM.Contains(serializeObj)) {
                    UnityEngine.Debug.LogError("Try to set an object to serializeFlag that is already in! type:" + serializeObj.GetType());
                }
                serializingATM.Add(serializeObj);
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
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
            if (FlatBufferSerializer.DeserializationVersionMismatch
                && deserializedObject is IFBUpgradeable) {
                // the data we just deserialized has another version. Try to convert it to have valid data
                ((IFBUpgradeable)deserializedObject).Upgrade(CurrentDeserializingDataFormatVersion, CurrentDataFormatVersion, incomingData);
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
            try {
                UnityEngine.Profiling.Profiler.BeginSample("GetOrCreateDeserialize");

                if (incoming == null || incoming.BufferPosition == 0) {
                    return default(T);
                }
                var result = GetOrCreateDeserialize(incoming, typeof(T), newObject);
                return (T)result;
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        /// <summary>
        /// Deserializes a flatbufferobject to the type and (reuses newObject if provided)
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="type"></param>
        /// <param name="newObject"></param>
        /// <returns></returns>
        public static object GetOrCreateDeserialize(IFlatbufferObject incoming, Type type, IFBSerializable newObject = null) {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("GetOrCreateDeserialize");

                if (incoming == null || incoming.BufferPosition == 0) {
                    return null;
                }
                // first check if we already deserialize the object at this position
                object result = type.IsValueType ? null : FindInDeserializeCache<object>(incoming.BufferPosition);
                if (result != null) {
                    //UnityEngine.Debug.Log("Incoming-Type:"+incoming.GetType()+" Casting to :"+type.ToString());
                    // yeah, we found it. no need to create a new object we got it already
                    return result;
                }

                if (typeof(IFBSerializable).IsAssignableFrom(type)) {
                    UnityEngine.Profiling.Profiler.BeginSample("IFBSerializable");
                    // not deserialized, yet. Create a new object and call the deserialize method with the flatbuffers object
                    SetDeserializingFlag(incoming.BufferPosition);
                    try {
                        stb.Clear();
                        stb.Append("ifb-deserialize-").Append(type.ToString());
                        newObject = newObject == null ? (IFBSerializable)Activator.CreateInstance(type) : newObject;
                        PutIntoDeserializeCache(incoming.BufferPosition, newObject);
                        UnityEngine.Profiling.Profiler.BeginSample(stb.ToString());
                        newObject.Deserialize(incoming);
                        UnityEngine.Profiling.Profiler.EndSample();
                        // upgrade the object if there was a version mismatch
                        UpgradeObjectIfNeeded(newObject, incoming);

                        return newObject;
                    }
                    finally {
                        ClearDeserializingFlag(incoming.BufferPosition);
                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                } else {
                    try {
                        UnityEngine.Profiling.Profiler.BeginSample("Convert");
                        SetDeserializingFlag(incoming.BufferPosition);
                        UnityEngine.Profiling.Profiler.BeginSample("conv-deserialize");
                        var convResult = ConvertToObject(incoming, type);
                        UnityEngine.Profiling.Profiler.EndSample();
                        if (convResult == null) {
                            UnityEngine.Debug.LogError("There is no deserializer for " + type);
                            return null;
                        }
                        return convResult;
                    }
                    finally {
                        ClearDeserializingFlag(incoming.BufferPosition);
                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                }
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        /// <summary>
        /// Serialize an object accompanied with its c#-Type as (shared)string (every type is 'physically' only serialized once)
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int SerializeTypedObject(FlatBufferBuilder builder,object obj) {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("SerializeTypedObject");

                if (obj == null) return 0;


                var typeName = GetTypeName(obj);
                var offsetTypeName = builder.CreateSharedString(typeName);
                var offsetData = FlatBufferSerializer.GetOrCreateSerialize(builder, obj);
                builder.StartTable(2);
                builder.AddOffset(0, offsetTypeName.Value, 0);
                builder.AddOffset(1, offsetData.Value, 0);
                var result = builder.EndTable();

                return result;
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        /// <summary>
        /// Deserialize a typed object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="incoming"></param>
        /// <returns></returns>
        public static T DeserializeTypedObject<T>(object incoming) {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("DeserializeTypedObject");

                var manual = GetManualObject(incoming);
                var typeName = manual.GetString(0);
                if (typeName == null) {
                    return default(T);
                }
                var type = Type.GetType(typeName);
                var fbObj = manual.CreateSerialObject<Serial.FBRef>(1);
                var result = FlatBufferSerializer.GetOrCreateDeserialize(fbObj, type);
                return (T)result;
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public static void ClearCache() {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("ClearCache");

                obj2FSMapping.Clear();
                fb2objMapping.Clear();
                serializeBufferposCheck.Clear();
                deserializingATM.Clear();
                serializingATM.Clear();
                foreach (var objList in postProcessObjects) {
                    objList.Value.Clear();
                }
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public static byte[] SerializeToBytes(IFBSerializable root,int initialBufferSize=5000000)  {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("SerializeToBytes");

                var st = new System.Diagnostics.Stopwatch();
                st.Start();

                serializing = true;
                ClearCache();
                fbBuilder = new FlatBufferBuilder(initialBufferSize);

                var rootResult = root.Serialize(fbBuilder);
                fbBuilder.FlushCyclicResolver();
                fbBuilder.Finish(rootResult);
                // TODO: Check: Is this the whole buffer? Or is it even more?
                var buf = fbBuilder.DataBuffer.ToSizedArray();

                serializing = false;
                st.Stop();
                UnityEngine.Debug.Log("FINISHED SERIALIZATION inner:" + st.Elapsed.TotalMilliseconds / 1000.0f + "s");
                fbBuilder.Clear();
                return buf;
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public static void SerializeToFileDomain(FileSystem.FSDomain domain, String filename, IFBSerializable root)  {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("SerializeToFileDomain");

                var buf = SerializeToBytes(root);
                var fs = Kernel.Instance.Container.Resolve<Service.FileSystem.IFileSystemService>();
                fs.WriteBytesToFileAtDomain(domain, filename, buf);
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public static T DeserializeFromBytes<T>(byte[] buf,T dataRoot=default(T)) where T : IFBSerializable,new() {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("DeserializeFromBytes");

                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                ClearCache();
                var fbByteBuffer = new ByteBuffer(buf);
                if (dataRoot == null) {
                    dataRoot = new T();
                }
                dataRoot.Deserialize(fbByteBuffer);
                stopwatch.Stop();
                UnityEngine.Debug.Log("Deserialize final took:" + stopwatch.Elapsed.TotalMilliseconds / 1000.0f);
                return dataRoot;
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }


    }
}
