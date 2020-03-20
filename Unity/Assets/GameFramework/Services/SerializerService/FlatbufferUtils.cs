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
            __tbl = new Table(0, _bb);
            this.offset = offset;
            UpdateTable();
        }

        public ExtendedTable(int offset, FlatBufferBuilder builder) : this(offset, builder.DataBuffer) {
            
        }

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

        public float GetFloat(int fbPos) {
            int o = __tbl.__offset(4 + fbPos * 2);
            return o != 0 ? __tbl.bb.GetFloat(o + __tbl.bb_pos) : (float)0.0f;
        }
        public void MutateInt(int fbPos, int value) {
            int o = __tbl.__offset(4 + fbPos * 2);
            if (o == 0) return;
            __tbl.bb.PutInt(o + __tbl.bb_pos, value);
        }
        public int GetInt(int fbPos) {
            int o = __tbl.__offset(4 + fbPos * 2);
            return o != 0 ? __tbl.bb.GetInt(o + __tbl.bb_pos) : 0;
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


        public UnityEngine.Vector2 GetVec2(int o) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            return new UnityEngine.Vector2(__tbl.bb.GetFloat(vec_pos + 0), __tbl.bb.GetFloat(vec_pos + 4));
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


        public UnityEngine.Vector3 GetVec3(int o) {
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            return new UnityEngine.Vector3(__tbl.bb.GetFloat(vec_pos + 0), __tbl.bb.GetFloat(vec_pos + 4), __tbl.bb.GetFloat(vec_pos + 8));
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
            int vec_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            return new UnityEngine.Vector4(__tbl.bb.GetFloat(vec_pos + 0), __tbl.bb.GetFloat(vec_pos + 4), __tbl.bb.GetFloat(vec_pos + 8), __tbl.bb.GetFloat(vec_pos + 12));
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
            int quat_pos = __tbl.bb_pos + __tbl.__offset(o * 2 + 4);
            return new Quaternion(__tbl.bb.GetFloat(quat_pos + 0), __tbl.bb.GetFloat(quat_pos + 4), __tbl.bb.GetFloat(quat_pos + 8), __tbl.bb.GetFloat(quat_pos + 12));
        }

        public void MutateQuaternion(int o, ref Quaternion q) {
            // quaternions have same structure as vec4. so reuse this logic
            MutateVec4(o, q.x, q.y, q.z, q.w);
        }
        public void MutateQuaternion(int o, Quaternion q) {
            // quaternions have same structure as vec4. so reuse this logic
            MutateVec4(o, q.x, q.y, q.z, q.w);
        }
    }


}
