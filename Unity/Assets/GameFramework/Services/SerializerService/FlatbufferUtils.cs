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

        static readonly Type typeBool = typeof(bool);
        static readonly Type typeInt = typeof(int);
        static readonly Type typeFloat = typeof(float);
        static readonly Type typeLong = typeof(long);
        static readonly Type typeByte = typeof(byte);
        static readonly Type typeString = typeof(string);
        static readonly Type typeShort = typeof(short);
        static readonly Type IFBSERIALIZABLE_STRUCT = typeof(IFBSerializable2Struct);
        static readonly Type typeUID = typeof(ECS.UID);
        static readonly Type typeVector2 = typeof(Vector2);
        static readonly Type typeVector3 = typeof(Vector3);
        static readonly Type typeVector4 = typeof(Vector4);
        static readonly Type typeQuaternion = typeof(Quaternion); 

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
            vec2.x = __tbl.bb.GetFloat(vec_pos + 0);
            vec2.y = __tbl.bb.GetFloat(vec_pos + 4);
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
            vec.x = __tbl.bb.GetFloat(vec_pos + 0);
            vec.y = __tbl.bb.GetFloat(vec_pos + 4);
            vec.z = __tbl.bb.GetFloat(vec_pos + 8);
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
            vec.x = __tbl.bb.GetFloat(vec_pos + 0);
            vec.y = __tbl.bb.GetFloat(vec_pos + 4);
            vec.z = __tbl.bb.GetFloat(vec_pos + 8);
            vec.w = __tbl.bb.GetFloat(vec_pos + 12);
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
            quat.x = __tbl.bb.GetFloat(vec_pos + 0);
            quat.y = __tbl.bb.GetFloat(vec_pos + 4);
            quat.z = __tbl.bb.GetFloat(vec_pos + 8);
            quat.w = __tbl.bb.GetFloat(vec_pos + 12);
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
            uid.ID = __tbl.bb.GetInt(vec_pos + 0);
            uid.__SetRevision(__tbl.bb.GetInt(vec_pos + 4));
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

        public T GetReference<T>(int fbPos, DeserializationContext dctx, T obj = default(T)) where T : IFBSerializable2, new() {
            int offset = GetOffset(fbPos);
            T result = dctx._GetReference<T>(offset);
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
            int vtableOffset = GetVTableOffset(fbPos);

            return GetPrimitiveListFromOffset<T>(vtableOffset, ref tlist);
        }
        public List<T> GetPrimitiveListFromOffset<T>(int offset, ref List<T> tlist,bool directBufferAccess=false)  {
            if (offset == 0) {
                tlist = null;
                return null;
            }

            if (tlist == null) {
                tlist = new List<T>();
            } else {
                tlist.Clear();
            }

            Type listType = typeof(T);
            if (listType.IsEnum) {
                // enum. enums are serialized as int
                int[] tA = directBufferAccess ? __tbl.__vector_as_array<int>(offset,false) : __tbl.__vector_as_array_from_bufferpos<int>(offset);
                int length = tA.Length;
                for (int i = 0; i < length; i++) {
                    tlist.Add((T)(object)tA[i]); // i hate this casting madness. isn't there a cleaner way?
                }
                return tlist;
            } else {
                T[] tA = directBufferAccess ? __tbl.__vector_as_array<T>(offset, false) : __tbl.__vector_as_array_from_bufferpos<T>(offset);
                if (tA == null) {
                    tlist = null;
                    return null;
                }

                if (tlist == null) {
                    tlist = new List<T>(tA);
                    return tlist;
                }

                tlist.Clear();
                tlist.AddRange(tA);

                return tlist;
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
            return (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
        }

        /// <summary>
        /// If usingBufferPos=true => fbPos is the bufferPosition
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fbPos"></param>
        /// <param name="offset2obj"></param>
        /// <param name="list"></param>
        /// <param name="usingBufferPos"></param>
        /// <returns></returns>
        public List<T> TraverseList<T>(int fbPos,System.Func<int,T> offset2obj,ref List<T> list, bool usingBufferPos=false) {
            if (!usingBufferPos) {
                fbPos = GetVTableOffset(fbPos);
                if (fbPos == 0) {
                    return null;
                }
            }


            if (list == null) {
                list = new List<T>();
            } else {
                list.Clear();
            }

            int vector_start = usingBufferPos ? fbPos + sizeof(int) : __tbl.__vector(fbPos);
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

            return list;
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
            if (GetVTableOffset(fbPos) == 0) {
                tlist = null;
                return null;
            }

            if (tlist == null) {
                tlist = new List<T>();
            } else {
                tlist.Clear();
            }

           // int[] offsets = __tbl.__vector_as_array<int>(4 + fbPos * 2);
            int vector_start = __tbl.__vector(__tbl.__offset(4 + fbPos * 2)); 
            int vector_len = __tbl.__vector_len(__tbl.__offset(4 + fbPos * 2));
            int buflength = __tbl.bb.Length;
            for (int i = 0; i < vector_len; i++) {
                int offset = __tbl.__indirect(vector_start + i*4);
                if (offset == 0) {
                    tlist.Add(default(T));
                    continue;
                }
                
                IFBSerializable2 deserializedObject = dctx._GetReference<T>(buflength-offset);
                tlist.Add((T)deserializedObject); // i hate this casting madness. isn't there a cleaner way?
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
            int vecOffset = GetVTableOffset(fbPos);
            if (vecOffset == 0) {
                tlist = null;
                return null;
            }

            if (tlist == null) {
                tlist = new List<T>();
            } else {
                tlist.Clear();
            }

            Type innerType = typeof(T);

            int vector_start = __tbl.__vector(vecOffset);
            int vector_len = __tbl.__vector_len(vecOffset);
            int buflength = __tbl.bb.Length;

            if (innerType == typeUID) {
                int bytesize = 8;
                ECS.UID uid = new ECS.UID();
                for (int i = 0; i < vector_len; i++) {
                    GetUIDFromOffset(vector_start, ref uid);
                    tlist.Add((T)(object)uid);
                    vector_start += bytesize;
                }
                return tlist;
            } else if (IFBSERIALIZABLE_STRUCT.IsAssignableFrom(innerType)) {
                IFBSerializable2Struct elem = (IFBSerializable2Struct)new T();
                int bytesize = elem.ByteSize;
                for (int i = 0; i < vector_len; i++) {
                    elem.Get(this, vector_start);
                    tlist.Add((T)elem);
                    vector_start += bytesize;
                }
                return tlist;
            } else if (innerType == typeVector2) {
                int bytesize = 8;
                Vector2 vec2 = new Vector2();
                for (int i = 0; i < vector_len; i++) {
                    GetVector2FromOffset(vector_start, ref vec2);
                    tlist.Add((T)(object)vec2);
                    vector_start += bytesize;
                }
                return tlist;
            } else if (innerType == typeVector3) {
                int bytesize = 12;
                Vector3 vec3 = new Vector3();
                for (int i = 0; i < vector_len; i++) {
                    GetVector3FromOffset(vector_start, ref vec3);
                    tlist.Add((T)(object)vec3);
                    vector_start += bytesize;
                }
                return tlist;
            } else if (innerType == typeVector4) {
                int bytesize = 16;
                Vector4 vec4 = new Vector4();
                for (int i = 0; i < vector_len; i++) {
                    GetVector4FromOffset(vector_start, ref vec4);
                    tlist.Add((T)(object)vec4);
                    vector_start += bytesize;
                }
                return tlist;
            } else if (innerType == typeVector4) {
                int bytesize = 16;
                Quaternion quaternion = new Quaternion();
                for (int i = 0; i < vector_len; i++) {
                    GetQuaternionFromOffset(vector_start, ref quaternion);
                    tlist.Add((T)(object)quaternion);
                    vector_start += bytesize;
                }
                return tlist;
            }
            Debug.LogError($"GetStructList<{typeof(T)}>: Do not know how to serialize type:{innerType}");
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

        public T GetStruct<T>(int fbPos, ref T structElem) where T:IFBSerializable2Struct {
            int structOffset = GetStructBegin(fbPos);
            structElem.Get(this,structOffset);
            return structElem;
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
