/*
 * Copyright 2014 Google Inc. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Text;

namespace FlatBuffers
{
    /// <summary>
    /// All tables in the generated code derive from this struct, and add their own accessors.
    /// </summary>
    public struct Table
    {
        public int bb_pos { get; set; }
        public ByteBuffer bb { get; private set; }

        public ByteBuffer ByteBuffer { get { return bb; } }

        // Re-init the internal state with an external buffer {@code ByteBuffer} and an offset within.
        public Table(int _i, ByteBuffer _bb)
        {
            bb = _bb;
            bb_pos = _i;
        }

        public void Set(int address, ByteBuffer _bb) {
            bb = _bb;
            bb_pos = address;
        }

        // Look up a field in the vtable, return an offset into the object, or 0 if the field is not
        // present.
        public int __offset(int vtableOffset)
        {
            int vtable = bb_pos - bb.GetInt(bb_pos);
            return vtableOffset < bb.GetShort(vtable) ? (int)bb.GetShort(vtable + vtableOffset) : 0;
        }

        public bool IsOffsetPresent(int vtableOffset) {
            int vtable = bb_pos - bb.GetInt(bb_pos);
            return vtableOffset < bb.GetShort(vtable);
        }

        public static int __offset(int vtableOffset, int offset, ByteBuffer bb)
        {
            int vtable = bb.Length - offset;
            return (int)bb.GetShort(vtable + vtableOffset - bb.GetInt(vtable)) + vtable;
        }

        // Retrieve the relative offset stored at "offset"
        public int __indirect(int bufferPos)
        {
            return bufferPos + bb.GetInt(bufferPos);
        }

        public static int __indirect(int offset, ByteBuffer bb)
        {
            return offset + bb.GetInt(offset);
        }

        // Create a .NET String from UTF-8 data stored inside the flatbuffer.
        public string __string(int offset)
        {
            offset += bb.GetInt(offset);
            int len = bb.GetInt(offset);
            int startPos = offset + sizeof(int);
            return bb.GetStringUTF8(startPos, len);
        }

        // Get the length of a vector whose offset is stored at "offset" in this object.
        public int __vector_len(int offset)
        {
            offset += bb_pos;
            offset += bb.GetInt(offset);
            return bb.GetInt(offset);
        }

        // Get the start of data of a vector whose offset is stored at "offset" in this object.
        public int __vector(int offset)
        {
            offset += bb_pos;
            return offset + bb.GetInt(offset) + sizeof(int);  // data starts after the length
        }

#if ENABLE_SPAN_T
        // Get the data of a vector whoses offset is stored at "offset" in this object as an
        // Spant&lt;byte&gt;. If the vector is not present in the ByteBuffer,
        // then an empty span will be returned.
        public Span<byte> __vector_as_span(int offset)
        {
            var o = this.__offset(offset);
            if (0 == o)
            {
                return new Span<byte>();
            }

            var pos = this.__vector(o);
            var len = this.__vector_len(o);
            return bb.ToSpan(pos, len);
        }
#else
        // Get the data of a vector whoses offset is stored at "offset" in this object as an
        // ArraySegment&lt;byte&gt;. If the vector is not present in the ByteBuffer,
        // then a null value will be returned.
        public ArraySegment<byte>? __vector_as_arraysegment(int offset)
        {
            int o = this.__offset(offset);
            if (0 == o)
            {
                return null;
            }

            int pos = this.__vector(o);
            int len = this.__vector_len(o);
            return bb.ToArraySegment(pos, len);
        }
#endif

        // Get the data of a vector whoses offset is stored at "offset" in this object as an
        // T[]. If the vector is not present in the ByteBuffer, then a null value will be
        // returned.
        public T[] __vector_as_array<T>(int offset, bool lookupVtable = true,bool newMode=false)
        {
            if(!BitConverter.IsLittleEndian)
            {
                throw new NotSupportedException("Getting typed arrays on a Big Endian " +
                    "system is not support");
            }

            if (newMode) {
                if (lookupVtable) offset = this.__offset(4 + offset * 2);
            } else {
                offset = this.__offset(offset);
            }

            if (0 == offset)
            {
                return null;
            }

            int pos = this.__vector(offset);
            int len = this.__vector_len(offset);
            return bb.ToArray<T>(pos, len);
        }

        public T[] __vector_as_array_from_bufferpos<T>(int bufpos) {
            if (!BitConverter.IsLittleEndian) {
                throw new NotSupportedException("Getting typed arrays on a Big Endian " +
                    "system is not support");
            }


            if (0 == bufpos) {
                return null;
            }

            int pos = bufpos + sizeof(int);
            int len = bb.GetInt(bufpos);
            return bb.ToArray<T>(pos, len);
        }


        // Initialize any Table-derived type to point to the union at the given offset.
        public T __union<T>(int offset) where T : struct, IFlatbufferObject {
            int indirect = __indirect(offset);

            T t = new T();
            t.__init(indirect, bb);
            return t;
        }

        public IFlatbufferObject __union(int offset,IFlatbufferObject t)  {
            //offset += bb_pos;
            int indirect = __indirect(offset);
            t.__init(indirect, bb);
            return t;
        }

        public UnityEngine.Vector2 GetVec2(int o) {
            int vec_pos = bb_pos + __offset(o*2+4);
            return new UnityEngine.Vector2(bb.GetFloat(vec_pos + 0), bb.GetFloat(vec_pos + 4));
        }
        public UnityEngine.Vector3 GetVec3(int o) {
            int vec_pos = bb_pos + __offset(o * 2 + 4);
            return new UnityEngine.Vector3(bb.GetFloat(vec_pos + 0), bb.GetFloat(vec_pos + 4), bb.GetFloat(vec_pos + 8));
        }
        public UnityEngine.Vector4 GetVec4(int o) {
            int vec_pos = bb_pos + __offset(o * 2 + 4);
            return new UnityEngine.Vector4(bb.GetFloat(vec_pos + 0), bb.GetFloat(vec_pos + 4), bb.GetFloat(vec_pos + 8), bb.GetFloat(vec_pos + 12));
        }

        public static bool __has_identifier(ByteBuffer bb, string ident)
        {
            if (ident.Length != FlatBufferConstants.FileIdentifierLength)
                throw new ArgumentException("FlatBuffers: file identifier must be length " + FlatBufferConstants.FileIdentifierLength, "ident");

            for (int i = 0; i < FlatBufferConstants.FileIdentifierLength; i++)
            {
                if (ident[i] != (char)bb.Get(bb.Position + sizeof(int) + i)) return false;
            }

            return true;
        }

        // Compare strings in the ByteBuffer.
        public static int CompareStrings(int offset_1, int offset_2, ByteBuffer bb)
        {
            offset_1 += bb.GetInt(offset_1);
            offset_2 += bb.GetInt(offset_2);
            int len_1 = bb.GetInt(offset_1);
            int len_2 = bb.GetInt(offset_2);
            int startPos_1 = offset_1 + sizeof(int);
            int startPos_2 = offset_2 + sizeof(int);
            int len = Math.Min(len_1, len_2);
            for(int i = 0; i < len; i++) {
                byte b1 = bb.Get(i + startPos_1);
                byte b2 = bb.Get(i + startPos_2);
                if (b1 != b2)
                    return b1 - b2;
            }
            return len_1 - len_2;
        }

        // Compare string from the ByteBuffer with the string object
        public static int CompareStrings(int offset_1, byte[] key, ByteBuffer bb)
        {
            offset_1 += bb.GetInt(offset_1);
            int len_1 = bb.GetInt(offset_1);
            int len_2 = key.Length;
            int startPos_1 = offset_1 + sizeof(int);
            int len = Math.Min(len_1, len_2);
            for (int i = 0; i < len; i++) {
                byte b = bb.Get(i + startPos_1);
                if (b != key[i])
                    return b - key[i];
            }
            return len_1 - len_2;
        }
    }
}
