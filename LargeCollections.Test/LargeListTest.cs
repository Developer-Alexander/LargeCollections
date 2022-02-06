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

namespace LargeCollections.Test
{
    public class LargeListTest
    {
        [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesTestCasesArguments))]
        public void Create(long capacity)
        {
            LargeList<long> largeList;
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => largeList = new(capacity));
                return;
            }

            largeList = new(capacity);
            Assert.AreEqual(0L, largeList.Count);
            Assert.AreEqual(capacity, largeList.Capacity);
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesTestCasesArguments))]
        public void AddSetGetContainsRemove(long capacity)
        {
            if (capacity < 0L || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
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

            if (capacity == 0L)
            {
                Assert.IsFalse(largeList.Contains(0));
                Assert.IsFalse(largeList.Contains(capacity - 1L));
                Assert.IsFalse(largeList.Contains(capacity / 2L));
            }
            else
            {
                Assert.IsTrue(largeList.Contains(0));
                Assert.IsTrue(largeList.Contains(capacity - 1L));
                Assert.IsTrue(largeList.Contains(capacity / 2L));
            }

            for (long i = 0; i < capacity; i++)
            {
                Assert.AreEqual(i, largeList[i]);
                largeList[i] = i + 1L;
                Assert.AreEqual(i + 1L, largeList[i]);
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

                Assert.AreEqual(capacity - (i + 1L), largeList.Count);
            }
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesTestCasesArguments))]
        public void Enumeration(long capacity)
        {
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeList<long> largeList = new(capacity);
            for (long i = 0; i < capacity; i++)
            {
                largeList.Add(i);
            }

            CollectionAssert.AreEqual(largeList, LargeEnumerable.Range(capacity));

            long currentIToCompareWith = 0L;
            largeList.DoForEach(i =>
            {
                Assert.AreEqual(i, currentIToCompareWith);
                currentIToCompareWith++;
            });
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesTestCasesArguments))]
        public void SortBinarySearch(long capacity)
        {

            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeList<long> largeList = new(capacity);
            for (long i = capacity - 1L; i >= 0L; i--)
            {
                largeList.Add(i);
            }

            long currentIToCompareWith = capacity - 1L;
            largeList.DoForEach(i =>
            {
                Assert.AreEqual(i, currentIToCompareWith);
                currentIToCompareWith--;
            });

            largeList.Sort();

            currentIToCompareWith = 0L;
            largeList.DoForEach(i =>
            {
                Assert.AreEqual(i, currentIToCompareWith);
                currentIToCompareWith++;
            });

            for (long i = 0; i < capacity; i++)
            {
                long index = largeList.BinarySearch(i);
                Assert.AreEqual(i, index);
            }
        }
    }
}
