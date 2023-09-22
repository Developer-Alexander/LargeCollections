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

namespace LargeCollections
{
    public static class StreamExtensions
    {
        public static long Read(this Stream stream, ILargeArray<byte> target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            return stream.Read(target, 0L, target.Count);
        }

        public static long Read(this Stream stream, ILargeArray<byte> target, long offset)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            return stream.Read(target, offset, target.Count - offset);
        }

        public static long Read(this Stream stream, ILargeArray<byte> target, long offset, long count)
        {
            if (!stream.CanRead)
            {
                throw new NotSupportedException();
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            StorageExtensions.CheckRange(offset, count, target.Count);

            long maxReadableCount = stream.Length - stream.Position;
            if (count < maxReadableCount)
            {
                maxReadableCount = count;
            }

            if (stream is LargeReadableMemoryStream largeReadableMemoryStream)
            {
                largeReadableMemoryStream.Source.CopyTo(target, stream.Position, offset, maxReadableCount);
                stream.Position += maxReadableCount;
            }
            else
            {
                // TODO Improve performance
                for (long i = 0L; i < maxReadableCount; i++)
                {
                    int currentByte = stream.ReadByte();
                    if (currentByte < 0)
                    {
                        break;
                    }
                    target[i + offset] = (byte)currentByte;
                }
            }

            return maxReadableCount;
        }

        public static void Write(this Stream stream, IReadOnlyLargeArray<byte> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            stream.Write(source, 0L, source.Count);
        }

        public static void Write(this Stream stream, IReadOnlyLargeArray<byte> source, long offset)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            stream.Write(source, offset, source.Count - offset);
        }

        public static void Write(this Stream stream, IReadOnlyLargeArray<byte> source, long offset, long count)
        {
            if (!stream.CanWrite)
            {
                throw new NotSupportedException();
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            StorageExtensions.CheckRange(offset, count, source.Count);

            if (stream is LargeWritableMemoryStream largeWritableMemoryStream)
            {
                largeWritableMemoryStream.Storage.Add(source, offset, count);
            }
            else
            {
                // TODO Improve performance
                for (long i = 0L; i < count; i++)
                {
                    byte currentByte = source[i + offset];
                    stream.WriteByte(currentByte);
                }
            }
        }
    }
}
