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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UniRx;
using UnityEngine;

/// @file
/// @addtogroup flatbuffers_csharp_api
/// @{

namespace FlatBuffers
{
    /// <summary>
    /// Responsible for building up and accessing a FlatBuffer formatted byte
    /// array (via ByteBuffer).
    /// </summary>
    public partial class FlatBufferBuilder {
        public int _space;
        private ByteBuffer _bb;
        private int _minAlign = 1;

        // The vtable for the current table (if _vtableSize >= 0)
        private int[] _vtable = new int[16];
        // The size of the vtable. -1 indicates no vtable
        private int _vtableSize = -1;
        // Starting offset of the current struct/table.
        private int _objectStart;
        // List of offsets of all vtables.
        private int[] _vtables = new int[16];
        // Number of entries in `vtables` in use.
        private int _numVtables = 0;
        // For the current vector being built.
        private int _vectorNumElems = 0;
        // For the amount of elements to be created.
        private int _vectorCapacity = 0;

        static readonly Type typeBool = typeof(bool);
        static readonly Type typeInt = typeof(int);
        static readonly Type typeFloat = typeof(float);
        static readonly Type typeLong = typeof(long);
        static readonly Type typeByte = typeof(byte);
        static readonly Type typeString = typeof(string);
        static readonly Type typeShort = typeof(short);
        static readonly Type typeUID = typeof(ECS.UID);
        static readonly Type typeVector2 = typeof(Vector2);
        static readonly Type typeVector3 = typeof(Vector3);
        static readonly Type typeVector4 = typeof(Vector4);
        static readonly Type typeQuaternion = typeof(Quaternion);

        static readonly Type typeIFBserializabel2Struct = typeof(IFBSerializable2Struct);
        static readonly Type typeIList = typeof(IList);

        // For CreateSharedString
        private Dictionary<string, StringOffset> _sharedStringMap = null;

        private int cyclicID = -1000;
        private Dictionary<int, object> cyclicObjMapping = new Dictionary<int, object>();
        private List<Action> cyclicResolver = new List<Action>();

        /// <summary>
        /// Create a FlatBufferBuilder with a given initial size.
        /// </summary>
        /// <param name="initialSize">
        /// The initial size to use for the internal buffer.
        /// </param>
        public FlatBufferBuilder(int initialSize) {
            if (initialSize <= 0)
                throw new ArgumentOutOfRangeException("initialSize",
                    initialSize, "Must be greater than zero");
            _space = initialSize;
            _bb = new ByteBuffer(initialSize);
        }

        /// <summary>
        /// Create a FlatBufferBuilder backed by the pased in ByteBuffer
        /// </summary>
        /// <param name="buffer">The ByteBuffer to write to</param>
        public FlatBufferBuilder(ByteBuffer buffer) {
            _bb = buffer;
            _space = buffer.Length;
            buffer.Reset();
        }

        /// <summary>
        /// DEPRECATED. DO NOT USE
        /// mark this reference to be cyclic and postpone writting the actual value till after serialization
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int AddToCyclicResolver(System.Object obj) {
            int id = --cyclicID;
            cyclicObjMapping[id] = obj;
            return id;
        }

