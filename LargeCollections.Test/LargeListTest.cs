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

public class LargeListTest
{
    [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesTestCasesArguments))]
    public void Create(long capacity)
    {
        LargeList<long> largeList;
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => largeList = new LargeList<long>(capacity));
            return;
        }

        largeList = new LargeList<long>(capacity);
        Assert.AreEqual(0L, largeList.Count);
        Assert.AreEqual(capacity, largeList.Capacity);
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesTestCasesArguments))]
    public void AddRemoveClear(long capacity)
    {
        if (capacity < 0L || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        LargeList<long> largeList = new(capacity);
        for (long i = 0; i < capacity; i++)
        {
            largeList.Add(i);
            Assert.AreEqual(i + 1L, largeList.Count);
            Assert.AreEqual(i, largeList[i]);
        }

        for (long i = 0; i < capacity; i++)
        {
            if (i % 2 == 0)
            {
                largeList.RemoveAt(largeList.Count - 1L);
            }
            else
            {
                largeList.RemoveAt(0L);
            }

            long expectedValue = capacity - 1L - i;
            Assert.AreEqual(expectedValue, largeList.Count);
        }

        largeList.Add(LargeEnumerable.Range(capacity));
        Assert.AreEqual(capacity, largeList.Count);

        // verify ascending order
        for (long i = 0; i < capacity; i++)
        {
            long expectedValue = i;
            Assert.AreEqual(expectedValue, largeList[i]);
        }

        largeList.Clear();
        Assert.AreEqual(0L, largeList.Count);

        largeList.Shrink();
        Assert.AreEqual(0L, largeList.Capacity);

        largeList.EnsureRemainingCapacity(1L);
        Assert.GreaterOrEqual(1L, largeList.Capacity);



        long[] itemsArray = LargeEnumerable.Range(capacity).ToArray();
        largeList.Add(itemsArray.AsSpan());

        // verify ascending order
        for (long i = 0; i < capacity; i++)
        {
            long expectedValue = i;
            Assert.AreEqual(expectedValue, largeList[i]);
        }
        Assert.AreEqual(capacity, largeList.Count);

    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
    public void DoForEach(long capacity, long offset)
    {
        // input check
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        LargeArray<long> largeArray = new(capacity);

        DoForEachTest(largeArray, offset);
    }

    public static void DoForEachTest(ILargeArray<long> largeArray, long offset)
    {
        long capacity = largeArray.Count;
        long count = capacity - 2L * offset;

        // offset must not be less than 0
        // enumerable needs to be enumerated to throw the exception
        Assert.Throws<ArgumentException>(() => largeArray.GetAll(-1L, count).FirstOrDefault());

        // count must not be less than 0
        // enumerable needs to be enumerated to throw the exception
        Assert.Throws<ArgumentException>(() => largeArray.GetAll(0L, -1L).FirstOrDefault());

        // range must not exceed
        // enumerable needs to be enumerated to throw the exception
        Assert.Throws<ArgumentException>(() => largeArray.GetAll(1L, capacity).FirstOrDefault());

        if (count < 0L || offset + count > capacity)
        {
            return;
        }

        long currentValue = 0L;
        largeArray.DoForEach((ref long i) =>
        {
            i = currentValue++;
        });

        CollectionAssert.AreEqual(LargeEnumerable.Range(capacity), largeArray);

        currentValue = 0L;
        largeArray.DoForEach((ref long item) =>
        {
            Assert.AreEqual(currentValue++, item);
        });

        currentValue = offset;
        largeArray.DoForEach((ref long item) =>
        {
            Assert.AreEqual(currentValue++, item);
        }, offset, count);
        Assert.AreEqual(offset + count, currentValue);
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
    public void SetGet(long capacity, long offset)
    {
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        LargeList<long> largeList = new(capacity);
        largeList.Add(LargeEnumerable.Range(capacity));

        LargeArrayTest.SetGetTest(largeList, offset);
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
    public void Enumeration(long capacity, long offset)
    {
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        LargeList<long> largeList = new(capacity);
        largeList.Add(LargeEnumerable.Range(capacity));

        LargeArrayTest.EnumerationTest(largeList, offset);
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
    public void Sort(long capacity, long offset)
    {
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        LargeList<long> largeList = new(capacity);
        largeList.Add(LargeEnumerable.Range(capacity));

        LargeArrayTest.SortTest(largeList, offset);
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
    public void BinarySearch(long capacity, long offset)
    {
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        LargeList<long> largeList = new(capacity);
        largeList.Add(LargeEnumerable.Range(capacity));

        LargeArrayTest.BinarySearchTest(largeList, offset);
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
    public void Contains(long capacity, long offset)
    {
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        LargeList<long> largeList = new(capacity);
        largeList.Add(LargeEnumerable.Range(capacity));

        LargeArrayTest.ContainsTest(largeList, offset);
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
    public void Copy(long capacity, long offset)
    {
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        LargeList<long> largeList = new(capacity);
        largeList.Add(LargeEnumerable.Range(capacity));

        LargeArrayTest.CopyTest(largeList, offset, capacity => new LargeList<long>(LargeEnumerable.Repeat(1L, capacity), capacity));
    }
}
