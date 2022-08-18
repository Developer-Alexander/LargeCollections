# What is LargeCollections?

LargeCollections is a library for .NET framework that offers a number of interfaces and types for collections that can store up to 4607183514018775041 items.
In comparison many .NET standard collections are limited to about 2.1 billion items.
There are also thread-safe (concurrent) variants of the corresponding collection types.

Currently supported collections are:
- LargeArray<T>
- LargeList<T>
- LargeSet<T>
- LargeDictionary<TKey, TValue>
- ReadOnlyLargeSpan<T>
- LargeSpan<T>
- ConcurrentLargeArray<T>
- ConcurrentLargeList<T>
- ConcurrentLargeSet<T>
- ConcurrentLargeDictionary<TKey, TValue>
- DiskCache<TKey, TValue>
- SpatialDiskCache<long, TValue>

DiskCache<TKey, TValue> is a dictionary-like collection that allows to limit the amount of memory (RAM) in MB that will be used.
Any memory requirement that exceeds this amount is automatically swapped out to disk. 
Additionally it offers multi-threaded operations for performance improvements.

SpatialDiskCache<long, TValue> is a DiskCache<long, TValue> that allows to create a spatial index for the contained elements that can be used for spatial queries.

# License

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

SPDX-License-Identifier: MIT