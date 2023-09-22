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


using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LargeCollections;

/// <summary>
/// A slim readonly seekable wrapper for <see cref="Stream"/> APIs for <see cref="IReadOnlyLargeArray{byte}"/>.
/// </summary>
[DebuggerDisplay("LargeReadableMemoryStream: Position = {Position}, Length = {Length}")]
public class LargeReadableMemoryStream : Stream
{
    public IReadOnlyLargeArray<byte> Source { get; set; }


    public override bool CanRead
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => true;
    }

    public override bool CanSeek
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => true;
    }

    public override bool CanWrite
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }

    public override long Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Source != null ? Source.Count : 0L;
    }


    public override long Position
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    }

    public LargeReadableMemoryStream(IReadOnlyLargeArray<byte> source)
    {
        Source = source;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Flush()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int Read(byte[] target, int offset, int count)
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }
        StorageExtensions.CheckRange(offset, count, target.Length);

        Span<byte> targetSpan = target.AsSpan(offset, count);
        int result = Read(targetSpan);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int Read(Span<byte> target)
    {
        long maxReadableCount = Length - Position;
        if (maxReadableCount > int.MaxValue)
        {
            maxReadableCount = int.MaxValue;
        }
        if (target.Length < maxReadableCount)
        {
            maxReadableCount = target.Length;
        }

        Source.CopyTo(target, Position, (int)maxReadableCount);
        Position += maxReadableCount;

        return (int)maxReadableCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override long Seek(long offset, SeekOrigin origin)
    {
        Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => Length + offset,
            _ => Position,
        };
        return Position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
}
