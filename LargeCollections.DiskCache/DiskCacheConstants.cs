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
    public static class DiskCacheConstants
    {
        /// <summary>
        /// Sqlite page size in Byte
        /// </summary>
        public const long PageSize = 4096L;

        /// <summary>
        /// Maximum amount of memory (RAM) in MB that will be used if no other value in specified.
        /// </summary>
        public const long DefaultMaxMemorySize = 0L;

        /// <summary>
        /// The size of an item in Byte which it must not exeed. This limitation is inherited from Sqlite.
        /// </summary>
        public const long MaxItemLength = 1_000_000_000L;

        public const byte DefaultDegreeOfParallelism = 1;

        public const string DefaultFileExtension = "sqlite";
    }
}
