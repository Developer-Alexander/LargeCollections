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

using System.Runtime.CompilerServices;

namespace LargeCollections
{
    public static class LargeCollectionsExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[] source, ILargeArray<T> target, long sourceOffset, long targetOffset, long count)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            StorageExtensions.CheckRange(sourceOffset, count, source.LongLength);
            StorageExtensions.CheckRange(targetOffset, count, target.Count);

            ReadOnlySpan<T> sourceSpan = source.AsSpan((int)sourceOffset, (int)count);

            sourceSpan.CopyTo(target, sourceOffset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this ReadOnlySpan<T> source, ILargeArray<T> target, long targetOffset, long count)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            StorageExtensions.CheckRange(targetOffset, count, target.Count);
            if (target is LargeArray<T> largeArrayTarget)
            {
                T[][] targetStorage = largeArrayTarget.GetStorage();
                targetStorage.StorageCopyFrom(source, targetOffset, count);
            }
            else if (target is LargeList<T> largeListTarget)
            {
                T[][] targetStorage = largeListTarget.GetStorage();
                targetStorage.StorageCopyFrom(source, targetOffset, count);
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    T item = source[i];
                    target[targetOffset + i] = item;
                }
            }
        }

        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LargeReadOnlySpan<T> AsLargeReadOnlySpan<T>(this IReadOnlyLargeArray<T> array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            LargeReadOnlySpan<T> result = new(array);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LargeReadOnlySpan<T> AsLargeReadOnlySpan<T>(this IReadOnlyLargeArray<T> array, long offset)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            StorageExtensions.CheckRange(offset, array.Count - offset, array.Count);

            LargeReadOnlySpan<T> result = new(array, offset);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LargeReadOnlySpan<T> AsLargeReadOnlySpan<T>(this IReadOnlyLargeArray<T> array, long offset, long count)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            StorageExtensions.CheckRange(offset, count, array.Count);

            LargeReadOnlySpan<T> result = new(array, offset, count);
            return result;
        }
        */
    }
}
