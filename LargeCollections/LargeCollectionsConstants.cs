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
    public static class LargeCollectionsConstants
    {
#if UNIT_TEST
        public const long MaxStandardArrayCapacity = 10L;
#else
        /// <summary>
        /// The maximum number of elements that can be stored in a .NET array. <see cref="https://docs.microsoft.com/en-us/dotnet/api/system.array"/>
        /// </summary>
        public const long MaxStandardArrayCapacity = 2_146_435_071L;
#endif

        /// <summary>
        /// The maximum number of items that can be stored in a <see cref="IReadOnlyCollection{T}"/>
        /// </summary>
        public const long MaxLargeCollectionCount = MaxStandardArrayCapacity * MaxStandardArrayCapacity;

        public const double MaxCapacityGrowFactor = 3.0;

        public const double DefaultCapacityGrowFactor = 1.4;

        public const long DefaultFixedCapacityGrowAmount = 100L * 1024L * 1024L;

        public const long DefaultFixedCapacityGrowLimit = 50L * 1024L * 1024L;

        public const double DefaultMinLoadFactor = 0.5;
        public const double DefaultMaxLoadFactor = 1.0;

        public const double DefaultMinLoadFactorTolerance = 0.1;
    }
}