        /// <summary>
        /// DEPRECATED. DO NOT USE
        ///Write out the right addresses to the cyclic references
        /// </summary>
        public void FlushCyclicResolver() {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("FlushCyclicResolver");

                int amount = cyclicResolver.Count;
                for (int i = 0; i < amount; i++) {
                    cyclicResolver[i]();
                }
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        /// <summary>
        /// Reset the FlatBufferBuilder by purging all data that it holds.
        /// </summary>
        public void Clear() {
            _space = _bb.Length;
            _bb.Reset();
            _minAlign = 1;
            while (_vtableSize > 0) _vtable[--_vtableSize] = 0;
            _vtableSize = -1;
            _objectStart = 0;
            _numVtables = 0;
            _vectorNumElems = 0;
            cyclicResolver.Clear();
            cyclicObjMapping.Clear();
            cyclicID = -1000;
        }



        /// <summary>
        /// Gets and sets a Boolean to disable the optimization when serializing
        /// default values to a Table.
        ///
        /// In order to save space, fields that are set to their default value
        /// don't get serialized into the buffer.
        /// </summary>
        public bool ForceDefaults { get; set; }

        /// @cond FLATBUFFERS_INTERNAL

        public int Offset { get { return _bb.Length - _space; } }

        public int ___Space { get { return _space; } set { _space = value; } }

        public void Pad(int size) {
            _bb.PutByte(_space -= size, 0, size);
        }

        // Doubles the size of the ByteBuffer, and copies the old data towards
        // the end of the new buffer (since we build the buffer backwards).
        void GrowBuffer() {
            _bb.GrowFront(_bb.Length << 1);
        }

        // Prepare to write an element of `size` after `additional_bytes`
        // have been written, e.g. if you write a string, you need to align
        // such the int length field is aligned to SIZEOF_INT, and the string
        // data follows it directly.
        // If all you need to do is align, `additional_bytes` will be 0.
        public void Prep(int size, int additionalBytes,bool ignorePad=false) {
            // Track the biggest thing we've ever aligned to.
            if (size > _minAlign)
                _minAlign = size;
            // Find the amount of alignment needed such that `size` is properly
            // aligned after `additional_bytes`
            int alignSize =
                ((~((int)_bb.Length - _space + additionalBytes)) + 1) &
                (size - 1);
            // Reallocate the buffer if needed.
            while (_space < alignSize + size + additionalBytes) {
                int oldBufSize = (int)_bb.Length;
                GrowBuffer();
                _space += (int)_bb.Length - oldBufSize;

            }
            if (alignSize > 0)
                Pad(alignSize);
        }

        public void PutBool(bool x) {
            _bb.PutByte(_space -= sizeof(byte), (byte)(x ? 1 : 0));
        }

        public void PutSbyte(sbyte x) {
            _bb.PutSbyte(_space -= sizeof(sbyte), x);
        }

        public void PutByte(byte x) {
            _bb.PutByte(_space -= sizeof(byte), x);
        }

        public void PutShort(short x) {
            _bb.PutShort(_space -= sizeof(short), x);
        }

        public void PutUshort(ushort x) {
            _bb.PutUshort(_space -= sizeof(ushort), x);
        }

        public int PutInt(int x) {
            return _bb.PutInt(_space -= sizeof(int), x);
        }

        public int PutOffset(int address,int off) {
            Prep(sizeof(int), 0);  // Ensure alignment is already done.
            if (off > Offset)
                throw new ArgumentException();
            off = off != 0 ? Offset - off + sizeof(int) : 0;
            int addressWrittenTo = PutInt(off);
            return addressWrittenTo;
            //return new int[4] { offBefore, addressWrittenTo,beforeBBLen,beforeBBspace };
        }

        public void PutUint(uint x) {
            _bb.PutUint(_space -= sizeof(uint), x);
        }

        public void PutLong(long x) {
            _bb.PutLong(_space -= sizeof(long), x);
        }

        public void PutUlong(ulong x) {
            _bb.PutUlong(_space -= sizeof(ulong), x);
        }

        public void PutFloat(float x) {
            _bb.PutFloat(_space -= sizeof(float), x);
        }

        // just moves the current offset position
        public void MoveCurrentOffset(int amount) {
            _space -= amount;
        }

        /// <summary>
        /// Puts an array of type T into this builder at the
        /// current offset
        /// </summary>
        /// <typeparam name="T">The type of the input data </typeparam>
        /// <param name="x">The array to copy data from</param>
        public void Put<T>(T[] x, int length = -1)
            where T : struct {
            _space = _bb.Put(_space, x, length);
        }

#if ENABLE_SPAN_T
        /// <summary>
        /// Puts a span of type T into this builder at the
        /// current offset
        /// </summary>
        /// <typeparam name="T">The type of the input data </typeparam>
        /// <param name="x">The span to copy data from</param>
        public void Put<T>(Span<T> x)
            where T : struct
        {
            _space = _bb.Put(_space, x);
        }
#endif

        public void PutDouble(double x) {
            _bb.PutDouble(_space -= sizeof(double), x);
        }
        /// @endcond

        /// <summary>
        /// Add a `bool` to the buffer (aligns the data and grows if necessary).
        /// </summary>
        /// <param name="x">The `bool` to add to the buffer.</param>
        public void AddBool(bool x) { Prep(sizeof(byte), 0); PutBool(x); }

        /// <summary>
        /// Add a `sbyte` to the buffer (aligns the data and grows if necessary).
        /// </summary>
        /// <param name="x">The `sbyte` to add to the buffer.</param>
        public void AddSbyte(sbyte x) { Prep(sizeof(sbyte), 0); PutSbyte(x); }

        /// <summary>
        /// Add a `byte` to the buffer (aligns the data and grows if necessary).
        /// </summary>
        /// <param name="x">The `byte` to add to the buffer.</param>
        public void AddByte(byte x) { Prep(sizeof(byte), 0); PutByte(x); }

        /// <summary>
        /// Add a `short` to the buffer (aligns the data and grows if necessary).
        /// </summary>
        /// <param name="x">The `short` to add to the buffer.</param>
        public void AddShort(short x) { Prep(sizeof(short), 0); PutShort(x); }

        /// <summary>
        /// Add an `ushort` to the buffer (aligns the data and grows if necessary).
        /// </summary>
        /// <param name="x">The `ushort` to add to the buffer.</param>
        public void AddUshort(ushort x) { Prep(sizeof(ushort), 0); PutUshort(x); }

        /// <summary>
        /// Add an `int` to the buffer (aligns the data and grows if necessary).
        /// </summary>
        /// <param name="x">The `int` to add to the buffer.</param>
        public void AddInt(int x) { Prep(sizeof(int), 0); PutInt(x); }

        /// <summary>
        /// Add an `uint` to the buffer (aligns the data and grows if necessary).
        /// </summary>
        /// <param name="x">The `uint` to add to the buffer.</param>
        public void AddUint(uint x) { Prep(sizeof(uint), 0); PutUint(x); }

        /// <summary>
        /// Add a `long` to the buffer (aligns the data and grows if necessary).
        /// </summary>
        /// <param name="x">The `long` to add to the buffer.</param>
        public void AddLong(long x) { Prep(sizeof(long), 0); PutLong(x); }

        /// <summary>
        /// Add an `ulong` to the buffer (aligns the data and grows if necessary).
        /// </summary>
        /// <param name="x">The `ulong` to add to the buffer.</param>
        public void AddUlong(ulong x) { Prep(sizeof(ulong), 0); PutUlong(x); }

        /// <summary>
        /// Add a `float` to the buffer (aligns the data and grows if necessary).
        /// </summary>
        /// <param name="x">The `float` to add to the buffer.</param>
        public void AddFloat(float x) { Prep(sizeof(float), 0); PutFloat(x); }

        /// <summary>
        /// Add an array of type T to the buffer (aligns the data and grows if necessary).
        /// </summary>
        /// <typeparam name="T">The type of the input data</typeparam>
        /// <param name="x">The array to copy data from</param>
        public void Add<T>(T[] x, int length = -1)
            where T : struct {
            if (x == null) {
                throw new ArgumentNullException("Cannot add a null array");
            }

            length = length == -1 ? x.Length : length;

            if (length == 0) {
                // don't do anything if the array is empty
                return;
            }

            if (!ByteBuffer.IsSupportedType<T>()) {
                throw new ArgumentException("Cannot add this Type array to the builder (" + typeof(T) + ")");
            }

            int size = ByteBuffer.SizeOf<T>();
            // Need to prep on size (for data alignment) and then we pass the
            // rest of the length (minus 1) as additional bytes
            Prep(size, size * (length - 1));
            Put(x, length);
        }

#if ENABLE_SPAN_T
        /// <summary>
        /// Add a span of type T to the buffer (aligns the data and grows if necessary).
        /// </summary>
        /// <typeparam name="T">The type of the input data</typeparam>
        /// <param name="x">The span to copy data from</param>
        public void Add<T>(Span<T> x)
            where T : struct
        {
            if (!ByteBuffer.IsSupportedType<T>())
            {
                throw new ArgumentException("Cannot add this Type array to the builder");
            }

            int size = ByteBuffer.SizeOf<T>();
            // Need to prep on size (for data alignment) and then we pass the
            // rest of the length (minus 1) as additional bytes
            Prep(size, size * (x.Length - 1));
            Put(x);
        }
#endif

        /// <summary>
        /// Add a `double` to the buffer (aligns the data and grows if necessary).
        /// </summary>
        /// <param name="x">The `double` to add to the buffer.</param>
        public void AddDouble(double x) { Prep(sizeof(double), 0);
            PutDouble(x); }

        /// <summary>
        /// Adds an offset, relative to where it will be written.
        /// </summary>
        /// <param name="off">The offset to add to the buffer.</param>
        public int _AddOffset(int off) {
            Prep(sizeof(int), 0);  // Ensure alignment is already done.
            if (off > Offset)
                throw new ArgumentException();
            off = off != 0 ? Offset - off + sizeof(int) : 0;
            int addressWrittenTo = PutInt(off);
            return addressWrittenTo;
            //return new int[4] { offBefore, addressWrittenTo,beforeBBLen,beforeBBspace };
        }




        public void AddOffset(int offset) {
            if (offset >= 0) {
                // default behaviour
                _AddOffset(offset);
            } else {
                if (cyclicObjMapping.TryGetValue(offset, out object objToSerialize)) {
                    // cyclic reference. for now write some dummy offset(1895), get the position where this was written and write at this possition 
                    // once we are done
                    int offBefore = Offset;
                    int beforeBBLen = _bb.Length;
                    int beforeAddress = _AddOffset(1895);
                    // remove first stage mapping (-cyclicID) and replace with [addressToWriteTo]=obj
                    cyclicObjMapping.Remove(offset);
                    cyclicResolver.Add(() => {
                        // now(!) get the cached offset
                        int? objOffset = Service.Serializer.FlatBufferSerializer.FindInSerializeCache(objToSerialize);
                        if (objOffset.HasValue) {
                            // calc diff to adjust the address
                            int diff = _bb.Length - beforeBBLen;
                            int addressToWriteOffsetTo = beforeAddress + diff;
                            // calc relative offset
                            int off = offBefore - objOffset.Value + sizeof(int);
                            int i = _bb.GetInt(addressToWriteOffsetTo);
                            _bb.PutInt(addressToWriteOffsetTo, off);
                        }
                    });
                } else {
                    UnityEngine.Debug.LogError("Unknown cyclic reference:" + offset);
                }
            }

        }

        /// <summary>
        /// Same as StartVector, just that it reserves capacity amount of data (e.g. for later mutation)
        /// </summary>
        /// <param name="elemSize"></param>
        /// <param name="count"></param>
        /// <param name="alignment"></param>
        /// <param name="capacity"></param>
        public void StartCapacityVector(int elemSize, int count, int alignment, int capacity) {
            NotNested();
            _vectorNumElems = count;
            _vectorCapacity = capacity;
            Prep(sizeof(int), elemSize * count);
            Prep(alignment, elemSize * count); // Just in case alignment > int.
        }

        /// <summary>
        /// Writes data necessary to finish a capcity vector construction.
        /// </summary>
        public VectorOffset EndCapactiyVector() {
            PutInt(_vectorCapacity);
            PutInt(_vectorNumElems);
            return new VectorOffset(Offset);
        }

        /// @cond FLATBUFFERS_INTERNAL
        public void StartVector(int elemSize, int count, int alignment, bool deactivateNestedCheck=false) {
            if (!deactivateNestedCheck) NotNested();
            _vectorNumElems = count;
            Prep(sizeof(int), elemSize * count);
            Prep(alignment, elemSize * count); // Just in case alignment > int.
        }
        /// @endcond

        /// <summary>
        /// Writes data necessary to finish a vector construction.
        /// </summary>
        public VectorOffset EndVector() {
            PutInt(_vectorNumElems);
            return new VectorOffset(Offset);
        }

        /// <summary>
        /// Creates a vector of tables.
        /// </summary>
        /// <param name="offsets">Offsets of the tables.</param>
        public VectorOffset CreateVectorOfTables<T>(Offset<T>[] offsets) where T : struct {
            NotNested();
            StartVector(sizeof(int), offsets.Length, sizeof(int));
            for (int i = offsets.Length - 1; i >= 0; i--) AddOffset(offsets[i].Value);
            return EndVector();
        }

        public VectorOffset CreateIntVector(IList<int> list, bool convertToByte = false) {
            NotNested();

            if (convertToByte) {
                StartVector(sizeof(byte), list.Count, sizeof(byte));
                for (int i = list.Count - 1; i >= 0; i--) AddByte((byte)list[i]);
                return EndVector();
            } else {
                StartVector(sizeof(int), list.Count, sizeof(int));
                for (int i = list.Count - 1; i >= 0; i--) AddInt(list[i]);
                return EndVector();
            }
        }

        public int CreateOffsetVector(IList<int> list) {
            NotNested();
            StartVector(sizeof(int), list.Count, sizeof(int));
            for (int i = list.Count - 1; i >= 0; i--) AddOffset(list[i]);
            return EndVector().Value;
        }

        /// <summary>
        /// Creates a vector of tables.
        /// </summary>
        /// <param name="offsets">Offsets of the tables.</param>
        public VectorOffset CreateVectorOfTables<T>(IList<Offset<T>> offsets) where T : struct {
            NotNested();
            StartVector(sizeof(int), offsets.Count, sizeof(int));
            for (int i = offsets.Count - 1; i >= 0; i--) AddOffset(offsets[i].Value);
            return EndVector();
        }

        /// <summary>
        /// Creates a vector of offset-positions.
        /// </summary>
        /// <param name="offsets">Offsets of the tables.</param>
        public VectorOffset CreateVectorOfTables(IList<int> offsets) {
            NotNested();
            StartVector(sizeof(int), offsets.Count, sizeof(int));
            for (int i = offsets.Count - 1; i >= 0; i--) AddOffset(offsets[i]);
            return EndVector();
        }


        /// @cond FLATBUFFERS_INTENRAL
        public void Nested(int obj) {
            // Structs are always stored inline, so need to be created right
            // where they are used. You'll get this assert if you created it
            // elsewhere.
            if (obj != Offset)
                throw new Exception(
                    "FlatBuffers: struct must be serialized inline.");
        }

        public void NotNested() {
            // You should not be creating any other objects or strings/vectors
            // while an object is being constructed
            if (_vtableSize >= 0)
                throw new Exception(
                    "FlatBuffers: object serialization must not be nested.");
        }

        public void StartTable(int numfields) {
            if (numfields < 0)
                throw new ArgumentOutOfRangeException("Flatbuffers: invalid numfields");

            NotNested();

            if (_vtable.Length < numfields)
                _vtable = new int[numfields];

            _vtableSize = numfields;
            _objectStart = Offset;
        }


        // Set the current vtable at `voffset` to the current location in the
        // buffer.
        public void Slot(int voffset) {
            if (voffset >= _vtableSize)
                throw new IndexOutOfRangeException("Flatbuffers: invalid voffset");

            _vtable[voffset] = Offset;
        }

        /// <summary>
        /// Adds a Boolean to the Table at index `o` in its vtable using the value `x` and default `d`
        /// </summary>
        /// <param name="o">The index into the vtable</param>
        /// <param name="x">The value to put into the buffer. If the value is equal to the default
        /// and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        /// <param name="d">The default value to compare the value against</param>
        public void AddBool(int o, bool x, bool d) { if (ForceDefaults || x != d) { AddBool(x); Slot(o); } }
        public void AddBool(int o, bool x, int d) { AddBool(o, x, d == 0 ? false : true); }
        /// <summary>
        /// Adds a SByte to the Table at index `o` in its vtable using the value `x` and default `d`
        /// </summary>
        /// <param name="o">The index into the vtable</param>
        /// <param name="x">The value to put into the buffer. If the value is equal to the default
        /// and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        /// <param name="d">The default value to compare the value against</param>
        public void AddSbyte(int o, sbyte x, sbyte d) { if (ForceDefaults || x != d) { AddSbyte(x); Slot(o); } }

        /// <summary>
        /// Adds a Byte to the Table at index `o` in its vtable using the value `x` and default `d`
        /// </summary>
        /// <param name="o">The index into the vtable</param>
        /// <param name="x">The value to put into the buffer. If the value is equal to the default
        /// and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        /// <param name="d">The default value to compare the value against</param>
        public void AddByte(int o, byte x, byte d) { if (ForceDefaults || x != d) { AddByte(x); Slot(o); } }

        /// <summary>
        /// Adds a Int16 to the Table at index `o` in its vtable using the value `x` and default `d`
        /// </summary>
        /// <param name="o">The index into the vtable</param>
        /// <param name="x">The value to put into the buffer. If the value is equal to the default
        /// and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        /// <param name="d">The default value to compare the value against</param>
        public void AddShort(int o, short x, int d) { if (ForceDefaults || x != d) { AddShort(x); Slot(o); } }

        /// <summary>
        /// Adds a UInt16 to the Table at index `o` in its vtable using the value `x` and default `d`
        /// </summary>
        /// <param name="o">The index into the vtable</param>
        /// <param name="x">The value to put into the buffer. If the value is equal to the default
        /// and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        /// <param name="d">The default value to compare the value against</param>
        public void AddUshort(int o, ushort x, ushort d) { if (ForceDefaults || x != d) { AddUshort(x); Slot(o); } }

        /// <summary>
        /// Adds an Int32 to the Table at index `o` in its vtable using the value `x` and default `d`
        /// </summary>
        /// <param name="o">The index into the vtable</param>
        /// <param name="x">The value to put into the buffer. If the value is equal to the default
        /// and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        /// <param name="d">The default value to compare the value against</param>
        public void AddInt(int o, int x, int d) { if (ForceDefaults || x != d) { AddInt(x); Slot(o); } }

        /// <summary>
        /// Adds a UInt32 to the Table at index `o` in its vtable using the value `x` and default `d`
        /// </summary>
        /// <param name="o">The index into the vtable</param>
        /// <param name="x">The value to put into the buffer. If the value is equal to the default
        /// and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        /// <param name="d">The default value to compare the value against</param>
        public void AddUint(int o, uint x, uint d) { if (ForceDefaults || x != d) { AddUint(x); Slot(o); } }

        /// <summary>
        /// Adds an Int64 to the Table at index `o` in its vtable using the value `x` and default `d`
        /// </summary>
        /// <param name="o">The index into the vtable</param>
        /// <param name="x">The value to put into the buffer. If the value is equal to the default
        /// and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        /// <param name="d">The default value to compare the value against</param>
        public void AddLong(int o, long x, long d) { if (ForceDefaults || x != d) { AddLong(x); Slot(o); } }

        /// <summary>
        /// Adds a UInt64 to the Table at index `o` in its vtable using the value `x` and default `d`
        /// </summary>
        /// <param name="o">The index into the vtable</param>
        /// <param name="x">The value to put into the buffer. If the value is equal to the default
        /// and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        /// <param name="d">The default value to compare the value against</param>
        public void AddUlong(int o, ulong x, ulong d) { if (ForceDefaults || x != d) { AddUlong(x); Slot(o); } }

        /// <summary>
        /// Adds a Single to the Table at index `o` in its vtable using the value `x` and default `d`
        /// </summary>
        /// <param name="o">The index into the vtable</param>
        /// <param name="x">The value to put into the buffer. If the value is equal to the default
        /// and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        /// <param name="d">The default value to compare the value against</param>
        public void AddFloat(int o, float x, double d) { if (ForceDefaults || x != d) { AddFloat(x); Slot(o); } }

        /// <summary>
        /// Adds a Double to the Table at index `o` in its vtable using the value `x` and default `d`
        /// </summary>
        /// <param name="o">The index into the vtable</param>
        /// <param name="x">The value to put into the buffer. If the value is equal to the default
        /// and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        /// <param name="d">The default value to compare the value against</param>
        public void AddDouble(int o, double x, double d) { if (ForceDefaults || x != d) { AddDouble(x); Slot(o); } }

        /// <summary>
        /// Adds a buffer offset to the Table at index `o` in its vtable using the value `x` and default `d`
        /// </summary>
        /// <param name="o">The index into the vtable</param>
        /// <param name="x">The value to put into the buffer. If the value is equal to the default
        /// the value will be skipped.</param>
        /// <param name="d">The default value to compare the value against</param>
        /// 
        public void AddOffset(int o, int x, int d) { if (x != d) { AddOffset(x); Slot(o); } }
        //public int[] AddOffsetWithReturn(int o, int x, int d) { if (x != d) { int[] offsetData = AddOffset(x); Slot(o); return offsetData; } else return null; }

        public static int DUMMYREF = 2;

        /*public void AddObjectReference(int o,int objRef, object obj) {
            if (objRef == -1) {
                // the current object is already in serialization process, so set a dummy ref for now and replace it later

                // set a dummy ref so we have an address set in the vtable
                var offsetData = AddOffsetWithReturn(o, DUMMYREF, 0);
                // keep a reference to address and Offset, so we know where to put the data, once it is there
                var address = offsetData[1];
                var Offset = offsetData[0];

                Service.Serializer.FlatBufferSerializer.AddAfterSerializationAction(() => {
                    var objOffset = Service.Serializer.FlatBufferSerializer.FindInSerializeCache(obj);
                    if (objOffset.HasValue) {
                        var off = Offset - objOffset.Value + sizeof(int);
                        DataBuffer.PutInt(address, off);
                    }
                });
                // add a fake offset to reserve the memory in the buffer
            } else {
                // we already have a valid ref to the serilaized version of the obj 
                AddOffset(11, objRef, 0);
            }
        }  */

        /// <summary>
        /// Encode the string `s` in the buffer using UTF-8.
        /// </summary>
        /// <param name="s">The string to encode.</param>
        /// <returns>
        /// The offset in the buffer where the encoded string starts.
        /// </returns>
        public StringOffset CreateString(string s, bool nested = false) {
            if (!nested) NotNested();
            AddByte(0);
            int utf8StringLen = Encoding.UTF8.GetByteCount(s);
            StartVector(1, utf8StringLen, 1, true);
            _bb.PutStringUTF8(_space -= utf8StringLen, s);
            return new StringOffset(EndVector().Value);
        }


#if ENABLE_SPAN_T
        /// <summary>
        /// Creates a string in the buffer from a Span containing
        /// a UTF8 string.
        /// </summary>
        /// <param name="chars">the UTF8 string to add to the buffer</param>
        /// <returns>
        /// The offset in the buffer where the encoded string starts.
        /// </returns>
        public StringOffset CreateUTF8String(Span<byte> chars)
        {
            NotNested();
            AddByte(0);
            var utf8StringLen = chars.Length;
            StartVector(1, utf8StringLen, 1);
            _space = _bb.Put(_space, chars);
            return new StringOffset(EndVector().Value);
        }
#endif

        /// <summary>
        /// Store a string in the buffer, which can contain any binary data.
        /// If a string with this exact contents has already been serialized before,
        /// instead simply returns the offset of the existing string.
        /// </summary>
        /// <param name="s">The string to encode.</param>
        /// <returns>
        /// The offset in the buffer where the encoded string starts.
        /// </returns>
        public StringOffset CreateSharedString(string s, bool nested = false) {
            if (_sharedStringMap == null) {
                _sharedStringMap = new Dictionary<string, StringOffset>();
            }

            if (_sharedStringMap.ContainsKey(s)) {
                return _sharedStringMap[s];
            }

            StringOffset stringOffset = CreateString(s, nested);
            _sharedStringMap.Add(s, stringOffset);
            return stringOffset;
        }

        /// @cond FLATBUFFERS_INTERNAL
        // Structs are stored inline, so nothing additional is being added.
        // `d` is always 0.
        public void AddStruct(int voffset, int x, int d) {
            if (x != d) {
                Nested(x);
                Slot(voffset);
            }
        }

        public int EndTable() {
            if (_vtableSize < 0)
                throw new InvalidOperationException(
                  "Flatbuffers: calling EndTable without a StartTable");

            AddInt((int)0);
            int vtableloc = Offset;
            // Write out the current vtable.
            int i = _vtableSize - 1;
            // Trim trailing zeroes.
            for (; i >= 0 && _vtable[i] == 0; i--) { }
            int trimmedSize = i + 1;
            for (; i >= 0; i--) {
                // Offset relative to the start of the table.
                short off = (short)(_vtable[i] != 0
                                        ? vtableloc - _vtable[i]
                                        : 0);
                AddShort(off);

                // clear out written entry
                _vtable[i] = 0;
            }

            const int standardFields = 2; // The fields below:
            AddShort((short)(vtableloc - _objectStart));
            AddShort((short)((trimmedSize + standardFields) *
                             sizeof(short)));

            // Search for an existing vtable that matches the current one.
            int existingVtable = 0;
            /*
            for (i = 0; i < _numVtables; i++) {
                int vt1 = _bb.Length - _vtables[i];
                int vt2 = _space;
                short len = _bb.GetShort(vt1);
                if (len == _bb.GetShort(vt2)) {
                    for (int j = sizeof(short); j < len; j += sizeof(short)) {
                        if (_bb.GetShort(vt1 + j) != _bb.GetShort(vt2 + j)) {
                            goto endLoop;
                        }
                    }
                    existingVtable = _vtables[i];
                    break;
                }

                endLoop: { }
            }*/

            if (existingVtable != 0) {
                // Found a match:
                // Remove the current vtable.
                _space = _bb.Length - vtableloc;
                // Point table to existing vtable.
                _bb.PutInt(_space, existingVtable - vtableloc);
            } else {
                // No match:
                // Add the location of the current vtable to the list of
                // vtables.
                //if (_numVtables == _vtables.Length)
                //{
                //    // Arrays.CopyOf(vtables num_vtables * 2);
                //    var newvtables = new int[ _numVtables * 2];
                //    Array.Copy(_vtables, newvtables, _vtables.Length);

                //    _vtables = newvtables;
                //};
                //_vtables[_numVtables++] = Offset;

                // Point table to current vtable.
                _bb.PutInt(_bb.Length - vtableloc, Offset - vtableloc);
            }

            _vtableSize = -1;
            return vtableloc;
        }

        // This checks a required field has been set in a given table that has
        // just been constructed.
        public void Required(int table, int field) {
            int table_start = _bb.Length - table;
            int vtable_start = table_start - _bb.GetInt(table_start);
            bool ok = _bb.GetShort(vtable_start + field) != 0;
            // If this fails, the caller will show what field needs to be set.
            if (!ok)
                throw new InvalidOperationException("FlatBuffers: field " + field +
                                                    " must be set");
        }
        /// @endcond

        /// <summary>
        /// Finalize a buffer, pointing to the given `root_table`.
        /// </summary>
        /// <param name="rootTable">
        /// An offset to be added to the buffer.
        /// </param>
        /// <param name="sizePrefix">
        /// Whether to prefix the size to the buffer.
        /// </param>
        protected void Finish(int rootTable, bool sizePrefix) {
            Prep(_minAlign, sizeof(int) + (sizePrefix ? sizeof(int) : 0));
            AddOffset(rootTable);
            if (sizePrefix) {
                AddInt(_bb.Length - _space);
            }
            _bb.Position = _space;
        }

        /// <summary>
        /// Finalize a buffer, pointing to the given `root_table`.
        /// </summary>
        /// <param name="rootTable">
        /// An offset to be added to the buffer.
        /// </param>
        public void Finish(int rootTable) {
            Finish(rootTable, false);
        }

        /// <summary>
        /// Finalize a buffer, pointing to the given `root_table`, with the size prefixed.
        /// </summary>
        /// <param name="rootTable">
        /// An offset to be added to the buffer.
        /// </param>
        public void FinishSizePrefixed(int rootTable) {
            Finish(rootTable, true);
        }

        /// <summary>
        /// Get the ByteBuffer representing the FlatBuffer.
        /// </summary>
        /// <remarks>
        /// This is typically only called after you call `Finish()`.
        /// The actual data starts at the ByteBuffer's current position,
        /// not necessarily at `0`.
        /// </remarks>
        /// <returns>
        /// Returns the ByteBuffer for this FlatBuffer.
        /// </returns>
        public ByteBuffer DataBuffer { get { return _bb; } }

        /// <summary>
        /// A utility function to copy and return the ByteBuffer data as a
        /// `byte[]`.
        /// </summary>
        /// <returns>
        /// A full copy of the FlatBuffer data.
        /// </returns>
        public byte[] SizedByteArray() {
            return _bb.ToSizedArray();
        }

        /// <summary>
        /// Finalize a buffer, pointing to the given `rootTable`.
        /// </summary>
        /// <param name="rootTable">
        /// An offset to be added to the buffer.
        /// </param>
        /// <param name="fileIdentifier">
        /// A FlatBuffer file identifier to be added to the buffer before
        /// `root_table`.
        /// </param>
        /// <param name="sizePrefix">
        /// Whether to prefix the size to the buffer.
        /// </param>
        protected void Finish(int rootTable, string fileIdentifier, bool sizePrefix) {
            Prep(_minAlign, sizeof(int) + (sizePrefix ? sizeof(int) : 0) +
                            FlatBufferConstants.FileIdentifierLength);
            if (fileIdentifier.Length !=
                FlatBufferConstants.FileIdentifierLength)
                throw new ArgumentException(
                    "FlatBuffers: file identifier must be length " +
                    FlatBufferConstants.FileIdentifierLength,
                    "fileIdentifier");
            for (int i = FlatBufferConstants.FileIdentifierLength - 1; i >= 0;
                 i--) {
                AddByte((byte)fileIdentifier[i]);
            }
            Finish(rootTable, sizePrefix);
        }

        /// <summary>
        /// Finalize a buffer, pointing to the given `rootTable`.
        /// </summary>
        /// <param name="rootTable">
        /// An offset to be added to the buffer.
        /// </param>
        /// <param name="fileIdentifier">
        /// A FlatBuffer file identifier to be added to the buffer before
        /// `root_table`.
        /// </param>
        public void Finish(int rootTable, string fileIdentifier) {
            Finish(rootTable, fileIdentifier, false);
        }

        /// <summary>
        /// Finalize a buffer, pointing to the given `rootTable`, with the size prefixed.
        /// </summary>
        /// <param name="rootTable">
        /// An offset to be added to the buffer.
        /// </param>
        /// <param name="fileIdentifier">
        /// A FlatBuffer file identifier to be added to the buffer before
        /// `root_table`.
        /// </param>
        public void FinishSizePrefixed(int rootTable, string fileIdentifier) {
            Finish(rootTable, fileIdentifier, true);
        }

        /// <summary>
        /// Inline vec2
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int PutVector2(float x, float y) {
            Prep(4, 8);
            PutFloat(y);
            PutFloat(x);
            return Offset;
        }

        public int PutVector2(ref UnityEngine.Vector2 vec2) {
            return PutVector2(vec2.x, vec2.y);
        }
        public int PutVector2(UnityEngine.Vector2 vec2) {
            return PutVector2(vec2.x, vec2.y);
        }

        /// <summary>
        /// Inline vec3
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public int PutVector3(float x, float y, float z) {
            Prep(4, 12);
            PutFloat(z);
            PutFloat(y);
            PutFloat(x);
            return Offset;
        }

        public int PutVector3(ref UnityEngine.Vector3 vec3) {
            return PutVector3(vec3.x, vec3.y, vec3.z);
        }
        public int PutVector3(UnityEngine.Vector3 vec3) {
            return PutVector3(vec3.x, vec3.y, vec3.z);
        }

        /// <summary>
        /// Inline vec4
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        public int PutVector4(float x, float y, float z, float w) {
            Prep(4, 16);
            PutFloat(w);
            PutFloat(z);
            PutFloat(y);
            PutFloat(x);
            return Offset;
        }

        public int PutVector4(ref UnityEngine.Vector4 vec4) {
            return PutVector4(vec4.x, vec4.y, vec4.z, vec4.w);
        }
        public int PutVector4(UnityEngine.Vector4 vec4) {
            return PutVector4(vec4.x, vec4.y, vec4.z, vec4.w);
        }

        public int PutQuaternion(ref Quaternion q) {
            return PutVector4(q.x, q.y, q.z, q.w);
        }
        public int PutQuaternion(Quaternion q) {
            return PutVector4(q.x, q.y, q.z, q.w);
        }

        public int PutUID(ECS.UID uid) {
            Prep(4, 8);
            PutInt(uid.Revision);
            PutInt(uid.ID);
            return Offset;
        }

        public int PutUID(ref ECS.UID uid) {
            Prep(4, 8);
            PutInt(uid.Revision);
            PutInt(uid.ID);
            return Offset;
        }

        public int CreateList(IList list, SerializationContext sctx) {
            Type innerType = list.GetType().GetGenericArguments()[0];
            
            if (innerType == typeString) {
                return CreateStringList(list);
            }
            else if (innerType.IsPrimitive || innerType.IsEnum ) {
                return CreatePrimitiveList(list);
            } 
            else if (innerType.IsValueType) {
                return CreateStructList(list);
            }

            return CreateNonPrimitiveList(list, sctx);
        }

        public int CreateTypedObjectList(IList list,SerializationContext sctx) {
            return 0;
        }

        public int CreatePrimitiveList(IList list) {
            // I need to use IList here without generic due to usage from within the ReferenceResolving
            // TODO: Rethink how to reinvent this workflow with generics?!
            if (list == null) return 0;

            int count = list.Count;

            Type innerType = list.GetType().GetGenericArguments()[0];

            if (!(innerType.IsPrimitive||innerType.IsEnum) ) {
                Debug.LogError($"using non primitive type for primitive list:{innerType}");
            }
            
            if (innerType.IsEnum) {
                StartVector(4, count, 4);
                for (int i = count - 1; i >= 0; i--) AddInt((int)(object) list[i]);
                return EndVector().Value;
            }
            else if (innerType == typeInt) {
                IList<int> primList = (IList<int>)list;
                StartVector(4, count, 4);
                int[] buf = GetUnderlyingArray(primList);
                Add(buf, count);
                return EndVector().Value;
            } 
            else if (innerType == typeFloat) {
                IList<float> primList = (IList<float>)list;
                StartVector(4, count, 4);
                float[] buf = GetUnderlyingArray(primList);
                Add(buf, count);
                return EndVector().Value;
            } else if (innerType == typeBool) {
                IList<bool> primList = (IList<bool>)list;
                StartVector(1, count, 1);
                bool[] buf = GetUnderlyingArray(primList);
                Add(buf, count);
                return EndVector().Value;
            } else if (innerType == typeLong) {
                IList<long> primList = (IList<long>)list;
                StartVector(8, count, 8);
                long[] buf = GetUnderlyingArray(primList);
                Add(buf, count);
                return EndVector().Value;
            } else if (innerType == typeShort) {
                IList<short> primList = (IList<short>)list;
                StartVector(2, count, 2);
                short[] buf = GetUnderlyingArray(primList);
                Add(buf, count);
                return EndVector().Value;
            } else if (innerType == typeByte) {
                IList<byte> primList = (IList<byte>)list;
                StartVector(1, count, 1);
                byte[] buf = GetUnderlyingArray(primList);
                Add(buf, count);
                return EndVector().Value;
            } else {
                Debug.LogError($"Unsupported primitive-list-type: {innerType}");
            }

            Debug.LogError($"PrimitveList: Do not know how to serialize type:{innerType}");

            return 0;

        }

        private void WriteStructListHeader(int count,bool writeLengthInfo,int reserveAdditionalBytes) {
            if (writeLengthInfo) {
                PutInt(count);
            }
            if (reserveAdditionalBytes != 0) {
                MoveCurrentOffset(reserveAdditionalBytes);
            }
        }

        public int CreateStructList(IList list, int reserveAdditionalBytes=0, bool writeLengthInfo=true) {
            if (list == null) return 0;

            int count = list.Count;

            Type innerType = list.GetType().GetGenericArguments()[0];

            if (innerType == typeUID) {
                for (int i = count - 1; i >= 0; i--) {
                    PutUID((ECS.UID)(object)list[i]);
                }
                WriteStructListHeader(count, writeLengthInfo, reserveAdditionalBytes);
                return Offset;
            } 
              // struct list
              // I know here comes lots of repetition. Need to find an efficient way 
              // to abstract this without too much overhead. Until then, every type
              // gets its dedicated loop of its own. Too tired for fancy generic magic ;)
              // ...and this casting-madness if used here... :|
            else if (typeIFBserializabel2Struct.IsAssignableFrom(innerType)) {
                for (int i = count - 1; i >= 0; i--) {
                    IFBSerializable2Struct ifbStruct = (IFBSerializable2Struct)list[i];
                    ifbStruct.Put(this);
                }
                WriteStructListHeader(count, writeLengthInfo, reserveAdditionalBytes);
                return Offset;
            } else if (innerType == typeVector2) {
                for (int i = count - 1; i >= 0; i--) {
                    PutVector2((Vector2)(object)list[i]);
                }
                PutInt(count);
                return Offset;
            } else if (innerType == typeVector3) {
                for (int i = count - 1; i >= 0; i--) {
                    PutVector3((Vector3)(object)list[i]);
                }
                WriteStructListHeader(count, writeLengthInfo, reserveAdditionalBytes);
                return Offset;

            } else if (innerType == typeVector4) {
                for (int i = count - 1; i >= 0; i--) {
                    PutVector4((Vector4)(object)(list[i]));
                }
                WriteStructListHeader(count, writeLengthInfo, reserveAdditionalBytes);
                return Offset;

            } else if (innerType == typeQuaternion) {
                for (int i = count - 1; i >= 0; i--) {
                    PutQuaternion((Quaternion)(object)list[i]);
                }
                WriteStructListHeader(count, writeLengthInfo, reserveAdditionalBytes);
                return Offset;
            }


            Debug.LogError($"StructList: Do not know how to serialize type:{innerType}");

            return 0;
        }



        private List<int> tempOffsets = new List<int>();

        public int CreateStringList(IList stringList) {
            if (stringList == null) return 0;

            tempOffsets.Clear();

            int count = stringList.Count;
            foreach (string st in stringList) {
                tempOffsets.Add(CreateString(st).Value);
            }
            StartVector(4, count, 4);
            for (int i = count - 1; i >= 0; i--) {
                AddOffset(tempOffsets[i]);
            }
            return EndVector().Value;
        }

        //public int CreateNonPrimitiveList<T>(IList<T> list, SerializationContext ctx = null) {
        //    return CreateNonPrimitiveList(list, ctx);
        //}


        public int CreateNonPrimitiveList(IObservableList obsList, SerializationContext ctx) {
            return CreateNonPrimitiveList(obsList.InnerIList, ctx);
        }

        public int CreateNonPrimitiveList(IList list, SerializationContext ctx) {
            int count = list.Count;

            if (list == null) {
                return 0;
            }

            if (ctx == null) {
                Debug.LogError("you need to specfiy serialization context if using CreateNonPrimitiveList with objects");
                return 0;
            }

            //StartVector(4, count, 4);
            Prep(sizeof(int), 4 * count);
            Prep(4, 4 * count); // Just in case alignment > int.

            for (int i = count - 1; i >= 0; i--) {
                object obj = list[i];
                if (obj == null) {
                    PutInt(0);
                    continue;
                }
                //else if (!(obj is IFBSerializable2)) {
                //    Debug.LogError($"Tried to serialize unsupported object in CreateNonPrimitiveList:{obj.GetType()}! Ignoreing it. writing as null");
                //    PutInt(0);
                //    continue;
                //}


                ctx.AddReferenceOffset(-1, obj);
            }
            AddInt(count);
            return Offset;
        }

        public bool IsTypedObjectType(Type type) {
            return ExtendedTable.typeISerializeAsTypedObject.IsAssignableFrom(type);
        }

        public int CreateDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict, SerializationContext sctx) {
            return CreateIDictionary(dict, sctx);
        }

