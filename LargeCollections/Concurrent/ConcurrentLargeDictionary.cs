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
using System.Text;

namespace LargeCollections
{
    /// <summary>
    /// This is a thread-safe version of <see cref="LargeDictionary{TKey, TValue}"/>.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class ConcurrentLargeDictionary<TKey, TValue> : ILargeDictionary<TKey, TValue>
    {
        protected LargeDictionary<TKey, TValue> _storage;

        public ConcurrentLargeDictionary(Comparer<TKey> comparer = null,
            long capacity = 1L,
            double capacityGrowFactor = LargeCollectionsConstants.DefaultCapacityGrowFactor,
            long fixedCapacityGrowAmount = LargeCollectionsConstants.DefaultFixedCapacityGrowAmount,
            long fixedCapacityGrowLimit = LargeCollectionsConstants.DefaultFixedCapacityGrowLimit,
            double minLoadFactor = LargeCollectionsConstants.DefaultMinLoadFactor,
            double maxLoadFactor = LargeCollectionsConstants.DefaultMaxLoadFactor,
            double minLoadFactorTolerance = LargeCollectionsConstants.DefaultMinLoadFactorTolerance)
        {
            _storage = new LargeDictionary<TKey, TValue>(comparer,
                  capacity,
                  capacityGrowFactor,
                  fixedCapacityGrowAmount,
                  fixedCapacityGrowLimit,
                  minLoadFactor,
                  maxLoadFactor,
                  minLoadFactorTolerance);
        }

        public TValue this[TKey key] 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_storage)
                {
                    return _storage[key];
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                lock (_storage)
                {
                    _storage[key] = value;
                }
            }
        }

        TValue IReadOnlyLargeDictionary<TKey, TValue>.this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_storage)
                {
                    return _storage[key];
                }
            }
        }

        public IEnumerable<TKey> Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_storage)
                {
                    foreach(TKey key in _storage.Keys)
                    {
                        yield return key;
                    }
                }
            }
        }

        public IEnumerable<TValue> Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_storage)
                {
                    foreach (TValue value in _storage.Values)
                    {
                        yield return value;
                    }
                }
            }
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
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock(_storage)
            {
                _storage.Add(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            lock (_storage)
            {
                foreach (var item in items)
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
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (_storage)
            {
                return _storage.Contains(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key)
        {
            lock (_storage)
            {
                return _storage.ContainsKey(key);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoForEach(Action<KeyValuePair<TKey, TValue>> action)
        {
            lock (_storage)
            {
                _storage.DoForEach(action);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue Get(TKey key)
        {
            lock (_storage)
            {
                return _storage.Get(key);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<KeyValuePair<TKey, TValue>> GetAll()
        {
            lock (_storage)
            {
                foreach(var item in _storage)
                {
                    yield return item;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(TKey key)
        {
            lock (_storage)
            {
                _storage.Remove(key);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (_storage)
            {
                _storage.Remove(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            lock (_storage)
            {
                foreach (var item in items)
                {
                    _storage.Remove(item);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(TKey key, TValue value)
        {
            lock (_storage)
            {
                _storage.Set(key, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_storage)
            {
                return _storage.TryGetValue(key, out value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }
    }
}
