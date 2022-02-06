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
    [DebuggerDisplay("Count = {Count}")]
    public class LargeSet<T> : ILargeCollection<T>
    {
        protected LargeArray<SetElement> _storage = null;

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

        protected Comparer<T> _comparer;

        protected static Func<T, int> _hashCodeFunction;

        protected long _count;
        public long Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        public long Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _storage.Count;
        }

        public readonly double MinLoadFactor;
        public readonly double MaxLoadFactor;

        public readonly double MinLoadFactorTolerance;

        public double LoadFactor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (double)_count / (double)Capacity;
            }
        }

        public LargeSet(Comparer<T> comparer = null,
            long capacity = 1L,
            double capacityGrowFactor = LargeCollectionsConstants.DefaultCapacityGrowFactor,
            long fixedCapacityGrowAmount = LargeCollectionsConstants.DefaultFixedCapacityGrowAmount,
            long fixedCapacityGrowLimit = LargeCollectionsConstants.DefaultFixedCapacityGrowLimit,
            double minLoadFactor = LargeCollectionsConstants.DefaultMinLoadFactor,
            double maxLoadFactor = LargeCollectionsConstants.DefaultMaxLoadFactor,
            double minLoadFactorTolerance = LargeCollectionsConstants.DefaultMinLoadFactorTolerance)
        {
            if (capacity <= 0L || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }
            if (capacityGrowFactor <= 1.0 || capacityGrowFactor > LargeCollectionsConstants.MaxCapacityGrowFactor)
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
            if (minLoadFactor < 0.0 || minLoadFactor >= maxLoadFactor)
            {
                throw new ArgumentOutOfRangeException(nameof(minLoadFactor));
            }
            if (maxLoadFactor < 0.0 || maxLoadFactor <= minLoadFactor)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLoadFactor));
            }
            if (minLoadFactorTolerance < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(minLoadFactorTolerance));
            }

            _comparer = comparer ?? Comparer<T>.Default;
            _hashCodeFunction = item => item.GetHashCode();

            _storage = new LargeArray<SetElement>(capacity);

            _count = 0L;

            CapacityGrowFactor = capacityGrowFactor;

            FixedCapacityGrowAmount = fixedCapacityGrowAmount;

            FixedCapacityGrowLimit = fixedCapacityGrowLimit;

            MinLoadFactor = minLoadFactor;

            MaxLoadFactor = maxLoadFactor;

            MinLoadFactorTolerance = minLoadFactorTolerance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void AddToStorageInternal(ref T item, LargeArray<SetElement> storage, ref long count, Comparer<T> comparer)
        {
            long bucketIndex = GetBucketIndexInternal(ref item, storage.Count);

            SetElement element = storage[bucketIndex];

            if (element == null)
            {
                storage[bucketIndex] = new SetElement(item);
                count++;
                return;
            }

            while (element != null)
            {
                if (comparer.Compare(element.Item, item) == 0)
                {
                    element.Item = item;
                    return;
                }

                if (element.Next == null)
                {
                    element.Next = new SetElement(item);
                    count++;
                    return;
                }

                element = element.Next;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (_count >= LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                throw new InvalidOperationException($"Can not store more than {LargeCollectionsConstants.MaxLargeCollectionCount} items.");
            }

            AddToStorageInternal(ref item, _storage, ref _count, _comparer);

            ExtendInternal();
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
        public void Remove(T item)
        {
            long bucketIndex = GetBucketIndexInternal(ref item, _storage.Count);

            SetElement element = _storage[bucketIndex];
            SetElement previousElement = null;

            while (element != null)
            {
                if (_comparer.Compare(element.Item, item) == 0)
                {
                    element.Item = default(T);

                    // Is it the first and only element?
                    if (previousElement == null && element.Next == null)
                    {
                        _storage[bucketIndex] = null;
                    }
                    // Is it the first but one of many elements?
                    else if (previousElement == null && element.Next != null)
                    {
                        _storage[bucketIndex] = element.Next;
                    }
                    // Is is one of many elements but not the first one?
                    else
                    {
                        previousElement.Next = element.Next;
                    }

                    _count--;

                    Shrink();
                    return;
                }

                previousElement = element;
                element = element.Next;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                Remove(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            long capacity = Capacity;

            for (long i = 0L; i < capacity; i++)
            {
                SetElement element = _storage[i];

                while (element != null)
                {
                    element.Item = default(T);

                    SetElement nextElement = element.Next;
                    element.Next = null;
                    element = nextElement;
                }

                _storage[i] = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(T item, out T value)
        {
            long bucketIndex = GetBucketIndexInternal(ref item, _storage.Count);

            SetElement element = _storage[bucketIndex];

            while (element != null)
            {
                if (_comparer.Compare(element.Item, item) == 0)
                {
                    value = element.Item;
                    return true;
                }
                element = element.Next;
            }

            value = default(T);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            return TryGetValue(item, out _);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetAll()
        {
            long capacity = Capacity;

            for (long i = 0L; i < capacity; i++)
            {
                SetElement element = _storage[i];

                while (element != null)
                {
                    yield return element.Item;
                    element = element.Next;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ExtendInternal()
        {
            if (LoadFactor <= MaxLoadFactor)
            {
                return;
            }

            long capacity = Capacity;

            // As long as the used hash value only uses 32 bit it does not make sence to use more than 2^32-1 buckets
            if (capacity >= 4_294_967_295)
            {
                return;
            }

            if (capacity == long.MaxValue)
            {
                return;
            }

            long newCapacity = LargeList<T>.GetGrownCapacity(capacity, CapacityGrowFactor, FixedCapacityGrowAmount, FixedCapacityGrowLimit);

            // As long as the used hash value only uses 32 bit it does not make sence to use more than 2^32-1 buckets
            if (newCapacity >= 4_294_967_295L)
            {
                newCapacity = 4_294_967_295L;
            }

            LargeArray<SetElement> newStorage = new LargeArray<SetElement>(newCapacity);
            long newStorageCount = 0L;
            CopyStorageInternal(_storage, newStorage, ref newStorageCount, _comparer, true);

            _storage = newStorage;
            _count = newStorageCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Shrink()
        {
            if (LoadFactor <= MinLoadFactor * MinLoadFactorTolerance)
            {
                return;
            }

            long newCapacity = (long)(Capacity * MinLoadFactor);
            newCapacity = newCapacity > 0L ? newCapacity : 1L;

            LargeArray<SetElement> newStorage = new LargeArray<SetElement>(newCapacity);
            long newStorageCount = 0L;
            CopyStorageInternal(_storage, newStorage, ref newStorageCount, _comparer, true);

            _storage = newStorage;
            _count = newStorageCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void CopyStorageInternal(LargeArray<SetElement> sourceStorage, LargeArray<SetElement> targetStorage, ref long targetCount, Comparer<T> comparer, bool clearSourceStorage)
        {
            long capacity = sourceStorage.Count;
            for (long i = 0L; i < capacity; i++)
            {
                SetElement element = sourceStorage[i];

                while (element != null)
                {
                    AddToStorageInternal(ref element.Item, targetStorage, ref targetCount, comparer);

                    SetElement nextElement = element.Next;
                    if (clearSourceStorage)
                    {
                        element.Next = null;
                    }
                    element = nextElement;
                }

                if(clearSourceStorage)
                {
                    sourceStorage[i] = null;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static long GetBucketIndexInternal(ref T item, long capacity)
        {
            ulong hash = unchecked((uint)_hashCodeFunction(item));
            long bucketIndex = (long)(hash % (ulong)capacity);

            return bucketIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoForEach(Action<T> action)
        {
            long capacity = Capacity;

            for (long i = 0L; i < capacity; i++)
            {
                SetElement element = _storage[i];

                while (element != null)
                {
                    T item = element.Item;
                    action(item);
                    element = element.Next;
                }
            }
        }

        [DebuggerDisplay("Item = {Item}")]
        protected class SetElement
        {
            public T Item;
            public SetElement Next;

            public SetElement()
            {
                Item = default;
                Next = null;
            }

            public SetElement(T item)
            {
                Item = item;
                Next = null;
            }

            public SetElement(T item, SetElement next)
            {
                Item = item;
                Next = next;
            }
        }
    }
}
