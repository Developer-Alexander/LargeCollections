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
using System.Collections.Generic;
using System.Linq;

namespace LargeCollections.Test
{
    public class LargeDictionaryTest
    {

        [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesTestCasesArguments))]
        public void AddSetGetContainsRemove(long capacity)
        {
            if (capacity < 0L || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                return;
            }

            LargeDictionary<long, long> largeDictionary = new LargeDictionary<long, long>();

            for (long i = 0; i < capacity; i++)
            {
                if (i % 2 == 0)
                {
                    largeDictionary[i] = i;
                }
                else
                {
                    largeDictionary.Add(new KeyValuePair<long, long>(i, i));
                }

                Assert.AreEqual(i + 1L, largeDictionary.Count);
                Assert.IsTrue(largeDictionary.TryGetValue(i, out long foundI));
                Assert.AreEqual(i, foundI);
                Assert.AreEqual(i, largeDictionary[i]);
                Assert.AreEqual(i, largeDictionary.Get(i));
                Assert.IsTrue(largeDictionary.ContainsKey(i));
                Assert.IsTrue(largeDictionary.Contains(new KeyValuePair<long, long>(i, i)));
            }

            CollectionAssert.AreEquivalent(largeDictionary.Keys, LargeEnumerable.Range(capacity));
            CollectionAssert.AreEquivalent(largeDictionary.Values, LargeEnumerable.Range(capacity));
            CollectionAssert.AreEquivalent(largeDictionary, LargeEnumerable.Range(capacity).Select(i => new KeyValuePair<long, long>(i, i)));

            for (long i = 0; i < capacity; i++)
            {
                largeDictionary.Remove(i);

                Assert.AreEqual(capacity - 1L - i, largeDictionary.Count);
                Assert.IsFalse(largeDictionary.TryGetValue(i, out long foundI));
            }
        }
    }
}
