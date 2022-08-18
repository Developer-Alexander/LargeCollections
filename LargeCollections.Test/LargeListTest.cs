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
            if (capacity < 0L || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeList<long> largeList = new LargeList<long>(capacity);
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
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
        public void SetGet(long capacity, long offset)
        {
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeList<long> largeList = new LargeList<long>(capacity);
            largeList.Add(LargeEnumerable.Range(capacity));

            LargeArrayTest.SetGetTest(largeList, offset);
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
        public void Enumeration(long capacity, long offset)
        {
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeList<long> largeList = new LargeList<long>(capacity);
            largeList.Add(LargeEnumerable.Range(capacity));

            LargeArrayTest.EnumerationTest(largeList, offset);
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
        public void Sort(long capacity, long offset)
        {
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeList<long> largeList = new LargeList<long>(capacity);
            largeList.Add(LargeEnumerable.Range(capacity));

            LargeArrayTest.SortTest(largeList, offset);
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
        public void BinarySearch(long capacity, long offset)
        {
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeList<long> largeList = new LargeList<long>(capacity);
            largeList.Add(LargeEnumerable.Range(capacity));

            LargeArrayTest.BinarySearchTest(largeList, offset);
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
        public void Contains(long capacity, long offset)
        {
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeList<long> largeList = new LargeList<long>(capacity);
            largeList.Add(LargeEnumerable.Range(capacity));

            LargeArrayTest.ContainsTest(largeList, offset);
        }
    }
}