        public int CreateDictionary<TKey, TValue>(ObservableDictionary<TKey, TValue> dict, SerializationContext sctx) {
            return CreateIDictionary(dict.InnerDictionary, sctx);
        }

        public int CreateIDictionary(IDictionary dict,SerializationContext sctx) {
            if (dict == null) return 0;

            if (sctx == null) {
                Debug.LogError($"CreateIDictionary: sctx not set. DictType:{dict.GetType()}");
                return 0;
            }

            Type typeDict = dict.GetType();
            Type typeKey = typeDict.GetGenericArguments()[0];
            Type typeValue = typeDict.GetGenericArguments()[1];

            bool keyPrimitive = typeKey.IsPrimitive || typeKey.IsEnum;
            bool keyIsStruct = !keyPrimitive && typeKey.IsValueType;
            bool valuePrimitive = typeValue.IsPrimitive || typeValue.IsEnum;
            bool valueIsStruct = !valuePrimitive && typeValue.IsValueType;

            bool IsKeyTypedObject = IsTypedObjectType(typeKey);
            bool IsValueTypedObject = IsTypedObjectType(typeValue);

            int count = dict.Count;
            int keySize = (keyPrimitive || keyIsStruct) ? ByteBuffer.SizeOf(typeKey) : (IsKeyTypedObject?8:4);
            int valueSize = (valuePrimitive || valueIsStruct) ? ByteBuffer.SizeOf(typeValue) : (IsValueTypedObject ? 8 : 4);
            int elementSize = keySize + valueSize;
            int overallSize = elementSize * count + ByteBuffer.SizeOf(typeInt);

            // prepare space
            Prep(4,overallSize-4);

            // set startposition
            int dictionaryStart = Offset;

            PutCollectionData(dict.Keys, sctx, typeKey, keyPrimitive,keyIsStruct, elementSize);
            int dictHead = _space;
            // set the pointer on the value 'after' the dict by adding the keySize.
            // (before writing the first element, we subtract elementSize and are then on a valid position) 
            _space = Off2Buf(dictionaryStart) + valueSize;
            PutCollectionData(dict.Values, sctx, typeValue, valuePrimitive,valueIsStruct, elementSize);
            
            _space = dictHead;
            PutInt(count);
            return Offset;
        }

