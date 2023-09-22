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

namespace LargeCollections
{
    /// <summary>
    /// A mutable array of <typeparamref name="T"/> that can store up to <see cref="Constants.MaxLargeCollectionCount"/> elements.
    /// Arrays allow index based access to the elements.
    /// </summary>
    [DebuggerDisplay("LargeArray: Count = {Count}")]
    public class LargeArray<T> : ILargeArray<T>
    {
        private static readonly Comparer<T> _DefaultComparer = Comparer<T>.Default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DefaultComparer(T left, T right)
        {
            int result = _DefaultComparer.Compare(left, right);
            return result;
        }

        private T[][] _Storage;

        public long Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set;
        }

        public LargeArray(long capacity = 0L)
        {
            _Storage = StorageExtensions.StorageCreate<T>(capacity);
            Count = capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(long capacity)
        {
            if (capacity < 0L || capacity > Constants.MaxLargeCollectionCount)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if (capacity == Count)
            {
                return;
            }
            _Storage = _Storage.StorageResize(capacity);
            Count = capacity;
        }

        public T this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                StorageExtensions.CheckIndex(index, Count);
                T result = _Storage.StorageGet(index);
                return result;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                StorageExtensions.CheckIndex(index, Count);
                _Storage.StorageSet(index, value);
            }
        }

        T IReadOnlyLargeArray<T>.this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                StorageExtensions.CheckIndex(index, Count);
                T result = _Storage.StorageGet(index);
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            bool result = _Storage.Contains(item, 0L, Count, LargeSet<T>.DefaultEqualsFunction);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item, long offset, long count)
        {
            StorageExtensions.CheckRange(offset, count, Count);

            if (count == 0L)
            {
                return false;
            }

            bool result = _Storage.Contains(item, offset, count, LargeSet<T>.DefaultEqualsFunction);
            return result;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(ILargeArray<T> target, long sourceOffset, long targetOffset, long count)
        {
            StorageExtensions.CheckRange(sourceOffset, count, Count);
            StorageExtensions.CheckRange(targetOffset, count, target.Count);

            if (target is LargeArray<T> largeArrayTarget)
            {
                T[][] targetStorage = largeArrayTarget.GetStorage();
                _Storage.StorageCopyTo(targetStorage, sourceOffset, targetOffset, count);
            }
            else if (target is LargeList<T> largeListTarget)
            {
                T[][] targetStorage = largeListTarget.GetStorage();
                _Storage.StorageCopyTo(targetStorage, sourceOffset, targetOffset, count);
            }
            else
            {
                for (long i = 0L; i < count; i++)
                {
                    T item = _Storage.StorageGet(sourceOffset + i);
                    target[targetOffset + i] = item;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> target, long sourceOffset, long count)
        {
            StorageExtensions.CheckRange(sourceOffset, count, Count);

            _Storage.StorageCopyTo(target, sourceOffset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(long index)
        {
            StorageExtensions.CheckIndex(index, Count);
            T result = _Storage.StorageGet(index);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetAll()
        {
            foreach (T item in _Storage.StorageGetAll(0L, Count))
            {
                yield return item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetAll(long offset, long count)
        {
            StorageExtensions.CheckRange(offset, count, Count);

            foreach (T item in _Storage.StorageGetAll(offset, count))
            {
                yield return item;
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
            StorageExtensions.CheckIndex(index, Count);
            _Storage.StorageSet(index, item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoForEach(Action<T> action)
        {
            _Storage.StorageDoForEach((ref T item) => action.Invoke(item), 0L, Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoForEach(Action<T> action, long offset, long count)
        {
            StorageExtensions.CheckRange(offset, count, Count);
            _Storage.StorageDoForEach((ref T item) => action.Invoke(item), offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoForEach(RefAction<T> action)
        {
            _Storage.StorageDoForEach(action, 0L, Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoForEach(RefAction<T> action, long offset, long count)
        {
            StorageExtensions.CheckRange(offset, count, Count);
            _Storage.StorageDoForEach(action, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(Func<T, T, int> comparer)
        {
            comparer ??= DefaultComparer;
            _Storage.StorageSort(comparer, 0L, Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(Func<T, T, int> comparer, long offset, long count)
        {
            StorageExtensions.CheckRange(offset, count, Count);
            comparer ??= DefaultComparer;
            _Storage.StorageSort(comparer, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long BinarySearch(T item, Func<T, T, int> comparer)
        {
            comparer ??= DefaultComparer;
            long result = _Storage.StorageBinarySearch(item, comparer, 0L, Count);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long BinarySearch(T item, Func<T, T, int> comparer, long offset, long count)
        {
            StorageExtensions.CheckRange(offset, count, Count);
            comparer ??= DefaultComparer;
            long result = _Storage.StorageBinarySearch(item, comparer, offset, count);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Swap(long leftIndex, long rightIndex)
        {
            StorageExtensions.CheckIndex(leftIndex, Count);
            StorageExtensions.CheckIndex(rightIndex, Count);
            _Storage.StorageSwap(leftIndex, rightIndex);
        }

        internal T[][] GetStorage()
        {
            T[][] result = _Storage;
            return result;
        }
    }
}
