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
using System;
using System.Collections.Generic;
using System.Linq;

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
        public FBManualObject __initFromRef(IFlatbufferObject refObj) {
            __init(refObj.BufferPosition, refObj.ByteBuffer);
            return this;
        }

        public float GetFloat(int fbPos) { int o = __p.__offset(4 + fbPos * 2); return o != 0 ? __p.bb.GetFloat(o + __p.bb_pos) : (float)0.0f; }
        public int GetInt(int fbPos) { int o = __p.__offset(4 + fbPos * 2); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : 0; }
        
        public bool GetBool(int fbPos) { int o = __p.__offset(4 + fbPos * 2); return o != 0 ? 0 != __p.bb.Get(o + __p.bb_pos) : (bool)false; }
        public long GetLong(int fbPos) {  int o = __p.__offset(4 + fbPos * 2); return o != 0 ? __p.bb.GetLong(o + __p.bb_pos) : (long)0; }

        public string GetString(int fbPos) { int o = __p.__offset(4 + fbPos * 2); return o != 0 ? __p.__string(o + __p.bb_pos) : null; }

        public Serial.FBRef? GetFBRef(int fbPos) {  int o = __p.__offset(4 + fbPos * 2); return o != 0 ? (Serial.FBRef?)(new Serial.FBRef()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; }
        public T GetOrCreate<T>(int fbPos) where T:new() {
            if (typeof(T) == typeof(UnityEngine.Vector2)) {
                return GetOrCreate<T, Serial.FBVector2>(fbPos);
            }
            else if (typeof(T) == typeof(UnityEngine.Vector3)) {
                return GetOrCreate<T, Serial.FBVector3>(fbPos);
            } else if (typeof(T) == typeof(UnityEngine.Vector4)) {
                return GetOrCreate<T, Serial.FBVector4>(fbPos);
            } else if (typeof(T) == typeof(UnityEngine.Quaternion)) {
                return GetOrCreate<T, Serial.FBQuaternion>(fbPos);
            } 
            else return FlatBufferSerializer.GetOrCreateDeserialize<T>(GetFBRef(fbPos));
        }

        public T CreateSerialObject<T>(int fbPos) where T: IFlatbufferObject, new() {
            var t = new T();
            t.__init(GetFBRefPos(fbPos), ByteBuffer);
            return t;
        }

        public T GetOrCreate<T,S>(int fbPos) where T : new() where S : IFlatbufferObject,new() {
            var s = CreateSerialObject<S>(fbPos);
            return FlatBufferSerializer.GetOrCreateDeserialize<T>(s);
        }
        public int GetFBRefPos(int fbPos) { int o = __p.__offset(4 + fbPos * 2); return o!=0?__p.__indirect(o + __p.bb_pos):0; }

        public string GetStringListElementAt(int fbPos,int idx) { int o = __p.__offset(4+fbPos*2); return o != 0 ? __p.__string(__p.__vector(o) + idx * 4) : null; }
        public int GetListLength(int fbPos) { int o = __p.__offset(4+fbPos*2); return o != 0 ? __p.__vector_len(o) : 0;  }
        public int GetBufferPos(int fbPos){ int o = __p.__offset(4 + fbPos * 2); return o != 0 ? __p.__vector(o) : 0; }

        public bool MutateLongValue(int fbPos, long longValue) { int o = __p.__offset(4 + fbPos * 2); if (o != 0) { __p.bb.PutLong(o + __p.bb_pos, longValue); return true; } else { return false; } }
        public bool MutateOffset(int fbPos, long longValue) { int o = __p.__offset(4 + fbPos * 2); if (o != 0) { __p.bb.PutLong(o + __p.bb_pos, longValue); return true; } else { return false; } }


        public List<string> GetStringList(int fbPos) {
            int bufPos = GetBufferPos(fbPos);
            if (bufPos == 0) {
                return null;
            }

            object cacheResult = FlatBufferSerializer.FindInDeserializeCache(bufPos);
            if (cacheResult != null) {
                return (List<string>)cacheResult;
            }
            int listLength = GetListLength(fbPos);
            var newList = new List<string>(listLength);
            for (int i = 0; i < listLength; i++) {
                newList.Add(GetStringListElementAt(fbPos, i));
            }
            FlatBufferSerializer.PutIntoDeserializeCache(bufPos, newList);
            return newList;
        }

        public IList<T> GetPrimitiveList<T>(int fbPos,bool isObservableList=false) where T: struct {
            int bufPos = GetBufferPos(fbPos);

            if (bufPos == 0) {
                return null;
            }

            object cacheResult = FlatBufferSerializer.FindInDeserializeCache(bufPos);
            if (cacheResult != null) {
                return (List<T>)cacheResult;
            }

            // get the array, but don't write the result in the lookup-table, because we want to map the result to the list
            T[] array = GetPrimitivesArray<T>(fbPos,true); 
            var newList = isObservableList? (IList<T>)new ObservableList<T>(array) : (IList<T>)new List<T>(array);
            FlatBufferSerializer.PutIntoDeserializeCache(bufPos, newList);
            return newList;
        }

        public T[] GetPrimitivesArray<T>(int fbPos, bool ignoreLookup=false) where T : struct {
            int bufPos = GetBufferPos(fbPos);

            if (bufPos == 0) {
                return null;
            }

            object cacheResult = ignoreLookup ? null : FlatBufferSerializer.FindInDeserializeCache(bufPos);
            if (cacheResult != null) {
                return (T[])cacheResult;
            }
            if (typeof(T).IsEnum) {
                int[] tA = __p.__vector_as_array<int>(4 + fbPos * 2);
                var result = tA.Cast<T>().ToArray();
                if (!ignoreLookup) FlatBufferSerializer.PutIntoDeserializeCache(bufPos, result);
                return result;

            } else {
                T[] tA = __p.__vector_as_array<T>(4 + fbPos * 2);
                if (!ignoreLookup) FlatBufferSerializer.PutIntoDeserializeCache(bufPos, tA);
                return tA;
            }
        }


        public T? GetListElemAt<T>(int fbPos,int j) where T : struct,IFlatbufferObject {
            int o = __p.__offset(4 + fbPos * 2);
            if (o == 0) return null;
            var result = new T();
            result.__init(__p.__indirect(__p.__vector(o) + j * 4), __p.bb);
            return result;
        }

        public IList<TResult> GetNonPrimList<TSerialized, TResult>(int fbPos) where TSerialized : struct,IFlatbufferObject where TResult:IFBSerializable, new() {
            var bufPos = GetBufferPos(fbPos);
            var cachedResult = FlatBufferSerializer.FindInDeserializeCache(bufPos);
            if (cachedResult!=null) {
                if (cachedResult.GetType() != typeof(TResult)) {
                    UnityEngine.Debug.LogError("Got cached value but the types are different! Cached:" + cachedResult.GetType() + " Expected:" + typeof(TResult));
                }
                return (List<TResult>)cachedResult;
            }
            var listSize = GetListLength(fbPos);
            var tempList = new System.Collections.Generic.List<object>(listSize); // first create List<object> of all results and then pass this to the Create-method. Didn't find a better way,yet Generics with T? do not work for interfaces
            for (int i = 0; i < listSize; i++) tempList.Add(GetListElemAt<TSerialized>(fbPos,i));
            var result = FlatBufferSerializer.DeserializeList<TResult, TSerialized>(bufPos,  listSize, tempList);
            return result;
        }

        /// <summary>
        /// Check if there is an offset at this vtable-position
        /// </summary>
        /// <param name="fbPos"></param>
        /// <returns></returns>
        public bool HasOffset(int fbPos) {
            return GetFBRefPos(fbPos) != 0;
        }

        /// <summary>
        /// Get a list that was created with FlatBufferSerializer.CreateTypedList(...)
        /// This list saves also the type of the object along the data and makes it possible to have Lists of super-types and its inhertied classes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fbPos"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<T> GetTypedList<T>(int fbPos,List<T> input=null)  {
            if (input == null) {
                input = new List<T>();
            }
            int listLength = GetListLength(fbPos);
            for (int i = 0; i < listLength; i = i + 2) {
                var typeName = GetStringListElementAt(fbPos, i);
                if (typeName == null) {
                    input.Add(default(T));
                    continue;
                }
                var type = Type.GetType(typeName);
                var fbObj = GetListElemAt<Serial.FBRef>(fbPos, i+1);
                var result = FlatBufferSerializer.GetOrCreateDeserialize(fbObj, type);
                input.Add((T)result);
            }
            return input;
        }

        public TResult GetObject<TResult, TSerialized>(int fbPos) where TResult : IFBSerializable, new() where TSerialized : IFlatbufferObject,new() {
            var result = FlatBufferSerializer.GetOrCreateDeserialize<TResult>(CreateSerialObject<TSerialized>(fbPos));
            return result;
        }



    }
}
