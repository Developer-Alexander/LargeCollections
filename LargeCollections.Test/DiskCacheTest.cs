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

public struct LongStruct
{
    public long Value;

    public LongStruct(long value)
    {
        Value = value;
    }

    public static byte[] Serialize(LongStruct longStruct)
    {
        return BitConverter.GetBytes(longStruct.Value);
    }

    public static LongStruct Deserialize(byte[] serializedLongStruct)
    {
        long value = BitConverter.ToInt64(serializedLongStruct, 0);
        return new LongStruct(value);
    }
}

public class DiskCacheTest
{
    [TestCaseSource(typeof(LargeArrayTest), nameof(LargeArrayTest.CapacitiesTestCasesArguments))]
    public void AddSetGetContainsRemoveEnumerate(long capacity)
    {
        if (capacity < 0L || capacity > Constants.MaxLargeCollectionCount)
        {
            return;
        }

        using DiskCache<long, long> longLongDiskCache = new($"long_long_{capacity}", degreeOfParallelism: 4);
        using DiskCache<string, string> stringStringDiskCache = new($"string_string_{capacity}", degreeOfParallelism: 4);
        using DiskCache<byte[], byte[]> bytesBytesDiskCache = new($"bytes_bytes_{capacity}", degreeOfParallelism: 4);
        using DiskCache<LongStruct, LongStruct> structStructDiskCache = new($"struct_struct_{capacity}", degreeOfParallelism: 4,
            serializeKeyFunction: LongStruct.Serialize,
            deserializeKeyFunction: LongStruct.Deserialize,
            serializeValueFunction: LongStruct.Serialize,
            deserializeValueFunction: LongStruct.Deserialize);

        for (long i = 0; i < capacity; i++)
        {
            string stringI = i.ToString();
            byte[] bytesI = BitConverter.GetBytes(i);
            LongStruct structI = new(i);

            longLongDiskCache[i] = i;
            stringStringDiskCache[stringI] = stringI;
            bytesBytesDiskCache[bytesI] = bytesI;
            structStructDiskCache[structI] = structI;

            Assert.AreEqual(i + 1L, longLongDiskCache.Count);
            Assert.IsTrue(longLongDiskCache.TryGetValue(i, out long foundI));
            Assert.AreEqual(i, foundI);
            Assert.AreEqual(i, longLongDiskCache[i]);
            Assert.AreEqual(i, longLongDiskCache.Get(i));
            Assert.IsTrue(longLongDiskCache.ContainsKey(i));
            Assert.IsTrue(longLongDiskCache.Contains(new KeyValuePair<long, long>(i, i)));

            Assert.AreEqual(i + 1L, stringStringDiskCache.Count);
            Assert.IsTrue(stringStringDiskCache.TryGetValue(stringI, out string foundStringI));
            Assert.AreEqual(stringI, foundStringI);
            Assert.AreEqual(stringI, stringStringDiskCache[stringI]);
            Assert.AreEqual(stringI, stringStringDiskCache.Get(stringI));
            Assert.IsTrue(stringStringDiskCache.ContainsKey(stringI));
            Assert.IsTrue(stringStringDiskCache.Contains(new KeyValuePair<string, string>(stringI, stringI)));

            Assert.AreEqual(i + 1L, bytesBytesDiskCache.Count);
            Assert.IsTrue(bytesBytesDiskCache.TryGetValue(bytesI, out byte[] foundBytesI));
            Assert.AreEqual(bytesI, foundBytesI);
            Assert.AreEqual(bytesI, bytesBytesDiskCache[bytesI]);
            Assert.AreEqual(bytesI, bytesBytesDiskCache.Get(bytesI));
            Assert.IsTrue(bytesBytesDiskCache.ContainsKey(bytesI));
            Assert.IsTrue(bytesBytesDiskCache.Contains(new KeyValuePair<byte[], byte[]>(bytesI, bytesI)));

            Assert.AreEqual(i + 1L, structStructDiskCache.Count);
            Assert.IsTrue(structStructDiskCache.TryGetValue(structI, out LongStruct foundStructI));
            Assert.AreEqual(structI, foundStructI);
            Assert.AreEqual(structI, structStructDiskCache[structI]);
            Assert.AreEqual(structI, structStructDiskCache.Get(structI));
            Assert.IsTrue(structStructDiskCache.ContainsKey(structI));
            Assert.IsTrue(structStructDiskCache.Contains(new KeyValuePair<LongStruct, LongStruct>(structI, structI)));
        }

        CollectionAssert.AreEquivalent(longLongDiskCache.Keys, LargeEnumerable.Range(capacity));
        CollectionAssert.AreEquivalent(longLongDiskCache.Values, LargeEnumerable.Range(capacity));
        CollectionAssert.AreEquivalent(longLongDiskCache, LargeEnumerable.Range(capacity).Select(i => new KeyValuePair<long, long>(i, i)));

        CollectionAssert.AreEquivalent(stringStringDiskCache.Keys, LargeEnumerable.Range(capacity).Select(i => i.ToString()));
        CollectionAssert.AreEquivalent(stringStringDiskCache.Values, LargeEnumerable.Range(capacity).Select(i => i.ToString()));
        CollectionAssert.AreEquivalent(stringStringDiskCache, LargeEnumerable.Range(capacity).Select(i => new KeyValuePair<string, string>(i.ToString(), i.ToString())));

        CollectionAssert.AreEquivalent(bytesBytesDiskCache.Keys, LargeEnumerable.Range(capacity).Select(i => BitConverter.GetBytes(i)));
        CollectionAssert.AreEquivalent(bytesBytesDiskCache.Values, LargeEnumerable.Range(capacity).Select(i => BitConverter.GetBytes(i)));
        CollectionAssert.AreEquivalent(bytesBytesDiskCache, LargeEnumerable.Range(capacity).Select(i => new KeyValuePair<byte[], byte[]>(BitConverter.GetBytes(i), BitConverter.GetBytes(i))));

        CollectionAssert.AreEquivalent(structStructDiskCache.Keys, LargeEnumerable.Range(capacity).Select(i => new LongStruct(i)));
        CollectionAssert.AreEquivalent(structStructDiskCache.Values, LargeEnumerable.Range(capacity).Select(i => new LongStruct(i)));
        CollectionAssert.AreEquivalent(structStructDiskCache, LargeEnumerable.Range(capacity).Select(i => new KeyValuePair<LongStruct, LongStruct>(new LongStruct(i), new LongStruct(i))));

        for (long i = 0; i < capacity; i++)
        {
            string stringI = i.ToString();
            byte[] bytesI = BitConverter.GetBytes(i);
            LongStruct structI = new(i);

            longLongDiskCache.Remove(i);
            stringStringDiskCache.Remove(stringI);
            bytesBytesDiskCache.Remove(bytesI);
            structStructDiskCache.Remove(structI);

            Assert.AreEqual(capacity - 1L - i, longLongDiskCache.Count);
            Assert.IsFalse(longLongDiskCache.TryGetValue(i, out long foundI));

            Assert.AreEqual(capacity - 1L - i, stringStringDiskCache.Count);
            Assert.IsFalse(stringStringDiskCache.TryGetValue(stringI, out string foundStringI));

            Assert.AreEqual(capacity - 1L - i, bytesBytesDiskCache.Count);
            Assert.IsFalse(bytesBytesDiskCache.TryGetValue(bytesI, out byte[] foundBytesI));

            Assert.AreEqual(capacity - 1L - i, structStructDiskCache.Count);
            Assert.IsFalse(structStructDiskCache.TryGetValue(structI, out LongStruct foundStructI));
        }
    }

}
