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
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using System.Drawing;
using LargeCollections.IO;

namespace LargeCollections
{
    public static class StreamExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, IEnumerable<byte> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (byte currentByte in source)
            {
                stream.WriteByte(currentByte);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, IReadOnlyLargeArray<byte> source, long offset = 0L)
        {
            if (stream is LargeMemoryStream largeMemoryStream)
            {
                // Check is done by LargeMemoryStream.Write()
                largeMemoryStream.Write(source, offset);
            }
            else
            {
                if (source == null)
                {
                    throw new ArgumentNullException(nameof(source));
                }

                long count = source.Count - offset;

                if (offset < 0 || count < 0 || offset + count > source.Count)
                {
                    throw new ArgumentException("offset < 0 || count < 0 || offset + count > source.Count");
                }

                for (long i = 0L; i < count; i++)
                {
                    stream.WriteByte(source[i + offset]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, IReadOnlyLargeArray<byte> source, long offset, long count)
        {
            if (stream is LargeMemoryStream largeMemoryStream)
            {
                // Check is done by LargeMemoryStream.Write()
                largeMemoryStream.Write(source, offset, count);
            }
            else
            {
                if (source == null)
                {
                    throw new ArgumentNullException(nameof(source));
                }

                if (offset < 0 || count < 0 || offset + count > source.Count)
                {
                    throw new ArgumentException("offset < 0 || count < 0 || offset + count > source.Count");
                }

                for (long i = 0L; i < count; i++)
                {
                    stream.WriteByte(source[i + offset]);
                }
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Read(this Stream stream, ILargeArray<byte> target, long offset = 0L)
        {
            long readCount = 0L;
            if (stream is LargeMemoryStream largeMemoryStream)
            {
                // Check is done by LargeMemoryStream.Read()
                readCount = largeMemoryStream.Read(target, offset);
            }
            else
            {
                if (target == null)
                {
                    throw new ArgumentNullException(nameof(target));
                }

                long count = target.Count - offset;

                if (offset < 0 || count < 0 || offset + count > target.Count)
                {
                    throw new ArgumentException("offset < 0 || count < 0 || offset + count > target.Count");
                }

                for (long i = 0L; i < count; i++)
                {
                    int currentByte = stream.ReadByte();
                    if (currentByte < 0)
                    {
                        continue;
                    }
                    target[i + offset] = (byte)currentByte;
                    readCount++;
                }

            }

            return readCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Read(this Stream stream, ILargeArray<byte> target, long offset, long count)
        {
            long readCount = 0L;
            if (stream is LargeMemoryStream largeMemoryStream)
            {
                // Check is done by LargeMemoryStream.Read()
                readCount = largeMemoryStream.Read(target, offset, count);
            }
            else
            {
                if (target == null)
                {
                    throw new ArgumentNullException(nameof(target));
                }
                if (offset < 0 || count < 0 || offset + count > target.Count)
                {
                    throw new ArgumentException("offset < 0 || count < 0 || offset + count > target.Count");
                }

                for (long i = 0L; i < count; i++)
                {
                    int currentByte = stream.ReadByte();
                    if (currentByte < 0)
                    {
                        continue;
                    }
                    target[i + offset] = (byte)currentByte;
                    readCount++;
                }

            }

            return readCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<byte> AsEnumerable(this Stream stream)
        {
            int currentByte = 0;
            while ((currentByte = stream.ReadByte()) >= 0)
            {
                yield return (byte)currentByte;
            }
        }
    }
}
