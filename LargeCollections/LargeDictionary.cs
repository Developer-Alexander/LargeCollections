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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace LargeCollections
{
    [DebuggerDisplay("Count = {Count}")]
    public class LargeDictionary<TKey, TValue> : LargeSet<KeyValuePair<TKey, TValue>>, ILargeDictionary<TKey, TValue>
    {
        public class KeyOnlyComparer : Comparer<KeyValuePair<TKey, TValue>>
        {
            private Comparer<TKey> _baseComparer;

            public KeyOnlyComparer(Comparer<TKey> baseComparer = null)
            {
                _baseComparer = baseComparer ?? Comparer<TKey>.Default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
            {
                return _baseComparer.Compare(x.Key, y.Key);
            }
        }

        public LargeDictionary(Comparer<TKey> comparer = null,
            long capacity = 1L,
            double capacityGrowFactor = LargeCollectionsConstants.DefaultCapacityGrowFactor,
            long fixedCapacityGrowAmount = LargeCollectionsConstants.DefaultFixedCapacityGrowAmount,
            long fixedCapacityGrowLimit = LargeCollectionsConstants.DefaultFixedCapacityGrowLimit,
            double minLoadFactor = LargeCollectionsConstants.DefaultMinLoadFactor,
            double maxLoadFactor = LargeCollectionsConstants.DefaultMaxLoadFactor,
            double minLoadFactorTolerance = LargeCollectionsConstants.DefaultMinLoadFactorTolerance)

            : base(new KeyOnlyComparer(comparer),
                  capacity,
                  capacityGrowFactor,
                  fixedCapacityGrowAmount,
                  fixedCapacityGrowLimit,
                  minLoadFactor,
                  maxLoadFactor,
                  minLoadFactorTolerance)
        {
            _hashCodeFunction = (KeyValuePair<TKey, TValue> item) => item.Key.GetHashCode();
        }

        public IEnumerable<TKey> Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                long capacity = Capacity;

                for (long i = 0L; i < capacity; i++)
                {
                    SetElement element = _storage[i];

                    while (element != null)
                    {
                        yield return element.Item.Key;
                        element = element.Next;
                    }
                }
            }
        }

        public IEnumerable<TValue> Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                long capacity = Capacity;

                for (long i = 0L; i < capacity; i++)
                {
                    SetElement element = _storage[i];

                    while (element != null)
                    {
                        yield return element.Item.Value;
                        element = element.Next;
                    }
                }
            }
        }

        TValue IReadOnlyLargeDictionary<TKey, TValue>.this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Get(key);
        }

        public TValue this[TKey key] 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Get(key);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(TKey key, TValue value)
        {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(TKey key)
        {
            Remove(new KeyValuePair<TKey, TValue>(key, default(TValue)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue Get(TKey key)
        {
            KeyValuePair<TKey, TValue> keyItem = new KeyValuePair<TKey, TValue>(key, default(TValue));
            if (!TryGetValue(keyItem, out KeyValuePair<TKey, TValue> value))
            {
                throw new KeyNotFoundException(key.ToString());
            }

            return value.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key)
        {
            KeyValuePair<TKey, TValue> keyItem = new KeyValuePair<TKey, TValue>(key, default(TValue));
            return TryGetValue(keyItem, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value)
        {
            KeyValuePair<TKey, TValue> keyItem = new KeyValuePair<TKey, TValue>(key, default(TValue));
            if(TryGetValue(keyItem, out KeyValuePair<TKey, TValue> keyAndValue))
            {
                value = keyAndValue.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }
    }
}
