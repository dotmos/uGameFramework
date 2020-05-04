using System.Collections.Generic;
using System;
using FlatBuffers;
using System.Linq;
using System.Collections;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Service.Serializer
{

    public struct ExtendedTable
    {
        public static readonly ExtendedTable NULL = new ExtendedTable(-1, (ByteBuffer)null);

        public Table __tbl;
        public int offset;

        public static readonly Type typeBool = typeof(bool);
        public static readonly Type typeInt = typeof(int);
        public static readonly Type typeFloat = typeof(float);
        public static readonly Type typeLong = typeof(long);
        public static readonly Type typeByte = typeof(byte);
        public static readonly Type typeString = typeof(string);
        public static readonly Type typeShort = typeof(short);
        public static readonly Type typeIFBSerializableStruct = typeof(IFBSerializable2Struct);
        public static readonly Type typeIFBSerializable2 = typeof(IFBSerializable2);
        public static readonly Type typeUID = typeof(ECS.UID);
        public static readonly Type typeVector2 = typeof(Vector2);
        public static readonly Type typeVector3 = typeof(Vector3);
        public static readonly Type typeVector4 = typeof(Vector4);
        public static readonly Type typeQuaternion = typeof(Quaternion);
        public static readonly Type typeIList = typeof(IList);
        public static readonly Type typeIDictionary = typeof(IDictionary);
        public static readonly Type typeGenericList = typeof(IList<>);

        public static readonly Type typeExtendedTable = typeof(ExtendedTable);

        public ExtendedTable(int offset, ByteBuffer _bb) {
            __tbl = new Table(offset, _bb);
            this.offset = offset;
            UpdateTable();
        }

        public ExtendedTable(int offset, FlatBufferBuilder builder) : this(offset, builder.DataBuffer) {

        }

        public ByteBuffer bb => __tbl.bb;

        /// <summary>
        /// Update pointer to table. (Needed everytime the underlying buffer grows)
        /// </summary>
        public void UpdateTable() {
            if (IsNULL()) {
                return;
            }
            // since the buffer grows and the offset i s relative to the end of the buffer we need to update
            // the table-pos everytime the buffer grows (using this method)

            __tbl.bb_pos = __tbl.bb.Length - offset;
        }

        public bool IsNULL() {
            return offset == -1;
        }


        public int GetInt(int fbPos) {
            int o = __tbl.__offset(4 + fbPos * 2);
            return o != 0 ? __tbl.bb.GetInt(o + __tbl.bb_pos) : 0;
        }
        public int? GetNullableInt(int fbPos) {
            int o = __tbl.__offset(4 + fbPos * 2);
            if (o == 0) return null;
            return __tbl.bb.GetInt(o + __tbl.bb_pos);
        }

        public void MutateInt(int fbPos, int value) {
            int o = __tbl.__offset(4 + fbPos * 2);
            if (o == 0) return;
            __tbl.bb.PutInt(o + __tbl.bb_pos, value);
        }

        public byte GetByte(int fbPos) {
            int o = __tbl.__offset(4 + fbPos * 2);
            return o != 0 ? __tbl.bb.Get(o + __tbl.bb_pos) : (byte)0;
        }
        public void MutateByte(int fbPos, byte value) {
            int o = __tbl.__offset(4 + fbPos * 2);
            if (o == 0) return;
            __tbl.bb.PutByte(o + __tbl.bb_pos, value);
        }

        public short GetShort(int fbPos) {
            int o = __tbl.__offset(4 + fbPos * 2);
            return o != 0 ? __tbl.bb.GetShort(o + __tbl.bb_pos) : (short)0;
        }
        public void MutateShort(int fbPos, short value) {
            int o = __tbl.__offset(4 + fbPos * 2);
            if (o == 0) return;
            __tbl.bb.PutShort(o + __tbl.bb_pos, value);
        }

        public float GetFloat(int fbPos) {
            int o = __tbl.__offset(4 + fbPos * 2);
            return o != 0 ? __tbl.bb.GetFloat(o + __tbl.bb_pos) : (float)0.0f;
        }
        public void MutateFloat(int fbPos, float value) {
            int o = __tbl.__offset(4 + fbPos * 2);
            if (o == 0) return;
            __tbl.bb.PutFloat(o + __tbl.bb_pos, value);
        }
        public bool GetBool(int fbPos) {
            int o = __tbl.__offset(4 + fbPos * 2);
            return o != 0 ? 0 != __tbl.bb.Get(o + __tbl.bb_pos) : (bool)false;
        }

        public void MutateBool(int fbPos, bool value) {
            int o = __tbl.__offset(4 + fbPos * 2);
            if (o == 0) return;

            __tbl.bb.Put(o + __tbl.bb_pos, value ? (byte)1 : (byte)0);
        }
        public long GetLong(int fbPos) {
            int o = __tbl.__offset(4 + fbPos * 2);
            return o != 0 ? __tbl.bb.GetLong(o + __tbl.bb_pos) : (long)0;
        }

        public void MutateLong(int fbPos, long value) {
            int o = __tbl.__offset(4 + fbPos * 2);
            if (o == 0) return;
            __tbl.bb.PutLong(o + __tbl.bb_pos, value);
        }

        public string GetString(int fbPos) {
            int o = __tbl.__offset(4 + fbPos * 2);
            return o != 0 ? __tbl.__string(o + __tbl.bb_pos) : null;
        }

        public void MutateString(int fbPos, string newValue) {
            int o = __tbl.__offset(4 + fbPos * 2);
            if (o == 0) return;

            int stringOffset = o + __tbl.bb_pos;
            stringOffset += __tbl.bb.GetInt(stringOffset);
            int maxChars = __tbl.bb.GetInt(stringOffset);

            if (newValue.Length > maxChars) {
                Debug.LogError("Growing String not supported! Using only substring");
                newValue = newValue.Substring(0, maxChars);
            }

            // the string can be created inline reusing the current buffer-segment
            __tbl.bb.PutInt(stringOffset, newValue.Length);
            __tbl.bb.PutStringUTF8(stringOffset + 4, newValue);
            __tbl.bb.PutByte(stringOffset + 4 + (newValue.Length), 0);
        }


        public UnityEngine.Vector2 GetVector2(int o) {
            Vector2 vec2 = new UnityEngine.Vector2();
            GetVector2(o, ref vec2);
            return vec2;
        }
        public void GetVector2(int fbPos, ref Vector2 vec2) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(fbPos * 2 + 4);
            GetVector2FromOffset(vec_pos, ref vec2);
        }
        public void GetVector2FromOffset(int vec_pos, ref Vector2 vec2) {
            vec2.x = __tbl.bb.GetFloat(vec_pos + 0);
            vec2.y = __tbl.bb.GetFloat(vec_pos + 4);
        }


        public void MutateVector2(int o, float x, float y) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            __tbl.bb.PutFloat(vec_pos + 0, x);
            __tbl.bb.PutFloat(vec_pos + 4, y);
        }

        public void MutateVector2(int o, ref Vector2 vec2) {
            MutateVector2(o, vec2.x, vec2.y);
        }

        public void MutateVector2(int o, Vector2 vec2) {
            MutateVector2(o, vec2.x, vec2.y);
        }


        public UnityEngine.Vector4 GetVector3(int o) {
            Vector3 vec3 = new Vector3();
            GetVector3(o, ref vec3);
            return vec3;
        }

        public void GetVector3(int o, ref Vector3 vec) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            GetVector3FromOffset(vec_pos, ref vec);
        }
        public void GetVector3FromOffset(int vec_pos, ref Vector3 vec) {
            vec.x = __tbl.bb.GetFloat(vec_pos + 0);
            vec.y = __tbl.bb.GetFloat(vec_pos + 4);
            vec.z = __tbl.bb.GetFloat(vec_pos + 8);
        }

        public void MutateVector3(int o, float x, float y, float z) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            __tbl.bb.PutFloat(vec_pos + 0, x);
            __tbl.bb.PutFloat(vec_pos + 4, y);
            __tbl.bb.PutFloat(vec_pos + 8, z);
        }

        public void MutateVector3(int o, ref Vector3 vec3) {
            MutateVector3(o, vec3.x, vec3.y, vec3.z);
        }
        public void MutateVector3(int o, Vector3 vec3) {
            MutateVector3(o, vec3.x, vec3.y, vec3.z);
        }


        public UnityEngine.Vector4 GetVector4(int o) {
            Vector4 vec4 = new Vector4();
            GetVector4(o, ref vec4);
            return vec4;
        }

        public void GetVector4(int o, ref Vector4 vec) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            GetVector4FromOffset(vec_pos, ref vec);
        }
        public void GetVector4FromOffset(int vec_pos, ref Vector4 vec) {
            vec.x = __tbl.bb.GetFloat(vec_pos + 0);
            vec.y = __tbl.bb.GetFloat(vec_pos + 4);
            vec.z = __tbl.bb.GetFloat(vec_pos + 8);
            vec.w = __tbl.bb.GetFloat(vec_pos + 12);
        }

        public void MutateVector4(int o, float x, float y, float z, float w) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            __tbl.bb.PutFloat(vec_pos + 0, x);
            __tbl.bb.PutFloat(vec_pos + 4, y);
            __tbl.bb.PutFloat(vec_pos + 8, z);
            __tbl.bb.PutFloat(vec_pos + 12, w);
        }

        public void MutateVector4(int o, ref Vector4 vec4) {
            MutateVector4(o, vec4.x, vec4.y, vec4.z, vec4.w);
        }
        public void MutateVector4(int o, Vector4 vec4) {
            MutateVector4(o, vec4.x, vec4.y, vec4.z, vec4.w);
        }

        public Quaternion GetQuaternion(int o) {
            Quaternion quat = new Quaternion();
            GetQuaternion(o, ref quat);
            return quat;
        }

        public void GetQuaternion(int o, ref Quaternion quat) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            GetQuaternionFromOffset(vec_pos, ref quat);
        }

        public void GetQuaternionFromOffset(int quat_pos, ref Quaternion quat) {
            quat.x = __tbl.bb.GetFloat(quat_pos + 0);
            quat.y = __tbl.bb.GetFloat(quat_pos + 4);
            quat.z = __tbl.bb.GetFloat(quat_pos + 8);
            quat.w = __tbl.bb.GetFloat(quat_pos + 12);
        }

        public void MutateQuaternion(int o, ref Quaternion q) {
            // quaternions have same structure as vec4. so reuse this logic
            MutateVector4(o, q.x, q.y, q.z, q.w);
        }
        public void MutateQuaternion(int o, Quaternion q) {
            // quaternions have same structure as vec4. so reuse this logic
            MutateVector4(o, q.x, q.y, q.z, q.w);
        }

        public ECS.UID GetUID(int o) {
            ECS.UID uid = new ECS.UID();
            GetUID(o, ref uid);
            return uid;
        }

        public void GetUID(int fbPos, ref ECS.UID uid) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(fbPos * 2 + 4);
            GetUIDFromOffset(vec_pos, ref uid);
        }

        public void GetUIDFromOffset(int uid_pos, ref ECS.UID uid) {
            uid.ID = __tbl.bb.GetInt(uid_pos + 0);
            uid.__SetRevision(__tbl.bb.GetInt(uid_pos + 4));
        }


        public void MutateUID(int o, ref ECS.UID uid) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            __tbl.bb.PutInt(vec_pos + 0, uid.ID);
            __tbl.bb.PutInt(vec_pos + 4, uid.Revision);
        }

        /// <summary>
        /// Returns the offset's (current) buffer-position
        /// Caution: if the buffer grows those buffer-positions get
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int Off2Buf(int offset) {
            return  __tbl.bb.Length - offset;
        }

        public int Buf2Off(int bufferPosition) {
            return __tbl.bb.Length - bufferPosition; 
        }

        public int GetVTableOffset(int fbPos) { return __tbl.__offset(4 + fbPos * 2); }

        public int GetStructBegin(int fbPos) { return __tbl.bb_pos + __tbl.__offset(fbPos * 2 + 4); }
        public int GetListLength(int fbPos) { int o = __tbl.__offset(4 + fbPos * 2); return o != 0 ? __tbl.__vector_len(o) : 0; }
        public int GetListLengthFromOffset(int o) { return o != 0 ? __tbl.__vector_len(o) : 0; }
        public int GetIntElementAt(int fbPos, int j) { int o = __tbl.__offset(4 + fbPos * 2); return o != 0 ? __tbl.bb.GetInt(__tbl.__vector(o) + j * 4) : (int)0; }

        public int GetOffset(int fbPos) {
            int o = __tbl.__offset(4 + fbPos * 2);

            return o != 0 ? __tbl.bb.Length - __tbl.__indirect(o + __tbl.bb_pos) : 0;
        }

        /// <summary>
        /// Caution. This value will be invalid as soon as the buffer grows. always write down offset-values (bufferlength - direct-memory-address )
        /// </summary>
        /// <param name="fbPos"></param>
        /// <returns></returns>
        public int GetDirectMemoryAddressOfOffset(int fbPos) {
            int o = __tbl.__offset(4 + fbPos * 2);

            return o != 0 ? __tbl.__indirect(o + __tbl.bb_pos) : 0;
        }

        /// <summary>
        /// Creates a typed object. For this to work you need to create a sharedstring of the type name (TODO: add the string inline)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fbPos"></param>
        /// <param name="dctx"></param>
        /// <returns></returns>
        public T GetOrCreateTypedObject<T>(int fbPos, DeserializationContext dctx) where T : IFBSerializable2 {
            // pos of the struct ( with string(c# typename) and offset to object)
            int structOffset = __tbl.bb_pos + __tbl.__offset(4 + fbPos * 2);
            int objOff = __tbl.bb.Length - __tbl.__indirect(structOffset + 4);

            // get the typename
            string typeName = __tbl.__string(structOffset);
            // get the object
            T dObj = dctx.GetOrCreate<T>(objOff, Type.GetType(typeName));
            return dObj;
        }

        public T GetReference<T>(int fbPos, DeserializationContext dctx, T obj = default(T)) where T : new() {
            int bufferPos = GetOffset(fbPos);
            T result = dctx.GetReference<T>(bufferPos);
            return result;
        }


        public List<string> GetStringList(int fbPos, ref List<string> tlist) {
            int vtableOffset = GetVTableOffset(fbPos);
            if (vtableOffset == 0) {
                tlist = null;
                return null;
            }

            if (tlist == null) {
                tlist = new List<string>();
            } else {
                tlist.Clear();
            }

            int vector_start = __tbl.__vector(vtableOffset);
            int vector_len = __tbl.__vector_len(vtableOffset);
            for (int i = 0; i < vector_len; i++) {
                int offset = __tbl.__indirect(vector_start);
                if (offset == 0) {
                    tlist.Add(null);
                    continue;
                }

                int strlen = bb.GetInt(offset);
                int strstartPos = offset + 4;
                tlist.Add(bb.GetStringUTF8(strstartPos, strlen));
                vector_start += 4;
            }

            return tlist;
        }

        public ObservableList<string> GetStringList(int fbPos, ref ObservableList<string> tlist)  {
            if (GetVTableOffset(fbPos) == 0) {
                tlist = null;
                return null;
            }

            if (tlist == null) {
                tlist = new ObservableList<string>();
            } else {
                tlist.Clear();
            }
            GetStringList(fbPos, ref tlist.__innerList);
            return tlist;
        }

        /// <summary>
        /// Fastest was to read a primitve list 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TLIST"></typeparam>
        /// <param name="fbPos"></param>
        /// <param name="tlist"></param>
        /// <returns></returns>
        public List<T> GetPrimitiveList<T>(int fbPos, ref List<T> tlist)  {
            int offset = GetOffset(fbPos);

            return GetPrimitiveListFromOffset<T>(offset, ref tlist);
        }
        public List<T> GetPrimitiveListFromOffset<T>(int offset, ref List<T> tlist, bool directBufferAccess = false) {
            if (offset == 0) {
                tlist = null;
                return null;
            }

            if (tlist == null) {
                tlist = new List<T>();
            } else {
                tlist.Clear();
            }

            tlist = (List<T>)GetPrimitiveListFromOffset(offset, tlist, typeof(T), directBufferAccess);
            return tlist;
        }

        private List<T> _CreatePrimList<T>(int offset, List<T> resultList = null,bool directBufferAccess=false) {
            if (!directBufferAccess) {
                offset = Off2Buf(offset);
            }
            T[] tA = __tbl.__vector_as_array_from_bufferpos<T>(offset);
            if (tA == null) {
                return null;
            }
            if (resultList == null) {
                resultList = new List<T>(tA); // fast!
                return resultList;
            } 
                
            resultList.Clear();
            resultList.AddRange(tA);
            return resultList;
        }

        public IList GetPrimitiveListFromOffset(int offset, IList tlist,Type innerType,bool directBufferAccess=false)  {
            if (innerType.IsEnum) {
                if (!directBufferAccess) {
                    offset = Off2Buf(offset);
                }

                // enum. enums are serialized as int
                int[] tA = __tbl.__vector_as_array_from_bufferpos<int>(offset);
                int length = tA.Length;
                if (tlist == null) {
                    tlist = new List<int>();
                }
                for (int i = 0; i < length; i++) {
                    tlist.Add(tA[i]); 
                }
                return tlist;
            } else {
                if (innerType==typeInt) {
                    return _CreatePrimList<int>(offset, (List<int>)tlist, directBufferAccess);
                }
                else if (innerType == typeFloat) {
                    return _CreatePrimList<float>(offset, (List<float>)tlist, directBufferAccess);
                } else if (innerType == typeByte) {
                    return _CreatePrimList<byte>(offset, (List<byte>)tlist, directBufferAccess);
                } else if (innerType == typeShort) {
                    return _CreatePrimList<short>(offset, (List<short>)tlist, directBufferAccess);
                } else if (innerType == typeLong) {
                    return _CreatePrimList<long>(offset, (List<long>)tlist, directBufferAccess);
                } else if (innerType == typeBool) {
                    return _CreatePrimList<bool>(offset, (List<bool>)tlist, directBufferAccess);
                }
                throw new ArgumentException("unsupported type:" + innerType);

            }
        }

        public ObservableList<T> GetPrimitiveList<T>(int fbPos, ref ObservableList<T> tlist) where T : struct {
            if (GetVTableOffset(fbPos) == 0) {
                tlist = null;
                return null;
            }

            if (tlist == null) {
                tlist = new ObservableList<T>();
            } else {
                tlist.Clear();
            }
            GetPrimitiveList<T>(fbPos, ref tlist.__innerList);
            return tlist;
        }

        public static bool IsGenericList(Type oType) {
            return (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeGenericList));
        }

        private static bool UseGetPrimitiveList(Type type) {
            return type.IsPrimitive || type.IsEnum;
        }


        public ObservableList<T> GetList<T>(int fbPos, ref ObservableList<T> tlist, DeserializationContext dctx) {
            if (GetVTableOffset(fbPos) == 0) {
                tlist = null;
                return null;
            }

            if (tlist == null) {
                tlist = new ObservableList<T>();
            } else {
                tlist.Clear();
            }
            GetList<T>(fbPos, ref tlist.__innerList,dctx);
            return tlist;
        }

        public List<T> GetList<T>(int fbPos, ref List<T> tlist, DeserializationContext dctx){
            int offset = GetOffset(fbPos);
            tlist = (List<T>)GetListFromOffset(offset, typeof(List<T>), dctx, tlist,false);
            return tlist;
        }

        public IList GetListFromOffset(int offset,Type listType,DeserializationContext dctx, IList list=null,bool useDirectBuffer=false) {
            
            if (!listType.IsGenericType) {
                throw new ArgumentException($"GetListFromOffset: Invalid parameters offset:{offset} type:{listType}");
            }
            if (offset == 0) {
                return null;
            }


            Type innerType = listType.GetGenericArguments()[0];
            
            /*if (innerType.IsGenericType) {
                if (typeIList.IsAssignableFrom(innerType)) {
                    object newObject = list ?? Activator.CreateInstance(listType);
                    IList resultList = newObject is IObservableList ? ((IObservableList)newObject).InnerIList : (IList)newObject;

                    resultList = TraverseIListFromOffset(offset, (_offset) => {
                        Debug.Log($"outer-offset:{offset}  inneroffset:{_offset}");
                        return thiz.GetListFromOffset(_offset, innerType, dctx, null, true);
                    }, resultList, useDirectBuffer);
                    return resultList;
                } 
                else if (typeIDictionary.IsAssignableFrom(innerType)) {

                }
            } */
            
            if (innerType.IsPrimitive || innerType.IsEnum) {
                object newObject = list ?? Activator.CreateInstance(listType);
                IList resultList = newObject is IObservableList ? ((IObservableList)newObject).InnerIList : (IList)newObject;

                var result = GetPrimitiveListFromOffset(offset, resultList, innerType, useDirectBuffer);
                return result;
            } 
            else if (innerType.IsValueType) {
                object newObject = list ?? Activator.CreateInstance(listType);
                IList resultList = newObject is IObservableList ? ((IObservableList)newObject).InnerIList : (IList)newObject;
                var result = GetStructListFromOffset(offset, resultList, innerType, useDirectBuffer);
                return result;
            } else {
                var thiz = this;
                object newObject = list ?? Activator.CreateInstance(listType);
                IList resultList = newObject is IObservableList ? ((IObservableList)newObject).InnerIList : (IList)newObject;
                resultList = TraverseIListFromOffset(offset, (_offset) => {
                    Debug.Log($"outer-offset:{offset}  inneroffset:{_offset} innerType:{innerType}");
                    return dctx.GetOrCreate(thiz.Buf2Off(_offset),innerType,null);
                }, resultList, useDirectBuffer);
                return resultList;
                //object newObject = list ?? Activator.CreateInstance(listType);
                //IList resultList = newObject is IObservableList ? ((IObservableList)newObject).InnerIList : (IList)newObject;

                ////var result = GetObjectListFromOffset(offset, resultList, innerType, dctx, useDirectBuffer);
                //var result = dctx.GetOrCreate(offset, innerType, newObject);
                //return result;
            }
            // TODO: struct-list
            throw new ArgumentException($"GetListFromOffset: Could not create list for type {listType} innerType:{innerType}");
        }

        public IList TraverseIListFromOffset(int offset, System.Func<int, object> offset2obj, IList list, bool directMemoryAccess = false) {
            if (list == null) return null;

            if (!directMemoryAccess) {
                if (offset == 0) {
                    return null;
                }

                offset = Off2Buf(offset);
            } else {
                if (offset == 0) {
                    return null;
                }
            }

            Debug.Log($"Travers on offset:{offset}");

            list.Clear();

            int elempos = offset;
            int vector_len = __tbl.bb.GetInt(offset);
            int buflength = __tbl.bb.Length;
            for (int i = 0; i < vector_len; i++) {
                elempos += 4;
                if (__tbl.bb.GetInt(elempos) == 0) {
                    list.Add(null);
                    continue;
                }
                int idxOffset = __tbl.__indirect(elempos);
                var result = offset2obj(idxOffset); // call the callback to retrieve the object at this offset
                list.Add(result);
            }

            return list;
        }


        public ObservableList<T> TraverseList<T>(int fbPos, System.Func<int, object> offset2obj, ref ObservableList<T> obsList, bool usingBufferPos = false) {
            if (GetVTableOffset(fbPos) == 0) {
                obsList = null;
                return null;
            }

            if (obsList == null) {
                obsList = new ObservableList<T>();
            } else {
                obsList.Clear();
            }
            TraverseList<T>(fbPos, offset2obj, ref obsList.__innerList);
            return obsList;
        }

        /// <summary>
        /// If usingBufferPos=true => fbPos is the bufferPosition
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fbPos"></param>
        /// <param name="offset2obj">CAUTION: func that gets a DIRECT-buffer-pos</param>
        /// <param name="list"></param>
        /// <param name="usingBufferPos"></param>
        /// <returns></returns>
        public List<T> TraverseList<T>(int fbPos,System.Func<int,object> offset2obj,ref List<T> list) {
            Debug.Log($"Travers on offset:{fbPos}");

            if (list == null) {
                list = new List<T>();
            } else {
                list.Clear();
            }

            int offset = GetOffset(fbPos);

            var result = TraverseIListFromOffset(offset, offset2obj, list);
            return (List<T>)result;
            /*int vector_start = usingBufferPos ? fbPos + sizeof(int) : __tbl.__vector(fbPos);
            int vector_len = usingBufferPos ? __tbl.bb.GetInt(fbPos) : __tbl.__vector_len(fbPos);
            int buflength = __tbl.bb.Length;
            for (int i = 0; i < vector_len; i++) {
                int offset = __tbl.__indirect(vector_start + i * 4);
                
                if (offset == 0) {
                    list.Add(default(T));
                    continue;
                }
                var result = offset2obj(offset);
                list.Add(result);
            }

            return list;*/
        }

        /*
        public List<T> GetListOfLists<T,S>(int fbPos, ref List<S> list) where T : new() where S:List<T>,new() {
            IList<S> root = null;

            var vOffset = GetVTableOffset(fbPos);
            if (vOffset == 0) {
                return null;
            }

            if (list == null) {
                root = new List<S>();
            } else {
                root = (IList<S>)list;
                root.Clear();
            }

            Type innerListType = root.GetType().GetGenericArguments()[0];
            
            int vector_start = __tbl.__vector(vOffset);
            int vector_len = __tbl.__vector_len(vOffset);
            int buflength = __tbl.bb.Length;
            for (int i = 0; i < vector_len; i++) {
                int offset = __tbl.__indirect(vector_start + i * 4);
                
                if (offset == 0) {
                    root.Add(null);
                    continue;
                }
                List<T> innerList = new S();
                Type innerListgenericType = typeof(T);

                if (innerListgenericType.IsPrimitive || innerListgenericType.IsEnum || innerListgenericType.IsValueType) {
                    var result = GetPrimitiveListFromOffset<T>(offset, ref innerList);
                    root.Add((S)result);
                } 
                else if ( IsGenericList(innerListgenericType)) {
                    List<T> genList = new S();
                    var result = GetListOfLists<T>(0, ref genList);
                }

                

                IFBSerializable2 deserializedObject = dctx._GetReference<T>(buflength - offset);
                tlist.Add((T)deserializedObject); // i hate this casting madness. isn't there a cleaner way?
            }
            return tlist;


            // the innerList
            
            
            return null;
        }*/

        public List<T> GetObjectList<T>(int fbPos, ref List<T> tlist, DeserializationContext dctx = null) where T : IFBSerializable2, new() {
            int vtableOffset = GetVTableOffset(fbPos);
            return GetObjectListFromOffset<T>(vtableOffset, ref tlist, dctx,false);
        }

        public List<T> GetObjectListFromOffset<T>(int offset, ref List<T> tlist, DeserializationContext dctx = null, bool directMemoryAccess = false) where T : IFBSerializable2, new() {
            if (offset == 0) {
                tlist = null;
                return null;
            }

            if (tlist == null) {
                tlist = new List<T>();
            } else {
                tlist.Clear();
            }

            tlist = (List<T>)GetObjectListFromOffset(offset, tlist, typeof(T), dctx, directMemoryAccess);
            return tlist;
        }

        public IList GetObjectListFromOffset(int offset, IList tlist, Type innerType,DeserializationContext dctx = null,bool directMemoryAccess=false) {
           // int[] offsets = __tbl.__vector_as_array<int>(4 + fbPos * 2);
            int elempos = directMemoryAccess ? offset/* + sizeof(int)*/ : __tbl.__vector(offset)-4; 
            int vector_len = directMemoryAccess ? __tbl.bb.GetInt(offset) : __tbl.__vector_len(offset);
            int buflength = __tbl.bb.Length;
            for (int i = 0; i < vector_len; i++) {
                elempos += 4;
                if (__tbl.bb.GetInt(elempos)==0) {
                    tlist.Add(null);
                    continue;
                }

                int idxOffset = __tbl.__indirect(elempos);

                IFBSerializable2 deserializedObject = (IFBSerializable2)dctx.GetReferenceByType(buflength-idxOffset,innerType);
                tlist.Add(deserializedObject); // i hate this casting madness. isn't there a cleaner way?
            }
            return tlist;
        }

        public ObservableList<T> GetObjectList<T>(int fbPos, ref ObservableList<T> tlist, DeserializationContext dctx = null) where T : IFBSerializable2, new() {

            if (GetVTableOffset(fbPos) == 0) {
                tlist = null;
                return null;
            }

            if (tlist == null) {
                tlist = new ObservableList<T>();
            } else {
                tlist.Clear();
            }
            GetObjectList<T>(fbPos, ref tlist.__innerList,dctx);
            return tlist;
        }


        public List<T> GetStructList<T>(int fbPos, ref List<T> tlist) where T : struct {
            int vecOffset = GetOffset(fbPos);
            if (vecOffset == 0) {
                tlist = null;
                return null;
            }

            if (tlist == null) {
                tlist = new List<T>();
            } else {
                tlist.Clear();
            }

            var result = GetStructListFromOffset(vecOffset, tlist,typeof(T), false);
            return (List<T>)result;
        }

        public IList GetStructListFromOffset(int offset, IList tlist,Type innerType, bool directMemoryAccess = false) {
            int currentAddress = directMemoryAccess ? offset : Off2Buf(offset);

            int vector_start = currentAddress + sizeof(int);
            int vector_len = bb.GetInt(currentAddress);
            int buflength = bb.Length;

            if (innerType == typeUID) {
                int bytesize = 8;
                ECS.UID uid = new ECS.UID();
                for (int i = 0; i < vector_len; i++) {
                    GetUIDFromOffset(vector_start, ref uid);
                    tlist.Add(uid);
                    vector_start += bytesize;
                }
                return tlist;
            } else if (typeIFBSerializableStruct.IsAssignableFrom(innerType)) {
                IFBSerializable2Struct elem = (IFBSerializable2Struct)Activator.CreateInstance(innerType);
                int bytesize = elem.ByteSize;
                for (int i = 0; i < vector_len; i++) {
                    elem.Get(this, vector_start);
                    tlist.Add(elem);
                    vector_start += bytesize;
                }
                return tlist;
            } else if (innerType == typeVector2) {
                int bytesize = 8;
                Vector2 vec2 = new Vector2();
                for (int i = 0; i < vector_len; i++) {
                    GetVector2FromOffset(vector_start, ref vec2);
                    tlist.Add(vec2);
                    vector_start += bytesize;
                }
                return tlist;
            } else if (innerType == typeVector3) {
                int bytesize = 12;
                Vector3 vec3 = new Vector3();
                for (int i = 0; i < vector_len; i++) {
                    GetVector3FromOffset(vector_start, ref vec3);
                    tlist.Add(vec3);
                    vector_start += bytesize;
                }
                return tlist;
            } else if (innerType == typeVector4) {
                int bytesize = 16;
                Vector4 vec4 = new Vector4();
                for (int i = 0; i < vector_len; i++) {
                    GetVector4FromOffset(vector_start, ref vec4);
                    tlist.Add(vec4);
                    vector_start += bytesize;
                }
                return tlist;
            } else if (innerType == typeQuaternion) {
                int bytesize = 16;
                Quaternion quaternion = new Quaternion();
                for (int i = 0; i < vector_len; i++) {
                    GetQuaternionFromOffset(vector_start, ref quaternion);
                    tlist.Add(quaternion);
                    vector_start += bytesize;
                }
                return tlist;
            }
            Debug.LogError($"GetStructList<{innerType}>: Do not know how to serialize type:{innerType}");
            return null;
        }


        public ObservableList<T> GetStructList<T>(int fbPos, ref ObservableList<T> tlist) where T : struct {

            if (GetVTableOffset(fbPos) == 0) {
                tlist = null;
                return null;
            }

            if (tlist == null) {
                tlist = new ObservableList<T>();
            } else {
                tlist.Clear();
            }
            GetStructList<T>(fbPos, ref tlist.__innerList);
            return tlist;
        }

        public void GetStruct<T>(int fbPos, ref T structElem) {
            int structOffset = GetStructBegin(fbPos);
            structElem = (T)GetStructFromOffset(structOffset, typeof(T),true);
        }

        public object GetStructFromOffset(int offset, Type type, bool useDirectMemory=false) {
                offset = useDirectMemory ? offset : Off2Buf(offset);

            if (type == typeUID) {
                var uid = (ECS.UID)Activator.CreateInstance(type);
                GetUIDFromOffset(offset, ref uid);
                return uid;
            } 
            else if (type == typeVector3) {
                Vector3 vec3 = new Vector3();
                GetVector3FromOffset(offset, ref vec3);
                return vec3;
            } 
            else if (typeIFBSerializableStruct.IsAssignableFrom(type)) {
                IFBSerializable2Struct structElem = (IFBSerializable2Struct)Activator.CreateInstance(type);
                structElem.Get(this, offset);
                return structElem;
            } 
            else if (type == typeVector2) {
                var vec2 = new Vector2();
                GetVector2FromOffset(offset, ref vec2);
                return vec2;
            } 
            else if (type == typeVector4) {
                Vector4 vec4 = new Vector4();
                GetVector4FromOffset(offset, ref vec4);
                return vec4;
            } 
            else if (type == typeQuaternion) {
                Quaternion quaternion = new Quaternion();
                GetQuaternionFromOffset(offset, ref quaternion);
                return quaternion;
            }
            Debug.LogError($"GetStructList<{type}>: Do not know how to serialize type:{type}");
            return null;
        }

        public Dictionary<TKey,TValue> GetDictionary<TKey,TValue>(int fbPos, ref Dictionary<TKey,TValue> dict, DeserializationContext dctx, bool directMemoryAccess = false) {
            int offset = GetDirectMemoryAddressOfOffset(fbPos);
            if (offset == 0) {
                dict = null;
                return null;
            }

            if (dict == null) {
                dict = new Dictionary<TKey,TValue>();
            } else {
                dict.Clear();
            }

            dict = (Dictionary<TKey,TValue>)GetDictionaryFromOffset(offset, dict, dctx, true);

            return dict;
        }

        private object GetPrimitiveOrStruct(int offset,Type type) {
            if (type.IsPrimitive) {
                return bb.Get(offset, type);
            }
            else {
                if (type == typeVector3) {
                    Vector3 vec = new Vector3();
                    GetVector3FromOffset(offset, ref vec);
                    return vec;
                }
            }
            throw new ArgumentException($"unsupported type in getPrimitveOrStruct:{type}");
        }

        /// <summary>
        /// Caution dict needs to be internally of IDictionary<,>
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="dict"></param>
        /// <param name="dctx"></param>
        /// <param name="directMemoryAccess"></param>
        /// <returns></returns>
        public IDictionary GetDictionaryFromOffset(int offset, IDictionary dict,DeserializationContext dctx,bool directMemoryAccess = false) {
            int currentAddress = directMemoryAccess ? offset : Off2Buf(offset);

            int count = bb.GetInt(currentAddress);
            currentAddress += 4;
            var dictType = dict.GetType();
            var genericTypes = dictType.GetGenericArguments();
            var typeKey = genericTypes[0];
            var typeValue = genericTypes[1];

            bool keyPrimitiveOrStruct = typeKey.IsValueType;
            bool keyIsStruct = !keyPrimitiveOrStruct && typeKey.IsValueType;
            bool valuePrimitiveOrStruct = typeValue.IsValueType;

            bool valueIsStruct = !valuePrimitiveOrStruct && typeValue.IsValueType;

            int keySize = keyPrimitiveOrStruct ? ByteBuffer.SizeOf(typeKey) : ByteBuffer.SizeOf(typeInt);
            int valueSize = valuePrimitiveOrStruct ? ByteBuffer.SizeOf(typeValue) : ByteBuffer.SizeOf(typeInt);
            int elementSize = keySize + valueSize;
            int overallSize = elementSize * count + ByteBuffer.SizeOf(typeInt);

            int currentValueAddress = currentAddress + keySize;
            if (keyPrimitiveOrStruct && valuePrimitiveOrStruct) {
                for (int i = 0; i < count; i++) {
                    var keyData = GetPrimitiveOrStruct(currentAddress, typeKey);
                    var valData = GetPrimitiveOrStruct(currentValueAddress, typeValue);

                    dict[keyData] = valData;
                    currentAddress += elementSize;
                    currentValueAddress += elementSize;
                }
            }
            else if (keyPrimitiveOrStruct && !valuePrimitiveOrStruct) {
                for (int i = 0; i < count; i++) {
                    var keyData = GetPrimitiveOrStruct(currentAddress, typeKey);
                    var valData = dctx.GetOrCreate(Buf2Off(__tbl.__indirect(currentValueAddress)), typeValue);
                    dict[keyData] = valData;
                    currentAddress += elementSize;
                    currentValueAddress += elementSize;
                }
            } 
            else if (!keyPrimitiveOrStruct && valuePrimitiveOrStruct) {
                for (int i = 0; i < count; i++) {
                    var keyData = dctx.GetOrCreate(Buf2Off(__tbl.__indirect(currentAddress)), typeKey);
                    var valData = GetPrimitiveOrStruct(currentValueAddress, typeValue);

                    dict[keyData] = valData;
                    currentAddress += elementSize;
                    currentValueAddress += elementSize;
                }
            } 
            else if (!keyPrimitiveOrStruct && !valuePrimitiveOrStruct) {
                for (int i = 0; i < count; i++) {
                    var keyData = dctx.GetOrCreate(Buf2Off(__tbl.__indirect(currentAddress)), typeKey);
                    var valueData = dctx.GetOrCreate(Buf2Off(__tbl.__indirect(currentValueAddress)), typeValue);
                    dict[keyData] = valueData;
                    currentAddress += elementSize;
                    currentValueAddress += elementSize;
                }
            }

            return dict;
        }


        public T GetOrCreate<T>(int fbPos, ref T obj,DeserializationContext dctx) where T : new() {
            int offset = GetOffset(fbPos);
            if (offset == 0) return default(T);
            var result = dctx.GetOrCreate<T>(offset, ref obj);
            return result;
        }

            //    public List<T> GetList<T>(int fbPos, ref List<T> intList) {
            //    int o = GetVTableOffset(fbPos);
            //    return GetListFromOffset(o, ref intList);
            //}
            //public List<T> GetListFromOffset<T>(int offset,ref List<T> intList)  {
            //    if (offset == 0) {
            //        intList = null;
            //        return null;
            //    } 

            //    if (intList == null) {
            //        intList = new List<T>();
            //    } else {
            //        intList.Clear();
            //    }
            //    int count = GetListLengthFromOffset(offset);
            //    int vecBase = __tbl.__vector(offset);
            //    for (int i = 0; i < count; i++) {
            //        int value = __tbl.bb.GetInt(vecBase + i * 4);
            //        intList.Add(value);
            //    }

            //    return intList;
            //}
        }
}
