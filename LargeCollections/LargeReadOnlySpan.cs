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

namespace LargeCollections;
/*
/// <summary>
/// An immutable readonly span of an <see cref="IReadOnlyLargeArray{T}"/>.
/// </summary>
[DebuggerDisplay("LargeReadOnlySpan: Offset = {_Offset}, Count = {_Count}")]
public readonly struct LargeReadOnlySpan<T> : IReadOnlyLargeArray<T>
{
    internal readonly IReadOnlyLargeArray<T> _Source;
    internal readonly long _Offset;
    internal readonly long _Count;

    public T this[long index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            long effectiveIndex = _Offset + index;
            T result = _Source.Get(effectiveIndex);
            return result;
        }
    }

    public long Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _Count;
    }

    public LargeReadOnlySpan(IReadOnlyLargeArray<T> source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        _Source = source;
        _Offset = 0L;
        _Count = source.Count;
    }

    public LargeReadOnlySpan(IReadOnlyLargeArray<T> source, long offset, long count)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        StorageExtensions.CheckRange(offset, count, source.Count);

        _Source = source;
        _Offset = offset;
        _Count = count;
    }

    public LargeReadOnlySpan(IReadOnlyLargeArray<T> source, long offset)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        long effectiveCount = source.Count - offset;
        StorageExtensions.CheckRange(offset, effectiveCount, source.Count);

        _Source = source;
        _Offset = offset;
        _Count = effectiveCount;
    }

    public LargeReadOnlySpan(LargeReadOnlySpan<T> source, long offset, long count)
    {
        long effectiveOffset = source._Offset + offset;
        long effectiveCount = source._Count + count;

        StorageExtensions.CheckRange(effectiveOffset, effectiveCount, source._Source.Count);

        _Source = source._Source;
        _Offset = effectiveOffset;
        _Count = effectiveCount;
    }

    public LargeReadOnlySpan(LargeReadOnlySpan<T> source, long offset)
    {
        long effectiveOffset = source._Offset + offset;
        long effectiveCount = source._Count - offset;

        StorageExtensions.CheckRange(effectiveOffset, effectiveCount, source._Source.Count);

        _Source = source._Source;
        _Offset = effectiveOffset;
        _Count = effectiveCount;
    }

    public static implicit operator LargeReadOnlySpan<T>(LargeArray<T> source)
    {
        return new(source);
    }

    public static implicit operator LargeReadOnlySpan<T>(ConcurrentLargeArray<T> source)
    {
        return new(source);
    }

    public static implicit operator LargeReadOnlySpan<T>(LargeList<T> source)
    {
        return new(source);
    }

    public static implicit operator LargeReadOnlySpan<T>(ConcurrentLargeList<T> source)
    {
        return new(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long BinarySearch(T item, Func<T, T, int> comparer)
    {
        long result = _Source.BinarySearch(item, comparer, _Offset, _Count);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long BinarySearch(T item, Func<T, T, int> comparer, long offset, long count)
    {
        long effectiveOffset = _Offset + offset;
        long effectiveCount = _Count + count;

        long result = _Source.BinarySearch(item, comparer, effectiveOffset, effectiveCount);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item, long offset, long count)
    {
        long effectiveOffset = _Offset + offset;
        long effectiveCount = _Count + count;

        bool result = _Source.Contains(item, effectiveOffset, effectiveCount);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item)
    {
        bool result = _Source.Contains(item, _Offset, _Count);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(ILargeArray<T> target, long sourceOffset, long targetOffset, long count)
    {
        long effectiveSourceOffset = _Offset + sourceOffset;

        _Source.CopyTo(target, effectiveSourceOffset, targetOffset, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(Span<T> target, long sourceOffset, long count)
    {
        long effectiveSourceOffset = _Offset + sourceOffset;

        _Source.CopyTo(target, effectiveSourceOffset, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoForEach(Action<T> action, long offset, long count)
    {
        long effectiveOffset = _Offset + offset;
        long effectiveCount = _Count + count;

        _Source.DoForEach(action, effectiveOffset, effectiveCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DoForEach(Action<T> action)
    {
        _Source.DoForEach(action, _Offset, _Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get(long index)
    {
        long effectiveIndex = _Offset + index;
        T result = _Source.Get(effectiveIndex);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<T> GetAll(long offset, long count)
    {
        long effectiveOffset = _Offset + offset;
        long effectiveCount = _Count + count;

        foreach (T item in _Source.GetAll(effectiveOffset, effectiveCount))
        {
            yield return item;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<T> GetAll()
    {
        foreach (T item in _Source.GetAll(_Offset, _Count))
        {
            yield return item;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<T> GetEnumerator()
    {
        IEnumerator<T> result = GetAll().GetEnumerator();
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator()
    {
        IEnumerator<T> result = GetAll().GetEnumerator();
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LargeReadOnlySpan<T> Slice(long offset)
    {
        LargeReadOnlySpan<T> result = new(this, offset);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LargeReadOnlySpan<T> Slice(long offset, long count)
    {
        LargeReadOnlySpan<T> result = new(this, offset, count);
        return result;
    }
}*/
