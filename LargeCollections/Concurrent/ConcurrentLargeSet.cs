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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LargeCollections
{
    /// <summary>
    /// This is a thread-safe version of <see cref="LargeSet{T}"/>.
    /// </summary>
    [DebuggerDisplay("ConcurrentLargeSet: Count = {Count}")]
    public class ConcurrentLargeSet<T> : ILargeCollection<T>
    {
        protected LargeSet<T> _storage;
        public ConcurrentLargeSet(Comparer<T> comparer = null,
            long capacity = 1L,
            double capacityGrowFactor = LargeCollectionsConstants.DefaultCapacityGrowFactor,
            long fixedCapacityGrowAmount = LargeCollectionsConstants.DefaultFixedCapacityGrowAmount,
            long fixedCapacityGrowLimit = LargeCollectionsConstants.DefaultFixedCapacityGrowLimit,
            double minLoadFactor = LargeCollectionsConstants.DefaultMinLoadFactor,
            double maxLoadFactor = LargeCollectionsConstants.DefaultMaxLoadFactor)
        {
            _storage = new LargeSet<T>(comparer, capacity, capacityGrowFactor, fixedCapacityGrowAmount, fixedCapacityGrowLimit, minLoadFactor, maxLoadFactor);
        }

        public ConcurrentLargeSet(IEnumerable<T> items,
            Comparer<T> comparer = null,
            long capacity = 1L,
            double capacityGrowFactor = LargeCollectionsConstants.DefaultCapacityGrowFactor,
            long fixedCapacityGrowAmount = LargeCollectionsConstants.DefaultFixedCapacityGrowAmount,
            long fixedCapacityGrowLimit = LargeCollectionsConstants.DefaultFixedCapacityGrowLimit,
            double minLoadFactor = LargeCollectionsConstants.DefaultMinLoadFactor,
            double maxLoadFactor = LargeCollectionsConstants.DefaultMaxLoadFactor)
        {
            _storage = new LargeSet<T>(items, comparer, capacity, capacityGrowFactor, fixedCapacityGrowAmount, fixedCapacityGrowLimit, minLoadFactor, maxLoadFactor);
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
        public void Add(T item)
        {
            lock (_storage)
            {
                _storage.Add(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(IEnumerable<T> items)
        {
            lock (_storage)
            {
                foreach (T item in items)
                {
                    _storage.Add(item);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            lock (_storage)
            {
                _storage.Clear();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoForEach(Action<T> action)
        {
            lock (_storage)
            {
                _storage.DoForEach(action);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetAll()
        {
            lock (_storage)
            {
                foreach (T item in _storage)
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
        public void Remove(T item)
        {
            lock (_storage)
            {
                _storage.Remove(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(IEnumerable<T> items)
        {
            lock (_storage)
            {
                foreach (T item in items)
                {
                    _storage.Remove(item);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Shrink()
        {
            lock (_storage)
            {
                _storage.Shrink();
            }
        }
    }
}
