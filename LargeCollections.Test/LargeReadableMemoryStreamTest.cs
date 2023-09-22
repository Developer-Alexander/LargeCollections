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

public class LargeReadableMemoryStreamTest
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

        LargeList<byte> source = LargeEnumerable.Range(capacity).Select(x => (byte)x).ToLargeList();

        LargeReadableMemoryStream stream = new(source);
        Assert.AreEqual(true, stream.CanRead);
        Assert.AreEqual(false, stream.CanWrite);
        Assert.AreEqual(true, stream.CanSeek);
        Assert.AreEqual(0L, stream.Position);
        Assert.AreEqual(source.Count, stream.Length);
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
    public void Seek(long capacity, long offset)
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

        LargeList<byte> source = LargeEnumerable.Range(capacity).Select(x => (byte)x).ToLargeList();
        LargeReadableMemoryStream stream = new(source);

        stream.Seek(0L, SeekOrigin.Begin);
        Assert.AreEqual(0L, stream.Position);

        if (capacity >= 1)
        {
            stream.Seek(1L, SeekOrigin.Current);
            Assert.AreEqual(1L, stream.Position);
        }

        stream.Seek(0L, SeekOrigin.End);
        Assert.AreEqual(capacity, stream.Position);

        stream.Seek(-capacity, SeekOrigin.End);
        Assert.AreEqual(0L, stream.Position);

    }


    [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
    public void Read(long capacity, long offset)
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

        LargeList<byte> source = LargeEnumerable.Range(capacity).Select(x => (byte)x).ToLargeList();

        LargeReadableMemoryStream stream = new(source);
        byte[] targetArray = new byte[capacity];
        stream.Read(targetArray, 0, (int)capacity);
        CollectionAssert.AreEqual(stream.Source, targetArray);

        stream = new(source);
        targetArray = new byte[capacity];
        stream.Read(targetArray, (int)offset, (int)count);
        CollectionAssert.AreEqual(stream.Source.Take(count), targetArray.SkipTake(offset, count));

        stream = new(source);
        stream.Seek(offset, SeekOrigin.Begin);
        targetArray = new byte[capacity];
        stream.Read(targetArray, 0, (int)count);
        CollectionAssert.AreEqual(stream.Source.SkipTake(offset, count), targetArray.Take(count));



        stream = new(source);
        LargeArray<byte> targetLargeArray = new(capacity);
        stream.Read(targetLargeArray, 0L, capacity);
        CollectionAssert.AreEqual(stream.Source, targetLargeArray);

        stream = new(source);
        targetLargeArray = new(capacity);
        stream.Read(targetLargeArray, offset, count);
        CollectionAssert.AreEqual(stream.Source.Take(count), targetLargeArray.SkipTake(offset, count));

        stream = new(source);
        targetLargeArray = new(capacity);
        stream.Read(targetLargeArray, offset);
        CollectionAssert.AreEqual(stream.Source.Take(count + offset), targetLargeArray.SkipTake(offset, count + offset));

        stream = new(source);
        stream.Seek(offset, SeekOrigin.Begin);
        targetLargeArray = new(capacity);
        stream.Read(targetLargeArray, 0L, count);
        CollectionAssert.AreEqual(stream.Source.SkipTake(offset, count), targetLargeArray.Take(count));


        MemoryStream memoryStream = new(source.ToArray());
        targetLargeArray = new(capacity);
        memoryStream.Read(targetLargeArray, offset, count);
        CollectionAssert.AreEqual(stream.Source.Take(count), targetLargeArray.SkipTake(offset, count));
    }

    [TestCase]
    public void Write()
    {
        LargeList<byte> source = LargeEnumerable.Range(1).Select(x => (byte)x).ToLargeList();
        LargeReadableMemoryStream stream = new(source);
        Assert.Throws<NotSupportedException>(() => stream.Write(new byte[1], 0, 1));
    }


    [TestCase]
    public void SetLength()
    {
        LargeList<byte> source = LargeEnumerable.Range(1).Select(x => (byte)x).ToLargeList();
        LargeReadableMemoryStream stream = new(source);
        Assert.Throws<NotSupportedException>(() => stream.SetLength(1L));
    }
}
