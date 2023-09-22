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


using NUnit.Framework;

namespace LargeCollections.Test;

public class LargeWritableMemoryStreamTest
{
    [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
    public void Create(long capacity, long offset)
    {
        long count = capacity - 2L * offset;
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }
        if (count < 0L || offset + count > capacity)
        {
            return;
        }

        LargeWritableMemoryStream stream = new();
        Assert.AreEqual(false, stream.CanRead);
        Assert.AreEqual(true, stream.CanWrite);
        Assert.AreEqual(false, stream.CanSeek);
        Assert.AreEqual(0L, stream.Position);
        Assert.AreEqual(0L, stream.Length);
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
    public void Write(long capacity, long offset)
    {
        long count = capacity - 2L * offset;
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }
        if (count < 0L || offset + count > capacity)
        {
            return;
        }

        byte[] buffer = LargeEnumerable.Range(0, capacity).Select(x => (byte)x).ToArray();

        LargeWritableMemoryStream stream = new();
        stream.Write(buffer);
        Assert.AreEqual(capacity, stream.Position);
        Assert.AreEqual(capacity, stream.Length);
        CollectionAssert.AreEqual(buffer, stream.Storage);

        stream = new();
        stream.Write(buffer, (int)offset, (int)count);
        Assert.AreEqual(count, stream.Position);
        Assert.AreEqual(count, stream.Length);
        CollectionAssert.AreEqual(buffer.SkipTake(offset, count), stream.Storage);

        stream = new();
        stream.Write(buffer.AsSpan((int)offset, (int)count));
        Assert.AreEqual(count, stream.Position);
        Assert.AreEqual(count, stream.Length);
        CollectionAssert.AreEqual(buffer.SkipTake(offset, count), stream.Storage);

        stream = new();
        stream.Write(buffer.ToLargeList(), offset, count);
        Assert.AreEqual(count, stream.Position);
        Assert.AreEqual(count, stream.Length);
        CollectionAssert.AreEqual(buffer.SkipTake(offset, count), stream.Storage);

        stream = new();
        stream.Write(buffer.ToLargeList(), offset);
        Assert.AreEqual(count + offset, stream.Position);
        Assert.AreEqual(count + offset, stream.Length);
        CollectionAssert.AreEqual(buffer.SkipTake(offset, count + offset), stream.Storage);

        MemoryStream memoryStream = new();
        memoryStream.Write(buffer.ToLargeList(), offset);
        Assert.AreEqual(count + offset, memoryStream.Position);
        Assert.AreEqual(count + offset, memoryStream.Length);
        CollectionAssert.AreEqual(buffer.SkipTake(offset, count + offset), memoryStream.ToArray());
    }

    [TestCase]
    public void Read()
    {
        LargeWritableMemoryStream stream = new();
        Assert.Throws<NotSupportedException>(() => stream.Read(new byte[1], 0, 1));
    }

    [TestCase]
    public void Seek()
    {
        LargeWritableMemoryStream stream = new();
        Assert.Throws<NotSupportedException>(() => stream.Seek(1L, SeekOrigin.Begin));
    }

