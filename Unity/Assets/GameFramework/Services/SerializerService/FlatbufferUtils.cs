﻿using System.Collections.Generic;
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

        static readonly Type typeBool = typeof(int);
        static readonly Type typeInt = typeof(int);
        static readonly Type typeFloat = typeof(int);
        static readonly Type typeLong = typeof(int);
        static readonly Type typeByte = typeof(int);
        static readonly Type typeString = typeof(string);
        static readonly Type typeShort = typeof(short);
        static readonly Type IFBSERIALIZABLE_STRUCT = typeof(IFBSerializable2Struct);

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
            GetVector2(0, ref vec2);
            return vec2;
        }
        public void GetVector2(int o, ref Vector2 vec2) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
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

        public void GetUID(int o, ref ECS.UID uid) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            uid.ID = __tbl.bb.GetInt(vec_pos + 0);
            uid.__SetRevision(__tbl.bb.GetInt(vec_pos + 4));
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


        /// <summary>
        /// Fastest was to read a primitve list 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TLIST"></typeparam>
        /// <param name="fbPos"></param>
        /// <param name="tlist"></param>
        /// <returns></returns>
        public List<T> GetPrimitiveList<T>(int fbPos, ref List<T> tlist) where T : struct {
            Type listType = typeof(T);
            if (listType.IsEnum) {
                // enum. enums are serialized as int
                int[] tA = __tbl.__vector_as_array<int>(4 + fbPos * 2);
                int length = tA.Length;
                for (int i = 0; i < length; i++) {
                    tlist.Add((T)(object)tA[i]); // i hate this casting madness. isn't there a cleaner way?
                }
                return tlist;
            } else if (listType == typeString) {
                return null;
            } else {
                T[] tA = __tbl.__vector_as_array<T>(4 + fbPos * 2);
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

        public ObservableList<T> GetObjectList<T>(int fbPos, ref ObservableList<T> tlist) where T : IFBSerializable2, new() {

            if (GetVTableOffset(fbPos) == 0) {
                tlist = null;
                return null;
            }

            if (tlist == null) {
                tlist = new ObservableList<T>();
            } else {
                tlist.Clear();
            }
            GetObjectList<T>(fbPos, ref tlist.__innerList);
            return tlist;
        }

        public List<T> GetStructList<T>(int fbPos, ref List<T> tlist) where T : struct {
            if (GetVTableOffset(fbPos) == 0) {
                tlist = null;
                return null;
            }

            if (tlist == null) {
                tlist = new List<T>();
            } else {
                tlist.Clear();
            }

            Type innerType = typeof(T);

            if (IFBSERIALIZABLE_STRUCT.IsAssignableFrom(innerType)) {
                int vector_start = __tbl.__vector(__tbl.__offset(4 + fbPos * 2));
                int vector_len = __tbl.__vector_len(__tbl.__offset(4 + fbPos * 2));
                int buflength = __tbl.bb.Length;
                IFBSerializable2Struct elem = (IFBSerializable2Struct)new T();
                int bytesize = elem.ByteSize;
                for (int i = 0; i < vector_len; i++) {
                    elem.Get(this, vector_start + i * bytesize);
                    tlist.Add((T)elem);
                }
                return tlist;
            }
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