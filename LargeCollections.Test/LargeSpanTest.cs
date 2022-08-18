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
    public class LargeSpanTest
    {
        [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
        public void Create(long capacity, long offset)
        {
            long count = capacity - 2L * offset;

            long offset2 = offset + offset;
            long count2 = capacity - 4L * offset;

            LargeArray<long> largeArray;
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => largeArray = new(capacity));
                return;
            }

            largeArray = new LargeArray<long>(capacity);

            // offset must not be less than 0
            Assert.Throws<ArgumentException>(() => largeArray.ToLargeSpan(-1L, count));

            // count must not be less than 0
            Assert.Throws<ArgumentException>(() => largeArray.ToLargeSpan(0L, -1L));

            // range must not exceed
            Assert.Throws<ArgumentException>(() => largeArray.ToLargeSpan(1L, capacity));

            if (count < 0L || offset + count > capacity)
            {
                return;
            }

            LargeSpan<long> span = largeArray.ToLargeSpan(offset, count);

            Assert.AreEqual(offset, span.Offset);
            Assert.AreEqual(count, span.Count);

            // offset must not be less than 0
            Assert.Throws<ArgumentException>(() => span.ToLargeSpan(-1L, count));

            // count must not be less than 0
            Assert.Throws<ArgumentException>(() => span.ToLargeSpan(0L, -1L));

            // range must not exceed
            Assert.Throws<ArgumentException>(() => span.ToLargeSpan(1L, capacity));

            if (count2 < 0L || offset2 + count2 > span.Count)
            {
                return;
            }

            span = span.ToLargeSpan(offset, count2);

            Assert.AreEqual(offset2, span.Offset);
            Assert.AreEqual(count2, span.Count);
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
        public void SetGet(long capacity, long offset)
        {
            // input check
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            long count = capacity - 2L * offset;

            if (count < 0L || offset + count > capacity)
            {
                return;
            }

            LargeArray<long> largeArray = new LargeArray<long>(capacity);

            LargeSpan<long> span = largeArray.ToLargeSpan(offset, count);

            LargeArrayTest.SetGetTest(span, 0L);
            LargeArrayTest.SetGetTest(span, offset);
        }


        [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
        public void Enumeration(long capacity, long offset)
        {
            // input check
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            long count = capacity - 2L * offset;

            if (count < 0L || offset + count > capacity)
            {
                return;
            }

            LargeArray<long> largeArray = new LargeArray<long>(capacity);

            LargeSpan<long> span = largeArray.ToLargeSpan(offset, count);

            LargeArrayTest.EnumerationTest(span, 0L);
            LargeArrayTest.EnumerationTest(span, offset);
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
        public void Sort(long capacity, long offset)
        {
            // input check
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            long count = capacity - 2L * offset;

            if (count < 0L || offset + count > capacity)
            {
                return;
            }

            LargeArray<long> largeArray = new LargeArray<long>(capacity);

            LargeSpan<long> span = largeArray.ToLargeSpan(offset, count);

            LargeArrayTest.SortTest(span, 0L);
            LargeArrayTest.SortTest(span, offset);
        }

        [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
        public void BinarySearch(long capacity, long offset)
        {
            // input check
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            long count = capacity - 2L * offset;

            if (count < 0L || offset + count > capacity)
            {
                return;
            }

            LargeArray<long> largeArray = new LargeArray<long>(capacity);

            LargeSpan<long> span = largeArray.ToLargeSpan(offset, count);

            LargeArrayTest.BinarySearchTest(span, 0L);
            LargeArrayTest.BinarySearchTest(span, offset);
        }


        [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesWithOffsetTestCasesArguments))]
        public void Contains(long capacity, long offset)
        {
            // input check
            if (capacity < 0 || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            long count = capacity - 2L * offset;

            if (count < 0L || offset + count > capacity)
            {
                return;
            }

            LargeArray<long> largeArray = new LargeArray<long>(capacity);

            LargeSpan<long> span = largeArray.ToLargeSpan(offset, count);

            LargeArrayTest.ContainsTest(span, 0L);
            LargeArrayTest.ContainsTest(span, offset);
        }
    }
}
