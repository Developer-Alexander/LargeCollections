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
    /// A mutable list of <typeparamref name="T"/> that can store up to <see cref="LargeCollectionsConstants.MaxLargeCollectionCount"/> elements.
    /// Lists allow index based access to the elements.
    /// </summary>
    [DebuggerDisplay("LargeList: Count = {Count}")]
    public class LargeList<T> : ILargeList<T>
    {
        protected static readonly Comparer<T> _comparer = Comparer<T>.Default;

        protected readonly LargeArray<T> _storage;
        public double CapacityGrowFactor 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected set;
        }

        public long FixedCapacityGrowAmount 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected set;
        }

        public long FixedCapacityGrowLimit 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected set;
        }

        public long Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected set;
        }

        public long Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _storage.Count;
        }

        public LargeList(long capacity = 1L,
            double capacityGrowFactor = LargeCollectionsConstants.DefaultCapacityGrowFactor,
            long fixedCapacityGrowAmount = LargeCollectionsConstants.DefaultFixedCapacityGrowAmount,
            long fixedCapacityGrowLimit = LargeCollectionsConstants.DefaultFixedCapacityGrowLimit)
        {
            if (capacity < 0L || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }
            if(capacityGrowFactor <= 1.0 || capacityGrowFactor > LargeCollectionsConstants.MaxCapacityGrowFactor)
            {
                throw new ArgumentOutOfRangeException(nameof(capacityGrowFactor));
            }
            if (fixedCapacityGrowAmount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(fixedCapacityGrowAmount));
            }
            if (fixedCapacityGrowLimit < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(fixedCapacityGrowLimit));
            }

            _storage = new LargeArray<T>(capacity);

            Count = 0L;

            CapacityGrowFactor = capacityGrowFactor;

            FixedCapacityGrowAmount = fixedCapacityGrowAmount;

            FixedCapacityGrowLimit = fixedCapacityGrowLimit;
        }

        public LargeList(IEnumerable<T> items,
            long capacity = 1L,
            double capacityGrowFactor = LargeCollectionsConstants.DefaultCapacityGrowFactor,
            long fixedCapacityGrowAmount = LargeCollectionsConstants.DefaultFixedCapacityGrowAmount,
            long fixedCapacityGrowLimit = LargeCollectionsConstants.DefaultFixedCapacityGrowLimit)
            
            :this(capacity, capacityGrowFactor, fixedCapacityGrowAmount, fixedCapacityGrowLimit)
        {
            Add(items);
        }

        public T this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Get(index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(index, value);
        }

        T IReadOnlyLargeArray<T>.this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Get(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (Count == LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                throw new InvalidOperationException($"Can not store more than {LargeCollectionsConstants.MaxLargeCollectionCount} items.");
            }

            if (Count >= Capacity)
            {
                long newCapacity = GetGrownCapacity(Capacity, CapacityGrowFactor, FixedCapacityGrowAmount, FixedCapacityGrowLimit);

                _storage.Resize(newCapacity);
            }

            _storage[Count] = item;
            Count++;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                Add(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            for (long i = 0L; i < Count; i++)
            {
                _storage[i] = default(T);
            }

            Count = 0L;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            return _storage.Contains(item, 0L, Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item, long offset, long count)
        {
            if (offset < 0L || count < 0L || offset + count > Count)
            {
                throw new ArgumentException("offset < 0L || count < 0L || offset + count > Count");
            }

            return _storage.Contains(item, offset, count);

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(long index)
        {
            if (index < 0L || index >= Count)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            return _storage[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetAll()
        {
            for (long i = 0L; i < Count; i++)
            {
                yield return _storage[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetAll(long offset, long count)
        {
            if (offset < 0L || count < 0L || offset + count > Count)
            {
                throw new ArgumentException("offset < 0L || count < 0L || offset + count > Count");
            }

            for (long i = 0L; i < count; i++)
            {
                yield return _storage[offset + i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(T item)
        {
            long shiftCount = 0L;

            for (long i = 0L; i < Count; i++)
            {
                if (_comparer.Compare(_storage[i], item) == 0 && shiftCount == 0)
                {
                    shiftCount++;
                }
                else if(shiftCount > 0L)
                {
                    _storage[i - shiftCount] = _storage[i];
                }
            }
        }

        public void Remove(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                Remove(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(long index)
        {
            if (index < 0L || index >= Count)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            for (long i = index; i < Count - 1L; i++)
            {
                _storage[i] = _storage[i + 1L];
            }

            _storage[Count - 1L] = default(T);
            Count--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(long index, T item)
        {
            if (index < 0L || index >= Count)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            _storage[index] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Shrink()
        {
            _storage.Resize(Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetGrownCapacity(long capacity, 
            double capacityGrowFactor = LargeCollectionsConstants.DefaultCapacityGrowFactor, 
            long fixedCapacityGrowAmount = LargeCollectionsConstants.DefaultFixedCapacityGrowAmount, 
            long fixedCapacityGrowLimit = LargeCollectionsConstants.DefaultFixedCapacityGrowLimit)
        {
            long newCapacity;
            try
            {
                if (capacity >= fixedCapacityGrowLimit)
                {
                    newCapacity = capacity + fixedCapacityGrowAmount;
                    newCapacity = newCapacity <= LargeCollectionsConstants.MaxLargeCollectionCount ? newCapacity : LargeCollectionsConstants.MaxLargeCollectionCount;
                }
                else
                {
                    newCapacity = (long)(capacity * capacityGrowFactor) + 1L;
                    newCapacity = newCapacity <= LargeCollectionsConstants.MaxLargeCollectionCount ? newCapacity : LargeCollectionsConstants.MaxLargeCollectionCount;
                }
            }
            catch
            {
                newCapacity = LargeCollectionsConstants.MaxLargeCollectionCount;
            }

            return newCapacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoForEach(Action<T> action)
        {
            _storage.DoForEach(0L, Count, action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoForEach(long offset, long count, Action<T> action)
        {
            if (offset < 0L || count < 0L || offset + count > Count)
            {
                throw new ArgumentException("offset < 0L || count < 0L || offset + count > Count");
            }

            _storage.DoForEach(offset, count, action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(Comparer<T> comparer = null)
        {
            _storage.Sort(0L, Count, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(long offset, long count, Comparer<T> comparer = null)
        {
            if (offset < 0L || count < 0L || offset + count > Count)
            {
                throw new ArgumentException("offset < 0L || count < 0L || offset + count > Count");
            }

            _storage.Sort(offset, count, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long BinarySearch(T item, Comparer<T> comparer = null)
        {
            return _storage.BinarySearch(item, 0L, Count, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long BinarySearch(T item, long offset, long count, Comparer<T> comparer)
        {
            if (offset < 0L || count < 0L || offset + count > Count)
            {
                throw new ArgumentException("offset < 0L || count < 0L || offset + count > Count");
            }

            return _storage.BinarySearch(item, offset, count, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(IReadOnlyLargeArray<T> source, long count, long targetOffset = 0L, long sourceOffset = 0L)
        {
            if (targetOffset + count > Count)
            {
                throw new ArgumentException("targetOffset + count > Count");
            }

            _storage.CopyFrom(source, count, targetOffset, sourceOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(T[] source, int count, long targetOffset = 0L, int sourceOffset = 0)
        {
            if (targetOffset + count > Count)
            {
                throw new ArgumentException("targetOffset + count > Count");
            }

            _storage.CopyFrom(source, count, targetOffset, sourceOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(ILargeArray<T> target, long count, long sourceOffset = 0L, long targetOffset = 0L)
        {
            if (sourceOffset + count > Count)
            {
                throw new ArgumentException("sourceOffset + count > Count");
            }

            _storage.CopyTo(target, count, sourceOffset, targetOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] target, int count, long sourceOffset = 0L, int targetOffset = 0)
        {
            if (sourceOffset + count > Count)
            {
                throw new ArgumentException("sourceOffset + count > Count");
            }

            _storage.CopyTo(target, count, sourceOffset, targetOffset);
        }
    }
}
