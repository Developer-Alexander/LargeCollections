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

using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LargeCollections;

/// <summary>
/// A mutable set of <typeparamref name="T"/> that can store up to <see cref="Constants.MaxLargeCollectionCount"/> elements.
/// Sets are hash based.
/// </summary>
[DebuggerDisplay("LargeSet: Count = {Count}")]
public class LargeSet<T> : ILargeCollection<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DefaultEqualsFunction(T left, T right)
    {
        bool result = (left == null && right == null) || (left != null && left.Equals(right));
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DefaultHashCodeFunction(T item)
    {
        int result = item == null ? 0 : item.GetHashCode();
        return result;
    }

    protected LargeArray<SetElement> _Storage = null;

    public Func<T, T, bool> EqualsFunction
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected set;
    }


    public static Func<T, int> HashCodeFunction
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected set;
    }

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
        get => _Storage.Count;
    }

    public readonly double MinLoadFactor;
    public readonly double MaxLoadFactor;

    public readonly double MinLoadFactorTolerance;

    public double LoadFactor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (double)Count / (double)Capacity;
    }

    public LargeSet(Func<T, T, bool> equalsFunction = null,
        Func<T, int> hashCodeFunction = null,
        long capacity = 1L,
        double capacityGrowFactor = Constants.DefaultCapacityGrowFactor,
        long fixedCapacityGrowAmount = Constants.DefaultFixedCapacityGrowAmount,
        long fixedCapacityGrowLimit = Constants.DefaultFixedCapacityGrowLimit,
        double minLoadFactor = Constants.DefaultMinLoadFactor,
        double maxLoadFactor = Constants.DefaultMaxLoadFactor,
        double minLoadFactorTolerance = Constants.DefaultMinLoadFactorTolerance)
    {
        if (capacity <= 0L || capacity > Constants.MaxLargeCollectionCount)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }
        if (capacityGrowFactor <= 1.0 || capacityGrowFactor > Constants.MaxCapacityGrowFactor)
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

        EqualsFunction = equalsFunction ?? DefaultEqualsFunction;
        HashCodeFunction = hashCodeFunction ?? DefaultHashCodeFunction;

        _Storage = new LargeArray<SetElement>(capacity);

        Count = 0L;

        CapacityGrowFactor = capacityGrowFactor;

        FixedCapacityGrowAmount = fixedCapacityGrowAmount;

        FixedCapacityGrowLimit = fixedCapacityGrowLimit;

        MinLoadFactor = minLoadFactor;

        MaxLoadFactor = maxLoadFactor;

        MinLoadFactorTolerance = minLoadFactorTolerance;
    }

    public LargeSet(IEnumerable<T> items,
        Func<T, T, bool> equalsFunction = null,
        Func<T, int> hashCodeFunction = null,
        long capacity = 1L,
        double capacityGrowFactor = Constants.DefaultCapacityGrowFactor,
        long fixedCapacityGrowAmount = Constants.DefaultFixedCapacityGrowAmount,
        long fixedCapacityGrowLimit = Constants.DefaultFixedCapacityGrowLimit,
        double minLoadFactor = Constants.DefaultMinLoadFactor,
        double maxLoadFactor = Constants.DefaultMaxLoadFactor,
        double minLoadFactorTolerance = Constants.DefaultMinLoadFactorTolerance)

        : this(equalsFunction,
            hashCodeFunction,
            capacity,
            capacityGrowFactor,
            fixedCapacityGrowAmount,
            fixedCapacityGrowLimit,
            minLoadFactor,
            maxLoadFactor,
            minLoadFactorTolerance)
    {
        Add(items);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void AddToStorageInternal(T item, LargeArray<SetElement> storage, ref long count, Func<T, T, bool> equalsFunction, Func<T, int> hashCodeFunction)
    {
        long bucketIndex = GetBucketIndexInternal(item, storage.Count, hashCodeFunction);

        SetElement element = storage[bucketIndex];

        if (element == null)
        {
            storage[bucketIndex] = new SetElement(item);
            count++;
            return;
        }

        while (element != null)
        {
            if (equalsFunction.Invoke(item, element.Item))
            {
                element.Item = item;
                return;
            }

            if (element.NextElement == null)
            {
                element.NextElement = new SetElement(item);
                count++;
                return;
            }

            element = element.NextElement;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        if (Count >= Constants.MaxLargeCollectionCount)
        {
            throw new InvalidOperationException($"Can not store more than {Constants.MaxLargeCollectionCount} items.");
        }
        long count = Count;
        AddToStorageInternal(item, _Storage, ref count, EqualsFunction, HashCodeFunction);
        Count = count;

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
        long bucketIndex = GetBucketIndexInternal(item, Capacity, HashCodeFunction);

        SetElement element = _Storage[bucketIndex];
        SetElement previousElement = null;

        while (element != null)
        {
            if (EqualsFunction.Invoke(item, element.Item))
            {
                element.Item = default;

                // Is it the first and only element?
                if (previousElement == null && element.NextElement == null)
                {
                    _Storage[bucketIndex] = null;
                }
                // Is it the first but one of many elements?
                else if (previousElement == null && element.NextElement != null)
                {
                    _Storage[bucketIndex] = element.NextElement;
                }
                // Is is one of many elements but not the first one?
                else
                {
                    previousElement.NextElement = element.NextElement;
                }

                Count--;

                Shrink();
                return;
            }

            previousElement = element;
            element = element.NextElement;
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
            SetElement element = _Storage[i];

            while (element != null)
            {
                element.Item = default;

                SetElement nextElement = element.NextElement;
                element.NextElement = null;
                element = nextElement;
            }

            _Storage[i] = null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(T item, out T value)
    {
        long bucketIndex = GetBucketIndexInternal(item, Capacity, HashCodeFunction);

        SetElement element = _Storage[bucketIndex];

        while (element != null)
        {
            if (EqualsFunction.Invoke(item, element.Item))
            {
                value = element.Item;
                return true;
            }
            element = element.NextElement;
        }

        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool Contains(T item)
    {
        return TryGetValue(item, out _);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<T> GetAll()
    {
        long capacity = Capacity;

        for (long i = 0L; i < capacity; i++)
        {
            SetElement element = _Storage[i];

            while (element != null)
            {
                yield return element.Item;
                element = element.NextElement;
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
        if (capacity >= uint.MaxValue)
        {
            return;
        }

        long newCapacity = StorageExtensions.GetGrownCapacity(capacity, CapacityGrowFactor, FixedCapacityGrowAmount, FixedCapacityGrowLimit);

        // As long as the used hash value only uses 32 bit it does not make sence to use more than 2^32-1 buckets
        if (newCapacity >= uint.MaxValue)
        {
            newCapacity = uint.MaxValue;
        }

        LargeArray<SetElement> newStorage = new(newCapacity);
        long newStorageCount = 0L;
        CopyStorageInternal(_Storage, newStorage, ref newStorageCount, EqualsFunction, HashCodeFunction, true);

        _Storage = newStorage;
        Count = newStorageCount;
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

        LargeArray<SetElement> newStorage = new(newCapacity);
        long newStorageCount = 0L;
        CopyStorageInternal(_Storage, newStorage, ref newStorageCount, EqualsFunction, HashCodeFunction, true);

        _Storage = newStorage;
        Count = newStorageCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static void CopyStorageInternal(LargeArray<SetElement> sourceStorage, LargeArray<SetElement> targetStorage, ref long targetCount, Func<T, T, bool> equalsFunction, Func<T, int> hashCodeFunction, bool clearSourceStorage)
    {
        long capacity = sourceStorage.Count;
        for (long i = 0L; i < capacity; i++)
        {
            SetElement element = sourceStorage[i];

            while (element != null)
            {
                AddToStorageInternal(element.Item, targetStorage, ref targetCount, equalsFunction, hashCodeFunction);

                SetElement nextElement = element.NextElement;
                if (clearSourceStorage)
                {
                    element.NextElement = null;
                }
                element = nextElement;
            }

            if (clearSourceStorage)
            {
                sourceStorage[i] = null;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static long GetBucketIndexInternal(T item, long capacity, Func<T, int> hashCodeFunction)
    {
        ulong hash = unchecked((uint)hashCodeFunction.Invoke(item));
        long bucketIndex = (long)(hash % (ulong)capacity);

        return bucketIndex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoForEach(Action<T> action)
    {
        long capacity = Capacity;

        for (long i = 0L; i < capacity; i++)
        {
            SetElement element = _Storage[i];

            while (element != null)
            {
                T item = element.Item;
                action(item);
                element = element.NextElement;
            }
        }
    }

    [DebuggerDisplay("Item = {Item}")]
    protected class SetElement
    {
        public T Item;
        public SetElement NextElement;

        public SetElement(T item, SetElement nextElement)
        {
            Item = item;
            NextElement = nextElement;
        }

        public SetElement() : this(default, null) { }

        public SetElement(T item) : this(item, null) { }
    }
}
