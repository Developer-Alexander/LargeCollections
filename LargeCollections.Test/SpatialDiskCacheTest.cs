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

public class SpatialDiskCacheTest
{
    [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesTestCasesArguments))]
    public void AddSetGetContainsRemoveEnumerateQuery(long capacity)
    {
        if (capacity < 0L || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        using (SpatialDiskCache<long> spatialLongDiskCache = new($"spatial_long_{capacity}", degreeOfParallelism: 4))
        using (SpatialDiskCache<string> spatialStringDiskCache = new($"spatial_string_{capacity}", degreeOfParallelism: 4))
        using (SpatialDiskCache<byte[]> spatialBytesDiskCache = new($"spatial_bytes_{capacity}", degreeOfParallelism: 4))
        using (SpatialDiskCache<LongStruct> spatialStructDiskCache = new($"spatial_struct_{capacity}", degreeOfParallelism: 4,
            serializeValueFunction: LongStruct.Serialize,
            deserializeValueFunction: LongStruct.Deserialize))
        {
            for (long i = 0; i < capacity; i++)
            {
                string stringI = i.ToString();
                byte[] bytesI = BitConverter.GetBytes(i);
                LongStruct structI = new(i);

                spatialLongDiskCache.Set(i, i, new BoundingBox(i, i, i, i));
                spatialStringDiskCache.Set(i, stringI, new BoundingBox(i, i, i, i));
                spatialBytesDiskCache.Set(i, bytesI, new BoundingBox(i, i, i, i));
                spatialStructDiskCache.Set(i, structI, new BoundingBox(i, i, i, i));

                Assert.AreEqual(i + 1L, spatialLongDiskCache.Count);
                Assert.IsTrue(spatialLongDiskCache.TryGetValue(i, out long foundI));
                Assert.AreEqual(i, foundI);
                Assert.AreEqual(i, spatialLongDiskCache[i]);
                Assert.AreEqual(i, spatialLongDiskCache.Get(i));
                Assert.IsTrue(spatialLongDiskCache.ContainsKey(i));
                Assert.IsTrue(spatialLongDiskCache.Contains(new KeyValuePair<long, long>(i, i)));

                Assert.AreEqual(i + 1L, spatialStringDiskCache.Count);
                Assert.IsTrue(spatialStringDiskCache.TryGetValue(i, out string foundStringI));
                Assert.AreEqual(stringI, foundStringI);
                Assert.AreEqual(stringI, spatialStringDiskCache[i]);
                Assert.AreEqual(stringI, spatialStringDiskCache.Get(i));
                Assert.IsTrue(spatialStringDiskCache.ContainsKey(i));
                Assert.IsTrue(spatialStringDiskCache.Contains(new KeyValuePair<long, string>(i, stringI)));

                Assert.AreEqual(i + 1L, spatialBytesDiskCache.Count);
                Assert.IsTrue(spatialBytesDiskCache.TryGetValue(i, out byte[] foundBytesI));
                Assert.AreEqual(bytesI, foundBytesI);
                Assert.AreEqual(bytesI, spatialBytesDiskCache[i]);
                Assert.AreEqual(bytesI, spatialBytesDiskCache.Get(i));
                Assert.IsTrue(spatialBytesDiskCache.ContainsKey(i));
                Assert.IsTrue(spatialBytesDiskCache.Contains(new KeyValuePair<long, byte[]>(i, bytesI)));

                Assert.AreEqual(i + 1L, spatialStructDiskCache.Count);
                Assert.IsTrue(spatialStructDiskCache.TryGetValue(i, out LongStruct foundStructI));
                Assert.AreEqual(structI, foundStructI);
                Assert.AreEqual(structI, spatialStructDiskCache[i]);
                Assert.AreEqual(structI, spatialStructDiskCache.Get(i));
                Assert.IsTrue(spatialStructDiskCache.ContainsKey(i));
                Assert.IsTrue(spatialStructDiskCache.Contains(new KeyValuePair<long, LongStruct>(i, structI)));
            }

            CollectionAssert.AreEquivalent(spatialLongDiskCache.Keys, LargeEnumerable.Range(capacity));
            CollectionAssert.AreEquivalent(spatialLongDiskCache.Values, LargeEnumerable.Range(capacity));
            CollectionAssert.AreEquivalent(spatialLongDiskCache, LargeEnumerable.Range(capacity).Select(i => new KeyValuePair<long, long>(i, i)));

            CollectionAssert.AreEquivalent(spatialStringDiskCache.Keys, LargeEnumerable.Range(capacity));
            CollectionAssert.AreEquivalent(spatialStringDiskCache.Values, LargeEnumerable.Range(capacity).Select(i => i.ToString()));
            CollectionAssert.AreEquivalent(spatialStringDiskCache, LargeEnumerable.Range(capacity).Select(i => new KeyValuePair<long, string>(i, i.ToString())));

            CollectionAssert.AreEquivalent(spatialBytesDiskCache.Keys, LargeEnumerable.Range(capacity));
            CollectionAssert.AreEquivalent(spatialBytesDiskCache.Values, LargeEnumerable.Range(capacity).Select(i => BitConverter.GetBytes(i)));
            CollectionAssert.AreEquivalent(spatialBytesDiskCache, LargeEnumerable.Range(capacity).Select(i => new KeyValuePair<long, byte[]>(i, BitConverter.GetBytes(i))));

            CollectionAssert.AreEquivalent(spatialStructDiskCache.Keys, LargeEnumerable.Range(capacity));
            CollectionAssert.AreEquivalent(spatialStructDiskCache.Values, LargeEnumerable.Range(capacity).Select(i => new LongStruct(i)));
            CollectionAssert.AreEquivalent(spatialStructDiskCache, LargeEnumerable.Range(capacity).Select(i => new KeyValuePair<long, LongStruct>(i, new LongStruct(i))));

            long min = (long)(capacity * 0.25);
            long max = (long)(capacity * 0.75);
            BoundingBox boundingBox = new(min, max, min, max);

            CollectionAssert.AreEquivalent(spatialLongDiskCache.Query(boundingBox), LargeEnumerable.Range(capacity).Where(i => i >= min && i <= max).Select(i => new KeyValuePair<long, long>(i, i)));

            CollectionAssert.AreEquivalent(spatialStringDiskCache.Query(boundingBox), LargeEnumerable.Range(capacity).Where(i => i >= min && i <= max).Select(i => new KeyValuePair<long, string>(i, i.ToString())));

            CollectionAssert.AreEquivalent(spatialBytesDiskCache.Query(boundingBox), LargeEnumerable.Range(capacity).Where(i => i >= min && i <= max).Select(i => new KeyValuePair<long, byte[]>(i, BitConverter.GetBytes(i))));

            CollectionAssert.AreEquivalent(spatialStructDiskCache.Query(boundingBox), LargeEnumerable.Range(capacity).Where(i => i >= min && i <= max).Select(i => new KeyValuePair<long, LongStruct>(i, new LongStruct(i))));


            for (long i = 0; i < capacity; i++)
            {
                string stringI = i.ToString();
                byte[] bytesI = BitConverter.GetBytes(i);
                LongStruct structI = new(i);

                spatialLongDiskCache.Remove(i);
                spatialStringDiskCache.Remove(i);
                spatialBytesDiskCache.Remove(i);
                spatialStructDiskCache.Remove(i);

                Assert.AreEqual(capacity - 1L - i, spatialLongDiskCache.Count);
                Assert.IsFalse(spatialLongDiskCache.TryGetValue(i, out long foundI));

                Assert.AreEqual(capacity - 1L - i, spatialStringDiskCache.Count);
                Assert.IsFalse(spatialStringDiskCache.TryGetValue(i, out string foundStringI));

                Assert.AreEqual(capacity - 1L - i, spatialBytesDiskCache.Count);
                Assert.IsFalse(spatialBytesDiskCache.TryGetValue(i, out byte[] foundBytesI));

                Assert.AreEqual(capacity - 1L - i, spatialStructDiskCache.Count);
                Assert.IsFalse(spatialStructDiskCache.TryGetValue(i, out LongStruct foundStructI));
            }
        }
    }
}
