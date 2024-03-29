﻿/*
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

public class LargeEnumerable
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<long> Range(long start, long end, long step = 1L)
    {
        if (step == 0L)
        {
            yield break;
        }
        if (step > 0L)
        {
            for (long i = start; i < end; i += step)
            {
                yield return i;
            }
        }
        else
        {
            for (long i = start; i > end; i += step)
            {
                yield return i;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<long> Range(long end)
    {
        for (long i = 0L; i < end; i++)
        {
            yield return i;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Repeat<T>(T item, long count)
    {
        if (count <= 0L)
        {
            yield break;
        }

        for (long i = 0L; i < count; i++)
        {
            yield return item;
        }
    }
}
