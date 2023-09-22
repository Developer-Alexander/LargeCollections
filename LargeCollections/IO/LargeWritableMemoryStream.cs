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
/// A slim writeonly non seekable wrapper for <see cref="Stream"/> APIs for <see cref="LargeList{byte}"/>.
/// </summary>
[DebuggerDisplay("LargeWritableMemoryStream: Length = {Length}")]
public class LargeWritableMemoryStream : Stream
{
    public LargeList<byte> Storage { get; set; }

    public override bool CanRead
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }

    public override bool CanSeek
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }

    public override bool CanWrite
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => true;
    }

    public override long Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Storage != null ? Storage.Count : 0L;
    }

    public override long Position
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Length;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => throw new NotSupportedException();
    }

    public LargeWritableMemoryStream()
    {
        Storage = new();
    }

    public LargeWritableMemoryStream(LargeList<byte> storage)
    {
        if (storage == null)
        {
            throw new ArgumentNullException(nameof(storage));
        }
        Storage = storage;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Flush()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Write(byte[] buffer, int offset, int count)
    {
        ReadOnlySpan<byte> source = buffer.AsSpan(offset, count);
        Storage.Add(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Write(ReadOnlySpan<byte> source)
    {
        Storage.Add(source);
    }
}
