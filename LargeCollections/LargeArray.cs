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
using System.Reflection;
using System.Drawing;
using LargeCollections.IO;

namespace LargeCollections
{
    /// <summary>
    /// A mutable array of <typeparamref name="T"/> that can store up to <see cref="LargeCollectionsConstants.MaxLargeCollectionCount"/> elements.
    /// Arrays allow index based access to the elements.
    /// </summary>
    [DebuggerDisplay("LargeArray: Count = {Count}")]
    public class LargeArray<T> : ILargeArray<T>
    {
        protected static readonly Comparer<T> _comparer = Comparer<T>.Default;

        protected T[][] _storage;

        public long Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected set;
        }

        public LargeArray(long capacity = 0L)
        {
            if (capacity < 0L || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            long storageCount = capacity / LargeCollectionsConstants.MaxStandardArrayCapacity + 1L;
            long remainder = capacity % LargeCollectionsConstants.MaxStandardArrayCapacity;

            _storage = new T[storageCount][];

            for (long i = 0L; i < storageCount - 1L; i++)
            {
                _storage[i] = new T[LargeCollectionsConstants.MaxStandardArrayCapacity];
            }
            _storage[storageCount - 1L] = new T[remainder];

            Count = capacity;
        }

        public LargeArray(long capacity, T initValue): this(capacity)
        {
            for (long i = 0L; i < _storage.LongLength; i++)
            {
                T[] currentStorage = _storage[i];
                for (long j = 0L; j < currentStorage.LongLength; j++)
                {
                    currentStorage[j] = initValue;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(long capacity)
        {
            if (capacity < 0L || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if (capacity == Count)
            {
                return;
            }

            long storageCount = capacity / LargeCollectionsConstants.MaxStandardArrayCapacity + 1L;
            long remainder = capacity % LargeCollectionsConstants.MaxStandardArrayCapacity;

            T[][] newStorage = new T[storageCount][];

            if (capacity < Count)
            {
                for (long i = 0; i < storageCount - 1L; i++)
                {
                    newStorage[i] = _storage[i];
                }

                newStorage[storageCount - 1L] = new T[remainder];

                T[] currentNewStorage = newStorage[storageCount - 1L];
                T[] currentOldStorage = _storage[storageCount - 1L];
                for (long j = 0L; j < currentNewStorage.LongLength; j++)
                {
                    currentNewStorage[j] = currentOldStorage[j];
                }
            }
            else
            {
                for (long i = 0L; i < storageCount - 1L; i++)
                {
                    if(i < _storage.LongLength - 1L)
                    {
                        newStorage[i] = _storage[i];
                    }
                    else
                    {
                        newStorage[i] = new T[LargeCollectionsConstants.MaxStandardArrayCapacity];
                    }
                }

                newStorage[storageCount - 1L] = new T[remainder];

                T[] currentNewStorage = newStorage[_storage.LongLength - 1L];
                T[] currentOldStorage = _storage[_storage.LongLength - 1L];
                for (long j = 0L; j < currentOldStorage.LongLength; j++)
                {
                    currentNewStorage[j] = currentOldStorage[j];
                }
            }

            _storage = newStorage;
            Count = capacity;
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
        public bool Contains(T item)
        {
            return Contains(item, 0L, Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item, long offset, long count)
        {
            if (offset < 0L || count < 0L || offset + count > Count)
            {
                throw new ArgumentException("offset < 0L || count < 0L || offset + count > Count");
            }

            if(count == 0L)
            {
                return false;
            }

            long storageIndex = offset / LargeCollectionsConstants.MaxStandardArrayCapacity;
            long itemIndex = offset % LargeCollectionsConstants.MaxStandardArrayCapacity;

            long currentCount = 0L;

            for (long i = storageIndex; i < _storage.LongLength; i++)
            {
                T[] currentStorage = _storage[i];
                for (long j = itemIndex; j < currentStorage.LongLength; j++)
                {
                    if (currentCount < count)
                    {
                        if (_comparer.Compare(currentStorage[j], item) == 0)
                        {
                            return true;
                        }
                        currentCount++;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(long index)
        {
            if(index < 0L || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }

            long storageIndex = index / LargeCollectionsConstants.MaxStandardArrayCapacity;
            long itemIndex = index % LargeCollectionsConstants.MaxStandardArrayCapacity;

            return _storage[storageIndex][itemIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetAll()
        {
            for (long i = 0L; i < _storage.LongLength; i++)
            {
                T[] currentStorage = _storage[i];
                for (long j = 0L; j < currentStorage.LongLength; j++)
                {
                    yield return currentStorage[j];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetAll(long offset, long count)
        {
            if (offset < 0L || count < 0L || offset + count > Count)
            {
                throw new ArgumentException("offset < 0L || count < 0L || offset + count > Count");
            }

            if(count == 0L)
            {
                yield break;
            }

            long storageIndex = offset / LargeCollectionsConstants.MaxStandardArrayCapacity;
            long itemIndex = offset % LargeCollectionsConstants.MaxStandardArrayCapacity;

            long currentCount = 0L;

            for (long i = storageIndex; i < _storage.LongLength; i++)
            {
                T[] currentStorage = _storage[i];
                for (long j = itemIndex; j < currentStorage.LongLength; j++)
                {
                    if (currentCount < count)
                    {
                        yield return currentStorage[j];
                        currentCount++;
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(long index, T item)
        {
            if (index < 0L || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }

            long storageIndex = index / LargeCollectionsConstants.MaxStandardArrayCapacity;
            long itemIndex = index % LargeCollectionsConstants.MaxStandardArrayCapacity;

            _storage[storageIndex][itemIndex] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoForEach(Action<T> action)
        {
            DoForEach(0L, Count, action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoForEach(long offset, long count, Action<T> action)
        {
            if (action == null)
            {
                return;
            }
            if (offset < 0L || count < 0L || offset + count > Count)
            {
                throw new ArgumentException("offset < 0L || count < 0L || offset + count > Count");
            }
            if(count == 0L)
            {
                return;
            }

            long storageIndex = offset / LargeCollectionsConstants.MaxStandardArrayCapacity;
            long itemIndex = offset % LargeCollectionsConstants.MaxStandardArrayCapacity;

            long currentCount = 0L;

            for (long i = storageIndex; i < _storage.LongLength; i++)
            {
                T[] currentStorage = _storage[i];
                for (long j = itemIndex; j < currentStorage.LongLength; j++)
                {
                    if(currentCount < count)
                    {
                        action(currentStorage[j]);
                        currentCount++;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Heapify(long i, long left, long right, Comparer<T> comparer)
        {
            long maxIndex = i;
            long leftIndex = left + (2L * (i - left)) + 1L;
            long rightIndex = left + (2L * (i - left)) + 2L;

            if (leftIndex <= right && comparer.Compare(this[maxIndex], this[leftIndex]) < 0)
            {
                maxIndex = leftIndex;
            }

            if (rightIndex <= right && comparer.Compare(this[maxIndex], this[rightIndex]) < 0)
            {
                maxIndex = rightIndex;
            }

            if (maxIndex != i)
            {
                T swapItem = this[i];
                this[i] = this[maxIndex];
                this[maxIndex] = swapItem;

                Heapify(maxIndex, left, right, comparer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(Comparer<T> comparer = null)
        {
            Sort(0L, Count, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(long offset, long count, Comparer<T> comparer = null)
        {
            if (offset < 0L || count < 0L || offset + count > Count)
            {
                throw new ArgumentException("offset < 0L || count < 0L || offset + count > Count");
            }

            if (count <= 1L)
            {
                return;
            }

            Comparer<T> effectiveComparer = comparer ?? _comparer;

            long left = offset;
            long mid = (offset + count) / 2L;
            long right = (offset + count) - 1L;

            for (long i = mid; i >= left; i--)
            {
                Heapify(i, left, right, effectiveComparer);
            }

            for (long i = right; i >= left; i--)
            {
                T swapItem = this[left];
                this[left] = this[i];
                this[i] = swapItem;

                Heapify(left, left, i - 1L, effectiveComparer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long BinarySearch(T item, Comparer<T> comparer = null)
        {
            return BinarySearch(item, 0L, Count, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long BinarySearch(T item, long offset, long count, Comparer<T> comparer = null)
        {
            if (offset < 0L || count < 0L || offset + count > Count)
            {
                throw new ArgumentException("offset < 0L || count < 0L || offset + count > Count");
            }

            if(count == 0L)
            {
                return -1;
            }

            Comparer<T> effectiveComparer = comparer ?? _comparer;

            long left = offset;
            long mid = offset;
            long right = offset + count - 1L;
            int compareResult = 0;

            while (right >= left)
            {
                mid = (right + left) / 2;

                T midItem = this[mid];

                compareResult = effectiveComparer.Compare(item, midItem);

                // item == midItem
                if(compareResult == 0)
                {
                    return mid;
                }

                // item < midItem
                if (compareResult < 0)
                {
                    right = mid - 1;
                }
                else // item > midItem
                {
                    left = mid + 1;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(ILargeArray<T> target, long count, long sourceOffset = 0L, long targetOffset = 0L)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if(target is LargeArray<T> targetArray)
            {
                this.CopyTo(targetArray, count, sourceOffset, targetOffset);
            }
            else
            {
                // Use extension method
                this.CopyTo<T>(target, count, sourceOffset, targetOffset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(LargeArray<T> target, long count, long sourceOffset = 0L, long targetOffset = 0L)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            if (targetOffset < 0L || count < 0L || targetOffset + count > target.Count)
            {
                throw new ArgumentException("targetOffset < 0L || count < 0L || targetOffset + count > target.Count");
            }
            if (sourceOffset < 0L || count < 0L || sourceOffset + count > Count)
            {
                throw new ArgumentException("sourceOffset < 0L || count < 0L || sourceOffset + count > Count");
            }

            long currentCount = 0L;

            while (currentCount < count)
            {
                long currentSourceOffset = sourceOffset + currentCount;
                long currentSourceStorageIndex = currentSourceOffset / LargeCollectionsConstants.MaxStandardArrayCapacity;
                long currentSourceItemIndex = currentSourceOffset % LargeCollectionsConstants.MaxStandardArrayCapacity;
                T[] currentSourceArray = _storage[currentSourceStorageIndex];

                long currentTargetOffset = targetOffset + currentCount;
                long currentTargetStorageIndex = currentTargetOffset / LargeCollectionsConstants.MaxStandardArrayCapacity;
                long currentTargetItemIndex = currentTargetOffset % LargeCollectionsConstants.MaxStandardArrayCapacity;
                T[] currentTargetArray = target._storage[currentTargetStorageIndex];

                long bytesToCopyCount = Math.Min(currentSourceArray.LongLength - currentSourceItemIndex, currentTargetArray.LongLength - currentTargetItemIndex);
                Array.Copy(currentSourceArray, currentSourceItemIndex, currentTargetArray, currentTargetItemIndex, bytesToCopyCount);

                currentCount += bytesToCopyCount;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] target, int count, long sourceOffset = 0L, int targetOffset = 0)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            if (targetOffset < 0L || count < 0L || targetOffset + count > target.Length)
            {
                throw new ArgumentException("targetOffset < 0L || count < 0L || targetOffset + count > target.Length");
            }
            if (sourceOffset < 0L || count < 0L || sourceOffset + count > Count)
            {
                throw new ArgumentException("sourceOffset < 0L || count < 0L || sourceOffset + count > Count");
            }

            long currentCount = 0L;
            T[] currentTargetArray = target;

            while (currentCount < count)
            {
                long currentSourceOffset = sourceOffset + currentCount;
                long currentSourceStorageIndex = currentSourceOffset / LargeCollectionsConstants.MaxStandardArrayCapacity;
                long currentSourceItemIndex = currentSourceOffset % LargeCollectionsConstants.MaxStandardArrayCapacity;
                T[] currentSourceArray = _storage[currentSourceStorageIndex];

                long currentTargetOffset = targetOffset + currentCount;
                long currentTargetItemIndex = currentTargetOffset % LargeCollectionsConstants.MaxStandardArrayCapacity;

                long bytesToCopyCount = Math.Min(currentSourceArray.LongLength - currentSourceItemIndex, currentTargetArray.LongLength - currentTargetItemIndex);
                Array.Copy(currentSourceArray, currentSourceItemIndex, currentTargetArray, currentTargetItemIndex, bytesToCopyCount);

                currentCount += bytesToCopyCount;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(IReadOnlyLargeArray<T> source, long count, long targetOffset = 0L, long sourceOffset = 0L)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source is LargeArray<T> sourceArray)
            {
                this.CopyFrom(sourceArray, count, sourceOffset, targetOffset);
            }
            else
            {
                // Use extension method
                this.CopyFrom<T>(source, count, sourceOffset, targetOffset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(LargeArray<T> source, long count, long targetOffset = 0L, long sourceOffset = 0L)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source.CopyTo(this, count, sourceOffset, targetOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(T[] source, int count, long targetOffset = 0L, int sourceOffset = 0)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (targetOffset < 0L || count < 0L || targetOffset + count > Count)
            {
                throw new ArgumentException("targetOffset < 0L || count < 0L || targetOffset + count > Count");
            }
            if (sourceOffset < 0L || count < 0L || sourceOffset + count > source.Length)
            {
                throw new ArgumentException("sourceOffset < 0L || count < 0L || sourceOffset + count > source.Length");
            }

            long currentCount = 0L;
            T[] currentSourceArray = source;

            while (currentCount < count)
            {
                long currentSourceOffset = sourceOffset + currentCount;
                long currentSourceItemIndex = currentSourceOffset % LargeCollectionsConstants.MaxStandardArrayCapacity;

                long currentTargetOffset = targetOffset + currentCount;
                long currentTargetStorageIndex = currentTargetOffset / LargeCollectionsConstants.MaxStandardArrayCapacity;
                long currentTargetItemIndex = currentTargetOffset % LargeCollectionsConstants.MaxStandardArrayCapacity;
                T[] currentTargetArray = _storage[currentTargetStorageIndex];

                long bytesToCopyCount = Math.Min(currentSourceArray.LongLength - currentSourceItemIndex, currentTargetArray.LongLength - currentTargetItemIndex);
                Array.Copy(currentSourceArray, currentSourceItemIndex, currentTargetArray, currentTargetItemIndex, bytesToCopyCount);

                currentCount += bytesToCopyCount;
            }
        }
    }
}
