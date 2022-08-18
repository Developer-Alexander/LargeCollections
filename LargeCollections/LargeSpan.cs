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
    /// An immutable segment of an <see cref="IReadOnlyLargeArray{T}"/> that behalfs as an <see cref="IReadOnlyLargeArray{T}"/> for itself.
    /// Readonly spans can be use to limit <see cref="ILargeArray{T}"/> to readonly access.
    /// Spans allow index based access to the elements. Spans are can be chained but a span of a span will be created as a span of the original collection.
    /// </summary>
    [DebuggerDisplay("ReadOnlyLargeSpan: Offset = {Offset}, Count = {Count}")]
    public class ReadOnlyLargeSpan<T> : IReadOnlyLargeArray<T>
    {
        protected static readonly Comparer<T> _comparer = Comparer<T>.Default;

        protected IReadOnlyLargeArray<T> _source
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set;
        }

        public long Offset
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set;
        }

        public long Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set;
        }

        public ReadOnlyLargeSpan(IReadOnlyLargeArray<T> array, long offset, long count)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (offset < 0L || count < 0L)
            {
                throw new ArgumentException("offset < 0L || count < 0L");
            }

            // Do not chain spans. Use the original source instead
            if (array is ReadOnlyLargeSpan<T> || array is LargeSpan<T>)
            {
                ReadOnlyLargeSpan<T> readOnlySpan = array as ReadOnlyLargeSpan<T>;
                if (readOnlySpan._source == null)
                {
                    throw new ArgumentNullException(nameof(readOnlySpan._source));
                }
                if (offset + count > readOnlySpan.Count)
                {
                    throw new ArgumentException("offset + count > readOnlySpan.Count");
                }
                if (readOnlySpan.Offset + offset + count > readOnlySpan._source.Count)
                {
                    throw new ArgumentException("readOnlySpan.Offset + offset + count > readOnlySpan.Source.Count");
                }

                _source = readOnlySpan._source;
                Offset = readOnlySpan.Offset + offset;
                Count = count;
            }
            else
            {
                if (offset + count > array.Count)
                {
                    throw new ArgumentException("offset + count > array.Count");
                }

                _source = array;
                Offset = offset;
                Count = count;
            }
        }

        public T this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Get(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long BinarySearch(T item, Comparer<T> comparer = null)
        {
            long index = _source.BinarySearch(item, Offset, Count, comparer);
            if (index < 0)
            {
                return -1;
            }

            long result = index - Offset;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long BinarySearch(T item, long offset, long count, Comparer<T> comparer = null)
        {
            if (offset < 0L || count < 0L || offset + count > Count)
            {
                throw new ArgumentException("offset < 0L || count < 0L || offset + count > Count");
            }

            long effectiveOffset = Offset + offset;
            long index = _source.BinarySearch(item, effectiveOffset, count, comparer);

            if (index < 0)
            {
                return -1;
            }

            long result = index - Offset;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            return _source.Contains(item, Offset, Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item, long offset, long count)
        {
            if (offset < 0L || count < 0L || offset + count > Count)
            {
                throw new ArgumentException("offset < 0L || count < 0L || offset + count > Count");
            }

            long effectiveOffset = Offset + offset;
            return _source.Contains(item, effectiveOffset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoForEach(Action<T> action)
        {
            _source.DoForEach(Offset, Count, action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoForEach(long offset, long count, Action<T> action)
        {
            if (offset < 0L || count < 0L || offset + count > Count)
            {
                throw new ArgumentException("offset < 0L || count < 0L || offset + count > Count");
            }

            long effectiveOffset = Offset + offset;
            _source.DoForEach(effectiveOffset, count, action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(long index)
        {

            if (index < 0L || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }

            long effectiveIndex = Offset + index;
            return _source.Get(effectiveIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetAll()
        {
            foreach (T item in _source.GetAll(Offset, Count))
            {
                yield return item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetAll(long offset, long count)
        {
            if (offset < 0L || count < 0L || offset + count > Count)
            {
                throw new ArgumentException("offset < 0L || count < 0L || offset + count > Count");
            }

            long effectiveOffset = Offset + offset;
            foreach (T item in _source.GetAll(effectiveOffset, count))
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
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }
    }

    /// <summary>
    /// A mutable segment of an <see cref="ILargeArray{T}"/> that behalfs as an <see cref="ILargeArray{T}"/> for itself.
    /// Spans allow index based access to the elements. Spans are can be chained but a span of a span will be created as a span of the original collection.
    /// </summary>
    [DebuggerDisplay("LargeSpan: Offset = {Offset}, Count = {Count}")]
    public class LargeSpan<T> : ReadOnlyLargeSpan<T>, ILargeArray<T>
    {
        protected ILargeArray<T> _sourceAsLargeArray 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source as ILargeArray<T>;
        }

        public LargeSpan(ILargeArray<T> array, long offset, long count) : base(array, offset, count)
        {
        }

        T ILargeArray<T>.this[long index] 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Get(index);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(index, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(long index, T item)
        {
            if (index < 0L || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }

            long effectiveIndex = Offset + index;
            _sourceAsLargeArray.Set(effectiveIndex, item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(Comparer<T> comparer = null)
        {
            _sourceAsLargeArray.Sort(Offset, Count, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(long offset, long count, Comparer<T> comparer = null)
        {
            if (offset < 0L || count < 0L || offset + count > Count)
            {
                throw new ArgumentException("offset < 0L || count < 0L || offset + count > Count");
            }

            long effectiveOffset = Offset + offset;
            _sourceAsLargeArray.Sort(effectiveOffset, count, comparer);
        }
    }
}
