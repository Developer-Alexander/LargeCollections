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

namespace LargeCollections;

public static class EnumerableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LargeList<T> ToLargeList<T>(this IEnumerable<T> items)
    {
        LargeList<T> largeList = new(items);
        return largeList;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LargeSet<T> ToLargeSet<T>(this IEnumerable<T> items)
    {
        LargeSet<T> largeSet = new(items);
        return largeSet;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Skip<T>(this IEnumerable<T> items, long count)
    {
        if (count < 0L)
        {
            yield break;
        }

        long currentCount = 0L;

        foreach (T item in items)
        {
            if (currentCount >= count)
            {
                yield return item;
            }
            else
            {
                currentCount++;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Take<T>(this IEnumerable<T> items, long count)
    {
        if (count <= 0L)
        {
            yield break;
        }

        long currentCount = 0L;

        foreach (T item in items)
        {
            if (currentCount < count)
            {
                yield return item;
                currentCount++;
            }
            else
            {
                yield break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> SkipTake<T>(this IEnumerable<T> items, long skipCount, long takeCount)
    {
        if (takeCount < 0L)
        {
            yield break;
        }

        long currentSkipCount = 0L;
        long currentTakeCount = 0L;

        foreach (T item in items)
        {
            if (currentSkipCount >= skipCount)
            {
                if (currentTakeCount < takeCount)
                {
                    yield return item;
                    currentTakeCount++;
                }
                else
                {
                    yield break;
                }
            }
            else
            {
                currentSkipCount++;
            }

        }
    }
}
