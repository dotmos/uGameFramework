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

using Service.Serializer;
using System.Collections.Generic;

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

        public float GetFloat(int fbPos) { int o = __p.__offset(4 + fbPos * 2); return o != 0 ? __p.bb.GetFloat(o + __p.bb_pos) : (float)0.0f; }
        public int GetInt(int fbPos) { int o = __p.__offset(4 + fbPos * 2); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : 0; }
        public bool GetBool(int fbPos) { int o = __p.__offset(4 + fbPos * 2); return o != 0 ? 0 != __p.bb.Get(o + __p.bb_pos) : (bool)false; }
        public long GetLong(int fbPos) {  int o = __p.__offset(4 + fbPos * 2); return o != 0 ? __p.bb.GetLong(o + __p.bb_pos) : (long)0; }

        public string GetString(int fbPos) { int o = __p.__offset(4 + fbPos * 2); return o != 0 ? __p.__string(o + __p.bb_pos) : null; }

        public string GetStringListElementAt(int fbPos,int idx) { int o = __p.__offset(4+fbPos*2); return o != 0 ? __p.__string(__p.__vector(o) + idx * 4) : null; }
        public int GetListLength(int fbPos) { int o = __p.__offset(4+fbPos*2); return o != 0 ? __p.__vector_len(o) : 0;  }
        public int GetBufferPos(int fbPos){ int o = __p.__offset(4 + fbPos * 2); return o != 0 ? __p.__vector(o) : 0; }

        public List<string> GetStringList(int fbPos) {
            int bufPos = GetBufferPos(fbPos);
            if (bufPos == 0) {
                return null;
            }

            object cacheResult = FlatbufferSerializer.FindInDeserializeCache(bufPos);
            if (cacheResult != null) {
                return (List<string>)cacheResult;
            }
            int listLength = GetListLength(fbPos);
            var newList = new List<string>(listLength);
            for (int i = 0; i < listLength; i++) {
                newList.Add(GetStringListElementAt(fbPos, i));
            }
            FlatbufferSerializer.PutIntoDeserializeCache(bufPos, newList);
            return newList;
        }

        public List<T> GetPrimitiveList<T>(int fbPos) where T: struct {
            int bufPos = GetBufferPos(fbPos);

            if (bufPos == 0) {
                return null;
            }

            object cacheResult = FlatbufferSerializer.FindInDeserializeCache(bufPos);
            if (cacheResult != null) {
                return (List<T>)cacheResult;
            }

            T[] tA = __p.__vector_as_array<T>(4 + fbPos * 2);
            var newList = new List<T>(tA);
            FlatbufferSerializer.PutIntoDeserializeCache(bufPos, newList);
            return newList;
        }


        public T? GetListElemAt<T>(int fbPos,int j) where T : struct,IFlatbufferObject {
            int o = __p.__offset(4 + fbPos * 2);
            if (o == 0) return null;
            var result = new T();
            result.__init(__p.__indirect(__p.__vector(o) + j * 4), __p.bb);
            return result;
        }

        public List<TResult> GetNonPrimList<TSerialized, TResult>(int fbPos) where TSerialized : struct,IFlatbufferObject where TResult:IFBSerializable, new() {
            var bufPos = GetBufferPos(fbPos);
            var cachedResult = FlatbufferSerializer.FindInDeserializeCache(bufPos);
            if (cachedResult!=null) {
                if (cachedResult.GetType() != typeof(TResult)) {
                    UnityEngine.Debug.LogError("Got cached value but the types are different! Cached:" + cachedResult.GetType() + " Expected:" + typeof(TResult));
                }
                return (List<TResult>)cachedResult;
            }
            var listSize = GetListLength(fbPos);
            var tempList = new System.Collections.Generic.List<object>(listSize); // first create List<object> of all results and then pass this to the Create-method. Didn't find a better way,yet Generics with T? do not work for interfaces
            for (int i = 0; i < listSize; i++) tempList.Add(GetListElemAt<TSerialized>(fbPos,i));
            var result = FlatbufferSerializer.DeserializeList<TResult, TSerialized>(bufPos,  listSize, tempList);
            return result;
        }
    }
}
