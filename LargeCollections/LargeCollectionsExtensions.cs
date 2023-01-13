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

using LargeCollections.IO;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LargeCollections
{
    public static class LargeCollectionsExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyLargeSpan<T> ToReadOnlyLargeSpan<T>(this IReadOnlyLargeArray<T> items, long offset, long count)
        {
            ReadOnlyLargeSpan<T> largeSpan = new ReadOnlyLargeSpan<T>(items, offset, count);
            return largeSpan;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LargeSpan<T> ToLargeSpan<T>(this ILargeArray<T> items, long offset, long count)
        {
            LargeSpan<T> largeSpan = new LargeSpan<T>(items, offset, count);
            return largeSpan;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LargeMemoryStream AsStream(this LargeArray<byte> array, bool isReadonly = false)
        {
            LargeMemoryStream stream = new LargeMemoryStream(array, true, isReadonly);
            return stream;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this IReadOnlyLargeArray<T> items, ILargeArray<T> target, long count, long sourceOffset = 0L, long targetOffset = 0L)
        {
            if(target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            if(targetOffset < 0L || count < 0L || targetOffset + count > target.Count)
            {
                throw new ArgumentException("targetOffset < 0L || count < 0L || targetOffset + count > target.Count");
            }
            if (sourceOffset < 0L || count < 0L || sourceOffset + count > items.Count)
            {
                throw new ArgumentException("sourceOffset < 0L || count < 0L || sourceOffset + count > items.Count");
            }

            for(long i = 0L; i < count; i++)
            {
                target[targetOffset + i] = items[sourceOffset + i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this IReadOnlyLargeArray<T> items, T[] target, int count, long sourceOffset = 0L, int targetOffset = 0)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            if (targetOffset < 0 || count < 0L || targetOffset + count > target.Length)
            {
                throw new ArgumentException("targetOffset < 0 || count < 0L || targetOffset + count > target.Length");
            }
            if (sourceOffset < 0L || count < 0L || sourceOffset + count > items.Count)
            {
                throw new ArgumentException("sourceOffset < 0L || count < 0L || sourceOffset + count > items.Count");
            }

            for (int i = 0; i < count; i++)
            {
                target[targetOffset + i] = items[sourceOffset + i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[] items, ILargeArray<T> target, int count, int sourceOffset = 0, long targetOffset = 0L)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            if (targetOffset < 0L || count < 0L || targetOffset + count > target.Count)
            {
                throw new ArgumentException("targetOffset < 0L || count < 0L || targetOffset + count > target.Count");
            }
            if (sourceOffset < 0 || count < 0L || sourceOffset + count > items.Length)
            {
                throw new ArgumentException("sourceOffset < 0 || count < 0L || sourceOffset + count > items.Length");
            }

            for (int i = 0; i < count; i++)
            {
                target[targetOffset + i] = items[sourceOffset + i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(this ILargeArray<T> items, IReadOnlyLargeArray<T> source, long count, long targetOffset = 0L, long sourceOffset = 0L)
        {
            if(source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source.CopyTo(items, count, sourceOffset, targetOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(this ILargeArray<T> items, T[] source, int count, long targetOffset = 0L, int sourceOffset = 0)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source.CopyTo(items, count, sourceOffset, targetOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(this T[] items, IReadOnlyLargeArray<T> source, int count, int targetOffset = 0, long sourceOffset = 0L)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source.CopyTo(items, count, sourceOffset, targetOffset);
        }
    }
}
