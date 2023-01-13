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
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LargeCollections.Test
{
    public class LargeArrayTest
    {
        private static long[] _capacities = new long[]
        {
            0L,
            5L,
            10L,
            30L,

            /* Running tests with following capacities requires a lot of time and memory */

            //LargeCollectionsConstants.MaxStandardArrayCapacity / 2L,
            //LargeCollectionsConstants.MaxStandardArrayCapacity,
            //2L * LargeCollectionsConstants.MaxStandardArrayCapacity,
            //3L * LargeCollectionsConstants.MaxStandardArrayCapacity
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
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
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
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeArray<long> largeArray = new LargeArray<long>(capacity);

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
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeArray<long> largeArray = new LargeArray<long>(capacity);

            EnumerationTest(largeArray, offset);
        }

        public static void EnumerationTest(ILargeArray<long> largeArray, long offset)
        {
            long capacity = largeArray.Count;
            long count = capacity - 2L * offset;

            // offset must not be less than 0
            Assert.Throws<ArgumentException>(() => largeArray.GetAll(-1L, count).FirstOrDefault());

            // count must not be less than 0
            Assert.Throws<ArgumentException>(() => largeArray.GetAll(0L, -1L).FirstOrDefault());

            // range must not exceed
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

            // offset must not be less than 0
            // enumerable needs to be enumerated to throw the exception
            Assert.Throws<ArgumentException>(() => largeArray.GetAll(-1L, count).FirstOrDefault());

            // count must not be less than 0
            // enumerable needs to be enumerated to throw the exception
            Assert.Throws<ArgumentException>(() => largeArray.GetAll(0L, -1L).FirstOrDefault());

            // range must not exceed
            // enumerable needs to be enumerated to throw the exception
            Assert.Throws<ArgumentException>(() => largeArray.GetAll(1L, capacity).FirstOrDefault());
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesTestCasesArguments))]
        public void Resize(long capacity)
        {
            // input check
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeArray<long> largeArray = new LargeArray<long>(capacity);

            // create array with ascending order
            for (long i = 0; i < capacity; i++)
            {
                largeArray[i] = i;
            }

            long newLargerCapacity = capacity * 2;
            long newSmallerCapacity = capacity / 2;

            if (newLargerCapacity <= LargeCollectionsConstants.MaxLargeCollectionCount)
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
            Assert.Throws<ArgumentOutOfRangeException>(() => largeArray.Resize(LargeCollectionsConstants.MaxLargeCollectionCount + 1L));
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesWithOffsetTestCasesArguments))]
        public void Sort(long capacity, long offset)
        {
            // input check
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeArray<long> largeArray = new LargeArray<long>(capacity);

            SortTest(largeArray, offset);
        }

        public static void SortTest(ILargeArray<long> largeArray, long offset)
        {
            long capacity = largeArray.Count;
            long count = capacity - 2L * offset;
            Comparer<long> comparer = Comparer<long>.Default;

            // offset must not be less than 0
            Assert.Throws<ArgumentException>(() => largeArray.Sort(-1L, count, comparer));

            // count must not be less than 0
            Assert.Throws<ArgumentException>(() => largeArray.Sort(0L, -1L, comparer));

            // range must not exceed
            Assert.Throws<ArgumentException>(() => largeArray.Sort(1L, capacity, comparer));

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
            largeArray.Sort(offset, count, comparer);

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
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeArray<long> largeArray = new LargeArray<long>(capacity);

            BinarySearchTest(largeArray, offset);
        }

        public static void BinarySearchTest(ILargeArray<long> largeArray, long offset)
        {
            long capacity = largeArray.Count;
            long count = capacity - 2L * offset;
            Comparer<long> comparer = Comparer<long>.Default;

            // offset must not be less than 0
            Assert.Throws<ArgumentException>(() => largeArray.BinarySearch(0L, -1L, count, comparer));

            // count must not be less than 0
            Assert.Throws<ArgumentException>(() => largeArray.BinarySearch(0L, 0L, -1L, comparer));

            // range must not exceed
            Assert.Throws<ArgumentException>(() => largeArray.BinarySearch(0L, 1L, capacity, comparer));

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
                long index = largeArray.BinarySearch(i, offset, count, comparer);

                long expectedValue = -1L;
                Assert.AreEqual(expectedValue, index);
            }
            for (long i = offset; i < offset + count; i++)
            {
                long index = largeArray.BinarySearch(i, offset, count, comparer);

                long expectedValue = i;
                Assert.AreEqual(expectedValue, index);
            }
            for (long i = offset + count; i < capacity; i++)
            {
                long index = largeArray.BinarySearch(i, offset, count, comparer);

                long expectedValue = -1L;
                Assert.AreEqual(expectedValue, index);
            }

            // capacity must not be contained
            Assert.AreEqual(-1, largeArray.BinarySearch(capacity, comparer));

            // offset must not be less than 0
            Assert.Throws<ArgumentException>(() => largeArray.BinarySearch(0L, -1L, count, comparer));

            // count must not be less than 0
            Assert.Throws<ArgumentException>(() => largeArray.BinarySearch(0L, 0L, -1L, comparer));

            // range must not exceed
            Assert.Throws<ArgumentException>(() => largeArray.BinarySearch(0L, 1L, capacity, comparer));
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesWithOffsetTestCasesArguments))]
        public void Contains(long capacity, long offset)
        {
            // input check
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeArray<long> largeArray = new LargeArray<long>(capacity);

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

        public static void CopyTest(ILargeArray<long> largeArray, long offset)
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

            LargeArray<long> targetArray = new LargeArray<long>(capacity, 0L);
            largeArray.CopyTo(targetArray, capacity);
        }
    }
}
