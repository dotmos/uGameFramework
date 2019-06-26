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

namespace FlatBuffers
{
    /// <summary>
    /// This is the base for both structs and tables.
    /// </summary>
    public interface IFlatbufferObject
    {
        void __init(int _i, ByteBuffer _bb);

        ByteBuffer ByteBuffer { get; }

        int BufferPosition { get; }
    }

    public struct FBManualObject : IFlatbufferObject {
        private Table __p;
        /// <summary>
        /// expose the current flatbuffer-object's position inside the buffer
        /// </summary>
        public int BufferPosition { get { return __p.bb_pos; } }
        public Table __table { get { return __p; } }
        public ByteBuffer ByteBuffer { get { return __p.bb; } }
        public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
        public FBManualObject __initFromRef(object refObj) {
            var _refObj = (Serial.FBRef)refObj;
            __init(_refObj.BufferPosition, _refObj.ByteBuffer);
            return this;
        }

        public float GetFloat(int pos) { int o = __p.__offset(4 + pos * 2); return o != 0 ? __p.bb.GetFloat(o + __p.bb_pos) : (float)0.0f; }
        public int GetInt(int pos) { int o = __p.__offset(4 + pos * 2); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : 0; }
        public bool GetBool(int pos) { int o = __p.__offset(4 + pos * 2); return o != 0 ? 0 != __p.bb.Get(o + __p.bb_pos) : (bool)false; }
    }
}
