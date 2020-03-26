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
        public static readonly ExtendedTable NULL = new ExtendedTable(-1,(ByteBuffer)null);

        public Table __tbl;
        public int offset;

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
            return o != 0 ? __tbl.bb.Get(o + __tbl.bb_pos) :(byte) 0;
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

        public void MutateString(int fbPos,string newValue) {
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


        public UnityEngine.Vector2 GetVec2(int o) {
            Vector2 vec2 = new UnityEngine.Vector2();
            GetVec2(0, ref vec2);
            return vec2;
        }
        public void GetVec2(int o,ref Vector2 vec2) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            vec2.x = __tbl.bb.GetFloat(vec_pos + 0);
            vec2.y = __tbl.bb.GetFloat(vec_pos + 4);
        }

        public void MutateVec2(int o, float x, float y) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            __tbl.bb.PutFloat(vec_pos + 0, x);
            __tbl.bb.PutFloat(vec_pos + 4, y);
        }

        public void MutateVec2(int o, ref Vector2 vec2) {
            MutateVec2(o, vec2.x, vec2.y);
        }

        public void MutateVec2(int o, Vector2 vec2) {
            MutateVec2(o, vec2.x, vec2.y);
        }


        public UnityEngine.Vector4 GetVec3(int o) {
            Vector3 vec3 = new Vector3();
            GetVec3(o, ref vec3);
            return vec3;
        }

        public void GetVec3(int o, ref Vector3 vec) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            vec.x = __tbl.bb.GetFloat(vec_pos + 0);
            vec.y = __tbl.bb.GetFloat(vec_pos + 4);
            vec.z = __tbl.bb.GetFloat(vec_pos + 8);
        }
        public void MutateVec3(int o, float x, float y, float z) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            __tbl.bb.PutFloat(vec_pos + 0, x);
            __tbl.bb.PutFloat(vec_pos + 4, y);
            __tbl.bb.PutFloat(vec_pos + 8, z);
        }

        public void MutateVec3(int o, ref Vector3 vec3) {
            MutateVec3(o, vec3.x, vec3.y, vec3.z);
        }
        public void MutateVec3(int o, Vector3 vec3) {
            MutateVec3(o, vec3.x, vec3.y, vec3.z);
        }


        public UnityEngine.Vector4 GetVec4(int o) {
            Vector4 vec4=new Vector4();
            GetVec4(o, ref vec4);
            return vec4;
        }

        public void GetVec4(int o,ref Vector4 vec) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            vec.x = __tbl.bb.GetFloat(vec_pos + 0);
            vec.y = __tbl.bb.GetFloat(vec_pos + 4);
            vec.z = __tbl.bb.GetFloat(vec_pos + 8);
            vec.w = __tbl.bb.GetFloat(vec_pos + 12);
        }

        public void MutateVec4(int o, float x, float y, float z, float w) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            __tbl.bb.PutFloat(vec_pos + 0, x);
            __tbl.bb.PutFloat(vec_pos + 4, y);
            __tbl.bb.PutFloat(vec_pos + 8, z);
            __tbl.bb.PutFloat(vec_pos + 12, w);
        }

        public void MutateVec4(int o, ref Vector4 vec4) {
            MutateVec4(o, vec4.x, vec4.y, vec4.z, vec4.w);
        }
        public void MutateVec4(int o, Vector4 vec4) {
            MutateVec4(o, vec4.x, vec4.y, vec4.z, vec4.w);
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
            MutateVec4(o, q.x, q.y, q.z, q.w);
        }
        public void MutateQuaternion(int o, Quaternion q) {
            // quaternions have same structure as vec4. so reuse this logic
            MutateVec4(o, q.x, q.y, q.z, q.w);
        }

        public ECS.UID GetUID(int o) {
            ECS.UID uid = new ECS.UID();
            GetUID(o, ref uid);
            return uid;
        }

        public void GetUID(int o,ref ECS.UID uid) {
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
        public int GetListLength(int fbPos) { int o = __tbl.__offset(4 + fbPos * 2); return o != 0 ? __tbl.__vector_len(o) : 0; }
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
        public T GetOrCreateTypedObject<T>(int fbPos,DeserializationContext dctx) where T:DefaultSerializable2,new() {
            // pos of the struct ( with string(c# typename) and offset to object)
            int structOffset = __tbl.bb_pos + __tbl.__offset(4 + fbPos * 2);
            int objOff = __tbl.bb.Length - __tbl.__indirect(structOffset+4);

            // get the typename
            string typeName = __tbl.__string(structOffset);
            // get the object
            T obj = (T)Activator.CreateInstance(Type.GetType(typeName));
            T dObj = dctx.GetOrCreate<T>(objOff, obj);
            return dObj;
        }
    }
}
