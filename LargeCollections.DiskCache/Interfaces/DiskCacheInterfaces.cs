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

using System;
using System.Collections.Generic;
using System.Text;

namespace LargeCollections
{
    public interface IDiskCache<TKey, TValue> : ILargeDictionary<TKey, TValue>
    {
        /// <summary>
        /// Maximum amount of memory (RAM) in MB that will be used.
        /// Any memory requirement that exceeds this is automatically swapped out to disk.
        /// The specified maximum may be exceeded by some percentage.
        /// </summary>
        long MaxMemorySize { get; }

        /// <summary>
        /// Number of Threads that will be used.
        /// It must be greater than 0.
        /// </summary>
        byte DegreeOfParallelism { get; }

        void AddParallel(IEnumerable<KeyValuePair<TKey, TValue>>[] parallelItems);

        IEnumerable<KeyValuePair<TKey, TValue>>[] GetAllParallel();

        void RemoveParallel(IEnumerable<TKey>[] parallelKeys);

        void RemoveParallel(IEnumerable<KeyValuePair<TKey, TValue>>[] parallelItems);
    }

    public interface ISpatialDiskCache<TValue> : IDiskCache<long, TValue>
    {
        void Add(KeyValuePair<long, TValue> item, BoundingBox boundingBox);

        void Add(IEnumerable<(KeyValuePair<long, TValue>, BoundingBox boundingBox)> items);

        void AddParallel(IEnumerable<(KeyValuePair<long, TValue> item, BoundingBox boundingBox)>[] parallelItems);

        IEnumerable<KeyValuePair<long, TValue>> Query(BoundingBox boundingBox);

        IEnumerable<KeyValuePair<long, TValue>>[] QueryParallel(BoundingBox boundingBox);

        void Set(long key, TValue value, BoundingBox boundingBox);
    }
}
