﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SharpFileSystem.IO {

    // CircularBuffer from http://circularbuffer.codeplex.com/.
    public class CircularBuffer<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable {
        int _capacity;
        int _size;
        int _head;
        int _tail;
        T[] _buffer;

        [NonSerialized]
        object _syncRoot;

        /// <summary>
        /// Constructor
        /// </summary>
        public CircularBuffer(int capacity): this(capacity, false) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CircularBuffer(int capacity, bool allowOverflow) {
            if(capacity < 0) throw new ArgumentException("capacity must be greater than or equal to zero.", nameof(capacity));

            this._capacity = capacity;
            _size = 0;
            _head = 0;
            _tail = 0;
            _buffer = new T[capacity];
            AllowOverflow = allowOverflow;
        }

        public bool AllowOverflow { get; set; }

        public int Capacity {
            get => _capacity;
            set {
                if(value == _capacity) return;

                if(value < _size) throw new ArgumentOutOfRangeException(nameof(value), "value must be greater than or equal to the buffer size.");

                var dst = new T[value];
                if(_size > 0) CopyTo(dst);
                _buffer = dst;

                _capacity = value;
            }
        }

        public int Size => _size;

        public bool Contains(T item) {
            int bufferIndex = _head;
            var comparer = EqualityComparer<T>.Default;
            for(int i = 0; i < _size; i++, bufferIndex++) {
                if(bufferIndex == _capacity) bufferIndex = 0;

                if(item == null && _buffer[bufferIndex] == null) {
                    return true;
                } else
                if((_buffer[bufferIndex] != null) && comparer.Equals(_buffer[bufferIndex], item)) {
                    return true;
                }
            }

            return false;
        }

        public void Clear() {
            _size = 0;
            _head = 0;
            _tail = 0;
        }

        public int Put(T[] src) {
            return Put(src, 0, src.Length);
        }

        public int Put(T[] src, int offset, int count) {
            int realCount = AllowOverflow ? count : Math.Min(count, _capacity - _size);
            int srcIndex = offset;
            for(int i = 0; i < realCount; i++, _tail++, srcIndex++) {
                if(_tail == _capacity) _tail = 0;
                _buffer[_tail] = src[srcIndex];
            }
            _size = Math.Min(_size + realCount, _capacity);
            return realCount;
        }

        public void Put(T item) {
            if(!AllowOverflow && _size == _capacity) throw new InternalBufferOverflowException("Buffer is full.");

            _buffer[_tail] = item;
            if(_tail++ == _capacity) _tail = 0;
            _size++;
        }

        public void Skip(int count) {
            _head += count;
            if(_head >= _capacity) _head -= _capacity;
        }

        public T[] Get(int count) {
            var dst = new T[count];
            Get(dst);
            return dst;
        }

        public int Get(T[] dst) {
            return Get(dst, 0, dst.Length);
        }

        public int Get(T[] dst, int offset, int count) {
            int realCount = Math.Min(count, _size);
            int dstIndex = offset;
            for(int i = 0; i < realCount; i++, _head++, dstIndex++) {
                if(_head == _capacity) _head = 0;
                dst[dstIndex] = _buffer[_head];
            }
            _size -= realCount;
            return realCount;
        }

        public T Get() {
            if(_size == 0) throw new InvalidOperationException("Buffer is empty.");

            var item = _buffer[_head];
            if(_head++ == _capacity) _head = 0;
            _size--;
            return item;
        }

        public void CopyTo(T[] array) {
            CopyTo(array, 0);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            CopyTo(0, array, arrayIndex, _size);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count) {
            if(count > _size) throw new ArgumentOutOfRangeException(nameof(count),  "count cannot be greater than the buffer size.");

            int bufferIndex = _head;
            for(int i = 0; i < count; i++, bufferIndex++, arrayIndex++) {
                if(bufferIndex == _capacity) bufferIndex = 0;
                array[arrayIndex] = _buffer[bufferIndex];
            }
        }

        public IEnumerator<T> GetEnumerator() {
            int bufferIndex = _head;
            for(int i = 0; i < _size; i++, bufferIndex++) {
                if(bufferIndex == _capacity) bufferIndex = 0;

                yield return _buffer[bufferIndex];
            }
        }

        public T[] GetBuffer() {
            return _buffer;
        }

        public T[] ToArray() {
            var dst = new T[_size];
            CopyTo(dst);
            return dst;
        }

        #region ICollection<T> Members

        int ICollection<T>.Count => Size;

        bool ICollection<T>.IsReadOnly => false;

        void ICollection<T>.Add(T item) {
            Put(item);
        }

        bool ICollection<T>.Remove(T item) {
            if(_size == 0) return false;

            Get();
            return true;
        }

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion

        #region ICollection Members

        int ICollection.Count => Size;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot {
            get {
                if(_syncRoot == null) Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                return _syncRoot;
            }
        }

        void ICollection.CopyTo(Array array, int arrayIndex) {
            CopyTo((T[])array, arrayIndex);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
    }
}
