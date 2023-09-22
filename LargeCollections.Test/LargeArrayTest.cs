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
using System.Collections;

namespace LargeCollections.Test;

public class LargeArrayTest
{
    private static long[] _capacities = new long[]
    {
        0L,
        5L,
        10L,
        30L,

        /* Running tests with following capacities requires a lot of time and memory */

        //Constants.MaxStandardArrayCapacity / 2L,
        //Constants.MaxStandardArrayCapacity,
        //2L * Constants.MaxStandardArrayCapacity,
        //3L * Constants.MaxStandardArrayCapacity
    };

    private static long[] _offsets = new long[]
    {
        0L,
        1L,
        2L,
    };

    public static IEnumerable CapacitiesTestCasesArguments
    {
        get
        {
            foreach (long capacity in _capacities)
            {
                yield return new TestCaseData(capacity - 2L);
                yield return new TestCaseData(capacity - 1L);
                yield return new TestCaseData(capacity);
                yield return new TestCaseData(capacity + 1L);
                yield return new TestCaseData(capacity + 2L);
            }
        }
    }

    public static IEnumerable CapacitiesWithOffsetTestCasesArguments
    {
        get
        {
            foreach (long capacity in _capacities)
            {
                foreach (long offset in _offsets)
                {
                    yield return new TestCaseData(capacity - 2L, offset);
                    yield return new TestCaseData(capacity - 1L, offset);
                    yield return new TestCaseData(capacity, offset);
                    yield return new TestCaseData(capacity + 1L, offset);
                    yield return new TestCaseData(capacity + 2L, offset);
                }
            }
        }
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesTestCasesArguments))]
    public void Create(long capacity)
    {
        LargeArray<long> largeArray;
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => largeArray = new LargeArray<long>(capacity));
            return;
        }

        largeArray = new LargeArray<long>(capacity);
        Assert.AreEqual(capacity, largeArray.Count);
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesWithOffsetTestCasesArguments))]
    public void SetGet(long capacity, long offset)
    {
        // input check
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        LargeArray<long> largeArray = new(capacity);

        SetGetTest(largeArray, offset);
    }

    public static void SetGetTest(ILargeArray<long> largeArray, long offset)
    {
        long capacity = largeArray.Count;
        long count = capacity - 2L * offset;

        if (count < 0L || offset + count > capacity)
        {
            return;
        }

        // create and verify array with ascending order
        for (long i = 0; i < capacity; i++)
        {
            largeArray[i] = i;
            Assert.AreEqual(i, largeArray[i]);
        }

        long dummy = 0L;
        Assert.Throws<IndexOutOfRangeException>(() => dummy = largeArray[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => dummy = largeArray[capacity]);
        Assert.Throws<IndexOutOfRangeException>(() => dummy = largeArray[capacity + 1L]);
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesWithOffsetTestCasesArguments))]
    public void Enumeration(long capacity, long offset)
    {
        // input check
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        LargeArray<long> largeArray = new(capacity);

        EnumerationTest(largeArray, offset);
    }

    public static void EnumerationTest(ILargeArray<long> largeArray, long offset)
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

        // create and verify array with ascending order
        for (long i = 0; i < capacity; i++)
        {
            largeArray[i] = i;
            Assert.AreEqual(i, largeArray[i]);
        }

        // GetAll
        CollectionAssert.AreEqual(LargeEnumerable.Range(capacity), largeArray);

        // Ranged GetAll
        CollectionAssert.AreEqual(LargeEnumerable.Range(offset, offset + count), largeArray.GetAll(offset, count));

        // DoForEach
        long currentIToCompareWith = 0L;
        largeArray.DoForEach(i =>
        {
            Assert.AreEqual(currentIToCompareWith, i);
            currentIToCompareWith++;
        });
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesWithOffsetTestCasesArguments))]
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

    [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesTestCasesArguments))]
    public void Resize(long capacity)
    {
        // input check
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        LargeArray<long> largeArray = new(capacity);

        // create array with ascending order
        for (long i = 0; i < capacity; i++)
        {
            largeArray[i] = i;
        }

        long newLargerCapacity = capacity * 2;
        long newSmallerCapacity = capacity / 2;

        if (newLargerCapacity <= Constants.MaxLargeCollectionCount)
        {
            largeArray.Resize(newLargerCapacity);
            Assert.AreEqual(largeArray.Count, newLargerCapacity);
            CollectionAssert.AreEqual(largeArray, LargeEnumerable.Range(capacity).Concat(LargeEnumerable.Repeat(default(long), newLargerCapacity - capacity)));
        }
        else
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => largeArray.Resize(newLargerCapacity));
            return;
        }

        largeArray.Resize(newSmallerCapacity);
        Assert.AreEqual(largeArray.Count, newSmallerCapacity);
        CollectionAssert.AreEqual(largeArray, LargeEnumerable.Range(newSmallerCapacity));

        Assert.Throws<ArgumentOutOfRangeException>(() => largeArray.Resize(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => largeArray.Resize(Constants.MaxLargeCollectionCount + 1L));
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesWithOffsetTestCasesArguments))]
    public void Sort(long capacity, long offset)
    {
        // input check
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        LargeArray<long> largeArray = new(capacity);

        SortTest(largeArray, offset);
    }

    public static void SortTest(ILargeArray<long> largeArray, long offset)
    {
        long capacity = largeArray.Count;
        long count = capacity - 2L * offset;
        Func<long, long, int> comparer = Comparer<long>.Default.Compare;

        // offset must not be less than 0
        Assert.Throws<ArgumentException>(() => largeArray.Sort(comparer, -1L, count));

        // count must not be less than 0
        Assert.Throws<ArgumentException>(() => largeArray.Sort(comparer, 0L, -1L));

        // range must not exceed
        Assert.Throws<ArgumentException>(() => largeArray.Sort(comparer, 1L, capacity));

        if (count < 0L || offset + count > capacity)
        {
            return;
        }

        // create array with descending order
        for (long i = 0; i < capacity; i++)
        {
            long expectedValue = capacity - 1L - i;
            largeArray[i] = expectedValue;
            Assert.AreEqual(expectedValue, largeArray[i]);
        }

        // Sort
        largeArray.Sort(comparer, offset, count);

        // verify ascending order after sort
        for (long i = 0; i < offset; i++)
        {
            long expectedValue = capacity - 1L - i;
            Assert.AreEqual(expectedValue, largeArray[i]);
        }
        for (long i = offset; i < offset + count; i++)
        {
            long expectedValue = i;
            Assert.AreEqual(expectedValue, largeArray[i]);
        }
        for (long i = offset + count; i < capacity; i++)
        {
            long expectedValue = capacity - 1L - i;
            Assert.AreEqual(expectedValue, largeArray[i]);
        }
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesWithOffsetTestCasesArguments))]
    public void BinarySearch(long capacity, long offset)
    {
        // input check
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        LargeArray<long> largeArray = new(capacity);

        BinarySearchTest(largeArray, offset);
    }

    public static void BinarySearchTest(ILargeArray<long> largeArray, long offset)
    {
        long capacity = largeArray.Count;
        long count = capacity - 2L * offset;
        Func<long, long, int> comparer = Comparer<long>.Default.Compare;

        // offset must not be less than 0
        Assert.Throws<ArgumentException>(() => largeArray.BinarySearch(0L, comparer, -1L, count));

        // count must not be less than 0
        Assert.Throws<ArgumentException>(() => largeArray.BinarySearch(0L, comparer, 0L, -1L));

        // range must not exceed
        Assert.Throws<ArgumentException>(() => largeArray.BinarySearch(0L, comparer, 1L, capacity));

        if (count < 0L || offset + count > capacity)
        {
            return;
        }

        // create and verify array with ascending order
        for (long i = 0; i < capacity; i++)
        {
            largeArray[i] = i;
            Assert.AreEqual(i, largeArray[i]);
        }

        // Binary Search
        for (long i = 0; i < offset; i++)
        {
            long index = largeArray.BinarySearch(i, comparer, offset, count);

            long expectedValue = -1L;
            Assert.AreEqual(expectedValue, index);
        }
        for (long i = offset; i < offset + count; i++)
        {
            long index = largeArray.BinarySearch(i, comparer, offset, count);

            long expectedValue = i;
            Assert.AreEqual(expectedValue, index);
        }
        for (long i = offset + count; i < capacity; i++)
        {
            long index = largeArray.BinarySearch(i, comparer, offset, count);

            long expectedValue = -1L;
            Assert.AreEqual(expectedValue, index);
        }

        // capacity must not be contained
        Assert.AreEqual(-1, largeArray.BinarySearch(capacity, comparer));

        // offset must not be less than 0
        Assert.Throws<ArgumentException>(() => largeArray.BinarySearch(0L, comparer, -1L, count));

        // count must not be less than 0
        Assert.Throws<ArgumentException>(() => largeArray.BinarySearch(0L, comparer, 0L, -1L));

        // range must not exceed
        Assert.Throws<ArgumentException>(() => largeArray.BinarySearch(0L, comparer, 1L, capacity));
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesWithOffsetTestCasesArguments))]
    public void Contains(long capacity, long offset)
    {
        // input check
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        LargeArray<long> largeArray = new(capacity);

        ContainsTest(largeArray, offset);
    }

    public static void ContainsTest(ILargeArray<long> largeArray, long offset)
    {
        long capacity = largeArray.Count;
        long count = capacity - 2L * offset;

        // offset must not be less than 0
        Assert.Throws<ArgumentException>(() => largeArray.Contains(0L, -1L, count));

        // count must not be less than 0
        Assert.Throws<ArgumentException>(() => largeArray.Contains(0L, 0L, -1L));

        // range must not exceed
        Assert.Throws<ArgumentException>(() => largeArray.Contains(0L, 1L, capacity));

        if (count < 0L || offset + count > capacity)
        {
            return;
        }

        // create and verify array with ascending order
        for (long i = 0; i < capacity; i++)
        {
            largeArray[i] = i;
            Assert.AreEqual(i, largeArray[i]);
        }

        // Contains
        for (long i = 0; i < offset; i++)
        {
            bool result = largeArray.Contains(i, offset, count);
            Assert.AreEqual(false, result);
        }
        for (long i = offset; i < offset + count; i++)
        {
            bool result = largeArray.Contains(i, offset, count);
            Assert.AreEqual(true, result);
        }
        for (long i = offset + count; i < capacity; i++)
        {
            bool result = largeArray.Contains(i, offset, count);
            Assert.AreEqual(false, result);
        }

        // capacity must not be contained
        Assert.AreEqual(false, largeArray.Contains(capacity));
    }

    [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesWithOffsetTestCasesArguments))]
    public void Copy(long capacity, long offset)
    {
        // input check
        if (capacity < 0 || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        LargeArray<long> largeArray = new(capacity);

        CopyTest(largeArray, offset, capacity => new LargeArray<long>(capacity));
    }

    public static void CopyTest(ILargeArray<long> largeArray, long offset, Func<long, ILargeArray<long>> getTarget)
    {
        long capacity = largeArray.Count;
        long count = capacity - 2L * offset;

        // offset must not be less than 0
        Assert.Throws<ArgumentException>(() => largeArray.Contains(0L, -1L, count));

        // count must not be less than 0
        Assert.Throws<ArgumentException>(() => largeArray.Contains(0L, 0L, -1L));

        // range must not exceed
        Assert.Throws<ArgumentException>(() => largeArray.Contains(0L, 1L, capacity));

        if (count < 0L || offset + count > capacity)
        {
            return;
        }

        // create and verify array with ascending order
        for (long i = 0; i < capacity; i++)
        {
            largeArray[i] = i;
            Assert.AreEqual(i, largeArray[i]);
        }

        // Copy To

        LargeArray<long> targetLargeArray = new(capacity);
        largeArray.CopyTo(targetLargeArray, 0L, 0L, capacity);
        CollectionAssert.AreEqual(largeArray, targetLargeArray);

        targetLargeArray = new(capacity);
        largeArray.CopyTo(targetLargeArray, offset, 0L, count);
        CollectionAssert.AreEqual(largeArray.SkipTake(offset, count), targetLargeArray.Take(count));

        targetLargeArray = new(capacity);
        largeArray.CopyTo(targetLargeArray, 0L, offset, count);
        CollectionAssert.AreEqual(largeArray.Take(count), targetLargeArray.SkipTake(offset, count));



        LargeList<long> targetLargeList = new(capacity);
        targetLargeList.Add(LargeEnumerable.Repeat(0L, capacity));
        largeArray.CopyTo(targetLargeList, 0L, 0L, capacity);
        CollectionAssert.AreEqual(largeArray, targetLargeList);

        targetLargeList = new(capacity);
        targetLargeList.Add(LargeEnumerable.Repeat(0L, capacity));
        largeArray.CopyTo(targetLargeList, offset, 0L, count);
        CollectionAssert.AreEqual(largeArray.SkipTake(offset, count), targetLargeList.Take(count));

        targetLargeList = new(capacity);
        targetLargeList.Add(LargeEnumerable.Repeat(0L, capacity));
        largeArray.CopyTo(targetLargeList, 0L, offset, count);
        CollectionAssert.AreEqual(largeArray.Take(count), targetLargeList.SkipTake(offset, count));


        long[] targetArray = new long[capacity];
        largeArray.CopyTo(targetArray, 0L, capacity);
        CollectionAssert.AreEqual(largeArray, targetArray);

        targetArray = new long[capacity];
        largeArray.CopyTo(targetArray, offset, count);
        CollectionAssert.AreEqual(largeArray.SkipTake(offset, count), targetArray.Take(count));

        targetArray = new long[capacity];
        largeArray.CopyTo(targetArray.AsSpan((int)offset, (int)count), 0L, count);
        CollectionAssert.AreEqual(largeArray.Take(count), targetArray.SkipTake(offset, count));


        // Copy From
        long[] sourceArray = LargeEnumerable.Repeat(0L, capacity).ToArray();

        ILargeArray<long> target = getTarget(capacity);
        sourceArray.CopyTo(targetLargeArray, 0L, 0L, capacity);
        CollectionAssert.AreEqual(sourceArray, targetLargeArray);

        target = getTarget(capacity);
        sourceArray.CopyTo(targetLargeArray, offset, 0L, count);
        CollectionAssert.AreEqual(sourceArray.SkipTake(offset, count), targetLargeArray.Take(count));

        target = getTarget(capacity);
        sourceArray.CopyTo(targetLargeArray, 0L, offset, count);
        CollectionAssert.AreEqual(sourceArray.Take(count), targetLargeArray.SkipTake(offset, count));
    }
}
