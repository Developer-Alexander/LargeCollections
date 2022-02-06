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
using System.Collections;
using System.Linq;

namespace LargeCollections.Test
{
    public class LargeArrayTest
    {
        private static long[] _capacities = new long[]
        {
            0L,
            LargeCollectionsConstants.MaxStandardArrayCapacity / 2L,
            LargeCollectionsConstants.MaxStandardArrayCapacity,
            5L * LargeCollectionsConstants.MaxStandardArrayCapacity,
            LargeCollectionsConstants.MaxStandardArrayCapacity * LargeCollectionsConstants.MaxStandardArrayCapacity
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

        [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesTestCasesArguments))]
        public void Create(long capacity)
        {
            LargeArray<long> largeArray;
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => largeArray = new(capacity));
                return;
            }

            largeArray = new(capacity);
            Assert.AreEqual(capacity, largeArray.Count);
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesTestCasesArguments))]
        public void SetGetContains(long capacity)
        {
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeArray<long> largeArray = new(capacity);
            for (long i = 0; i < capacity; i++)
            {
                largeArray[i] = i;
            }

            for (long i = 0; i < capacity; i++)
            {
                Assert.AreEqual(i, largeArray[i]);
            }

            if (capacity == 0L)
            {
                Assert.IsFalse(largeArray.Contains(0));
                Assert.IsFalse(largeArray.Contains(capacity - 1L));
                Assert.IsFalse(largeArray.Contains(capacity / 2L));
            }
            else
            {
                Assert.IsTrue(largeArray.Contains(0));
                Assert.IsTrue(largeArray.Contains(capacity - 1L));
                Assert.IsTrue(largeArray.Contains(capacity / 2L));
            }


            long dummy = 0L;
            Assert.Throws<IndexOutOfRangeException>(() => dummy = largeArray[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => dummy = largeArray[capacity]);
            Assert.Throws<IndexOutOfRangeException>(() => dummy = largeArray[capacity + 1L]);

            Assert.IsFalse(largeArray.Contains(-1));
            Assert.IsFalse(largeArray.Contains(capacity));
            Assert.IsFalse(largeArray.Contains(capacity + 1L));
            Assert.IsFalse(largeArray.Contains(capacity * 2L));
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesTestCasesArguments))]
        public void Enumeration(long capacity)
        {
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeArray<long> largeArray = new(capacity);
            for (long i = 0; i < capacity; i++)
            {
                largeArray[i] = i;
            }

            CollectionAssert.AreEqual(largeArray, LargeEnumerable.Range(capacity));

            long currentIToCompareWith = 0L;
            largeArray.DoForEach(i =>
            {
                Assert.AreEqual(i, currentIToCompareWith);
                currentIToCompareWith++;
            });
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesTestCasesArguments))]
        public void Resize(long capacity)
        {
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeArray<long> largeArray = new(capacity);
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

        [TestCaseSource(typeof(LargeArrayTest), nameof(CapacitiesTestCasesArguments))]
        public void SortBinarySearch(long capacity)
        {
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeArray<long> largeArray = new(capacity);
            for (long i = 0; i < capacity; i++)
            {
                largeArray[i] = capacity - 1L - i;
            }

            long currentIToCompareWith = capacity - 1L;
            largeArray.DoForEach(i =>
            {
                Assert.AreEqual(i, currentIToCompareWith);
                currentIToCompareWith--;
            });

            largeArray.Sort();

            currentIToCompareWith = 0L;
            largeArray.DoForEach(i =>
            {
                Assert.AreEqual(i, currentIToCompareWith);
                currentIToCompareWith++;
            });

            for (long i = 0; i < capacity; i++)
            {
                long index = largeArray.BinarySearch(i);
                Assert.AreEqual(i, index);
            }
        }
    }
}
