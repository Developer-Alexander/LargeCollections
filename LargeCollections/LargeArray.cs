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
    [DebuggerDisplay("Count = {Count}")]
    public class LargeArray<T> : ILargeArray<T>
    {
        protected static readonly Comparer<T> _comparer = Comparer<T>.Default;

        protected T[][] _storage;

        protected long _count;
        public long Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
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

            _count = capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(long capacity)
        {
            if (capacity < 0L || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if (capacity == _count)
            {
                return;
            }

            long storageCount = capacity / LargeCollectionsConstants.MaxStandardArrayCapacity + 1L;
            long remainder = capacity % LargeCollectionsConstants.MaxStandardArrayCapacity;

            T[][] newStorage = new T[storageCount][];

            if (capacity < _count)
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
            _count = capacity;
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
            for (long i = 0L; i < _storage.LongLength; i++)
            {
                T[] currentStorage = _storage[i];
                for (long j = 0L; j < currentStorage.LongLength; j++)
                {
                    if (_comparer.Compare(currentStorage[j], item) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(long index)
        {
            if(index < 0L || index >= _count)
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
        public IEnumerator<T> GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(long index, T item)
        {
            if (index < 0L || index >= _count)
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
            if(action == null)
            {
                return;
            }

            for (long i = 0L; i < _storage.LongLength; i++)
            {
                T[] currentStorage = _storage[i];
                for (long j = 0L; j < currentStorage.LongLength; j++)
                {
                    action(currentStorage[j]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PartialDoForEach(Action<T> action, long count = 0L)
        {
            if (action == null)
            {
                return;
            }

            if (count < 0L || count > _count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            count = count > 0 ? count : _count;
            long index = 0L;

            for (long i = 0L; i < _storage.LongLength; i++)
            {
                T[] currentStorage = _storage[i];
                for (long j = 0L; j < currentStorage.LongLength; j++)
                {
                    action(currentStorage[j]);
                    index++;
                    if(index >= count)
                    {
                        return;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Heapify(long i, long count, Comparer<T> comparer)
        {
            long maxIndex = i;
            long leftIndex = 2L * i + 1L;
            long rightIndex = 2L * i + 2L;

            if (leftIndex <= count && comparer.Compare(this[maxIndex], this[leftIndex]) < 0)
            {
                maxIndex = leftIndex;
            }

            if (rightIndex <= count && comparer.Compare(this[maxIndex], this[rightIndex]) < 0)
            {
                maxIndex = rightIndex;
            }

            if (maxIndex != i)
            {
                T swapItem = this[i];
                this[i] = this[maxIndex];
                this[maxIndex] = swapItem;

                Heapify(maxIndex, count, comparer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(Comparer<T> comparer = null)
        {
            PartialSort(comparer, _count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PartialSort(Comparer<T> comparer = null, long count = 0)
        {
            if (_count <= 1L)
            {
                return;
            }

            if (count < 0L || count > _count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            Comparer<T> effectiveComparer = comparer ?? _comparer;
            count = count > 0 ? count : _count;

            for (long i = count / 2L; i >= 0L; i--)
            {
                Heapify(i, count - 1L, effectiveComparer);
            }

            for (long i = count - 1L; i >= 0L; i--)
            {
                T swapItem = this[0];
                this[0] = this[i];
                this[i] = swapItem;

                Heapify(0, i - 1L, effectiveComparer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long BinarySearch(T item, Comparer<T> comparer = null)
        {
            return PartialBinarySearch(item, comparer, _count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long PartialBinarySearch(T item, Comparer<T> comparer = null, long count = 0)
        {
            if (count < 0L || count > _count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            Comparer<T> effectiveComparer = comparer ?? _comparer;
            count = count > 0 ? count : _count;

            long left = 0L;
            long mid = 0L;
            long right = count - 1L;
            int compareResult = 0;

            while (right >= left)
            {
                mid = (right + left) / 2;

                compareResult = effectiveComparer.Compare(item, this[mid]);

                if (compareResult < 0)
                {
                    right = mid - 1;

                }
                else if (compareResult > 0)
                {
                    left = mid + 1;
                }
                else
                {
                    return mid;
                }
            }

            return -1;
        }
    }
}