    [TestCase]
    public void SetLength()
    {
        LargeWritableMemoryStream stream = new();
        Assert.Throws<NotSupportedException>(() => stream.SetLength(1L));
    }
    /*
    [TestCase]
    public void Read()
    {
        int count = 16;
        byte[] buffer = LargeEnumerable.Range(0, count).Select(x => (byte)x).ToArray();
        byte[] readBuffer = new byte[count];

        LargeMemoryStream stream = new LargeMemoryStream(buffer);

        Assert.AreEqual(buffer.LongLength, stream.ReadableCount);

        CollectionAssert.AreEqual(buffer, stream.AsEnumerable());

        stream.Position = 0L;

        readBuffer = new byte[count];
        int i = 0;
        int currentByte = 0;
        while ((currentByte = stream.ReadByte()) >= 0)
        {
            readBuffer[i++] = (byte)currentByte;
        }
        Assert.AreEqual(i, count);
        CollectionAssert.AreEqual(buffer, readBuffer);

        stream.Position = 0L;

        readBuffer = new byte[count];
        int readCount = stream.Read(readBuffer, 0, count);
        Assert.AreEqual(count, readCount);
        CollectionAssert.AreEqual(buffer, readBuffer);

        stream.Position = 0L;

        readBuffer = new byte[count + 1];
        readCount = stream.Read(readBuffer, 0, count + 1);
        Assert.AreEqual(count, readCount);
        CollectionAssert.AreEqual(buffer, readBuffer.Take(count));

        stream.Position = 0L;

        readBuffer = new byte[count];
        readCount = stream.Read(readBuffer, 0, count - 1);
        Assert.AreEqual(count - 1, readCount);
        CollectionAssert.AreEqual(buffer.Take(count - 1), readBuffer.Take(count - 1));

        stream.Position = 0L;

        readBuffer = new byte[count];
        Assert.Throws<ArgumentException>(() => readCount = stream.Read(readBuffer, 1, count));
        Assert.Throws<ArgumentException>(() => readCount = stream.Read(readBuffer, 0, count + 1));
        Assert.Throws<ArgumentException>(() => readCount = stream.Read(readBuffer, -1, count));

        readBuffer = null;
        Assert.Throws<ArgumentNullException>(() => readCount = stream.Read(readBuffer, 0, count));

        stream.Position = 1L;

        readBuffer = new byte[count - 2];
        readCount = stream.Read(readBuffer, 0, count - 2);
        Assert.AreEqual(count - 2, readCount);
        CollectionAssert.AreEqual(buffer.Skip(1).Take(count - 2), readBuffer);

        stream.Position = 1L;

        readBuffer = new byte[count];
        readCount = stream.Read(readBuffer, 1, count - 2);
        Assert.AreEqual(count - 2, readCount);
        CollectionAssert.AreEqual(buffer.Skip(1).Take(count - 2), readBuffer.Skip(1).Take(count - 2));

    }

    [TestCase]
    public void Seek()
    {
        int count = 16;
        byte[] buffer = LargeEnumerable.Range(0, count).Select(x => (byte)x).ToArray();
        byte[] readBuffer = new byte[count];

        LargeMemoryStream stream = new LargeMemoryStream(buffer);

        stream.Seek(0L, SeekOrigin.Begin);
        Assert.AreEqual(0L, stream.Position);

        stream.Seek(1L, SeekOrigin.Begin);
        Assert.AreEqual(1L, stream.Position);

        stream.Seek(count - 1, SeekOrigin.Begin);
        Assert.AreEqual(count - 1, stream.Position);

        stream.Seek(count, SeekOrigin.Begin);
        Assert.AreEqual(count, stream.Position);

        stream.Seek(0L, SeekOrigin.Begin);
        Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(count + 1, SeekOrigin.Begin));
        Assert.AreEqual(0L, stream.Position);

        stream.Seek(0L, SeekOrigin.Begin);
        Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(-1L, SeekOrigin.Begin));
        Assert.AreEqual(0L, stream.Position);

        stream.Seek(0L, SeekOrigin.End);
        Assert.AreEqual(count, stream.Position);

        stream.Seek(-count, SeekOrigin.End);
        Assert.AreEqual(0L, stream.Position);

        stream.Seek(0L, SeekOrigin.Begin);
        Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(1L, SeekOrigin.End));
        Assert.AreEqual(0L, stream.Position);

        stream.Seek(0L, SeekOrigin.Begin);
        Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(-count - 1L, SeekOrigin.End));
        Assert.AreEqual(0L, stream.Position);

        stream.Seek(0L, SeekOrigin.Begin);
        stream.Seek(1L, SeekOrigin.Current);
        Assert.AreEqual(1L, stream.Position);

        stream.Seek(0L, SeekOrigin.Begin);
        stream.Seek(count, SeekOrigin.Current);
        Assert.AreEqual(count, stream.Position);
    }

    [TestCase]
    public void Write()
    {
        int count = 16;
        byte[] buffer = LargeEnumerable.Range(0, count).Select(x => (byte)x).ToArray();

        LargeMemoryStream stream = new LargeMemoryStream();

        stream.Write(buffer);
        Assert.AreEqual(count, stream.Position);
        Assert.AreEqual(count, stream.Length);
        stream.Position = 0L;
        CollectionAssert.AreEqual(buffer, stream.AsEnumerable());

        stream.Write(buffer);
        Assert.AreEqual(2 * count, stream.Position);
        Assert.AreEqual(2 * count, stream.Length);
        stream.Position = 0L;
        CollectionAssert.AreEqual(buffer.Concat(buffer), stream.AsEnumerable());

        stream = new LargeMemoryStream();

        stream.Write(buffer.ToLargeList());
        Assert.AreEqual(count, stream.Position);
        Assert.AreEqual(count, stream.Length);
        stream.Position = 0L;
        CollectionAssert.AreEqual(buffer.ToLargeList(), stream.AsEnumerable());

        stream = new LargeMemoryStream();

        stream.Write(buffer.ToLargeList(), 1L, count - 2);
        Assert.AreEqual(count - 2, stream.Position);
        Assert.AreEqual(count - 2, stream.Length);
        stream.Position = 0L;
        CollectionAssert.AreEqual(buffer.ToLargeList().Skip(1).Take(count - 2), stream.AsEnumerable());

        Assert.Throws<ArgumentException>(() => stream.Write(buffer, 1, count));
        Assert.Throws<ArgumentException>(() => stream.Read(buffer, 0, count + 1));
        Assert.Throws<ArgumentException>(() => stream.Read(buffer, -1, count));

        Assert.Throws<ArgumentException>(() => stream.Write(buffer.ToLargeList(), 1, count));
        Assert.Throws<ArgumentException>(() => stream.Read(buffer.ToLargeList(), 0, count + 1));
        Assert.Throws<ArgumentException>(() => stream.Read(buffer.ToLargeList(), -1, count));
    }*/
}