        private void PutCollectionData(ICollection data, SerializationContext sctx, Type type, bool isPrimitive,bool isStruct, int elementSize) {
            // lots of reperition....(to avoid if checks in the loop. 
            // TODO: Check how big the check for type impact would be...
            if (isPrimitive) {
                int spaceTemp = _space;
                if (type == typeInt || type.IsEnum) {
                    foreach (int elem in data) {
                        _space -= elementSize;
                        _bb.PutInt(_space, (int)(object)elem); // I don't want to, but I really don't know how to prevent it
                    }
                } else if (type == typeFloat) {
                    foreach (float elem in data) {
                        _space = spaceTemp -= elementSize;
                        _bb.PutFloat(_space,(float)(object)elem); // I don't want to, but I really don't know how to prevent it
                    }
                } else if (type == typeBool) {
                    foreach (bool elem in data) {
                        _space = spaceTemp -= elementSize;
                        _bb.Put(_space,((bool)(object)elem)?(byte)0:(byte)1); // I don't want to, but I really don't know how to prevent it
                    }
                } else if (type == typeShort) {
                    foreach (short elem in data) {
                        _space = spaceTemp -= elementSize;
                        _bb.PutShort(_space,(short)(object)elem); // I don't want to, but I really don't know how to prevent it
                    }
                } else if (type == typeByte) {
                    foreach (byte elem in data) {
                        _space = spaceTemp -= elementSize;
                        _bb.Put(_space,(byte)(object)elem); // I don't want to, but I really don't know how to prevent it
                    }
                } else if (type == typeLong) {
                    foreach (long elem in data) {
                        _space = spaceTemp -= elementSize;
                        _bb.PutLong(_space,(long)(object)elem); // I don't want to, but I really don't know how to prevent it
                    }
                } else Debug.LogError("Could not handle primitive type:" + type);
                return;
            } 
            else if (isStruct) {
                int spaceTemp = _space + 4;
                if (type == typeUID) {
                    foreach (ECS.UID elem in data) {
                        _space = spaceTemp -= elementSize;
                        PutUID(elem); // I don't want to, but I really don't know how to prevent it
                    }
                } 
                else if (type == typeVector3) {
                    foreach (Vector3 elem in data) {
                        _space = spaceTemp -= elementSize;
                        PutVector3(elem); // I don't want to, but I really don't know how to prevent it
                    }
                } 
                else if (type == typeQuaternion) {
                    foreach (Quaternion elem in data) {
                        _space = spaceTemp -= elementSize;
                        PutQuaternion(elem);
                    }
                } else if (type == typeVector4) {
                    foreach (Vector4 elem in data) {
                        _space = spaceTemp -= elementSize;
                        PutVector4(elem); 
                    }
                } else if (type == typeVector2) {
                    foreach (Vector2 elem in data) {
                        _space = spaceTemp -= elementSize;
                        PutVector2(elem); 
                    }
                } else if (type == typeVector3) {
                    foreach (Vector3 elem in data) {
                        _space = spaceTemp -= elementSize;
                        PutVector3(elem); // I don't want to, but I really don't know how to prevent it
                    }
                } 
                else if (typeIFBserializabel2Struct.IsAssignableFrom(type)) {
                    foreach (IFBSerializable2Struct elem in data) {
                        _space = spaceTemp -= elementSize;
                        elem.Put(this); // I don't want to, but I really don't know how to prevent it
                    }
                }
                else {
                    throw new ArgumentException($"Unsupported struct-dict-type: {type} ");
                }

                return;
            } 
            else if (!isPrimitive) {
                _space += 4;
                int spaceTemp = _space;
                foreach (object elem in data) {
                    _space = spaceTemp -= elementSize;
                    sctx.AddReferenceOffset(elem);
                    //sctx.AddLateReference(Offset, elem); // tell the sctx where to later put the offset to the serialized object
                }
                return;
            }
            throw new ArgumentException($"Unsupported Dictionary-Innertype {type}");
        }

       
        /// <summary>
        /// Returns the offset's (current) buffer-position
        /// Caution: if the buffer grows those buffer-positions get
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int Off2Buf(int offset) {
            return _bb.Length - offset;
        }

        /// <summary>
        /// Get typename including Assembly-Name
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        public String GetTypeName(object elem) {
            Type type = elem.GetType();
            return GetTypeName(type);
        }

        static Dictionary<Type, String> typeNameLookupTable = new Dictionary<Type, string>();

        static StringBuilder stb = new StringBuilder();

        private static String GetTypeName(Type type) {
            if (typeNameLookupTable.TryGetValue(type, out string value)) {
                return value;
            } else {
                stb.Clear();
                string assemblyName = type.Assembly.FullName;

                stb.Append(type.FullName).Append(", ").Append(assemblyName.Substring(0, assemblyName.IndexOf(',')));
                string typeName = stb.ToString();
                typeNameLookupTable[type] = typeName;
                return typeName;
            }
        }

        public static T[] GetUnderlyingArray<T>(IList<T> list) {
            var field = list.GetType().GetField("_items",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            return (T[])field.GetValue(list);
        }

    }
}

/// @}
