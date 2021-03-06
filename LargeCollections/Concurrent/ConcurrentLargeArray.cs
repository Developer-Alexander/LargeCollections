/*
MIT License
SPDX-License-Identifier: MIT

Copyright (c) 2022 Developer Alexander

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Diagnostics;

namespace LargeCollections
{
    /// <summary>
    /// This is a thread-safe version of <see cref="LargeArray{T}"/>.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class ConcurrentLargeArray<T> : ILargeArray<T>
    {
        protected LargeArray<T> _storage;

        public ConcurrentLargeArray(long capacity = 1L)
        {
            _storage = new LargeArray<T>(capacity);
        }

        public T this[long index] 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _storage.Get(index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _storage.Set(index, value);
        }

        T IReadOnlyLargeArray<T>.this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _storage.Get(index);
        }

        public long Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_storage)
                {
                    return _storage.Count;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long BinarySearch(T item, Comparer<T> comparer = null)
        {
            lock (_storage)
            {
                return _storage.BinarySearch(item, comparer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            lock (_storage)
            {
                return _storage.Contains(item);
            }
        }

        public void DoForEach(Action<T> action)
        {
            lock (_storage)
            {
                _storage.DoForEach(action);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(long index)
        {
            lock(_storage)
            {
                return _storage.Get(index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetAll()
        {
            lock (_storage)
            {
                foreach(T item in _storage)
                {
                    yield return item;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(long capacity)
        {
            lock (_storage)
            {
                _storage.Resize(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(long index, T item)
        {
            lock (_storage)
            {
                _storage.Set(index, item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(Comparer<T> comparer)
        {
            lock (_storage)
            {
                _storage.Sort(comparer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }
    }
}