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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LargeCollections.IO
{
    /// <summary>
    /// A Stream based on a <see cref="LargeArray{byte}"/>. Its storage extends like a <see cref="LargeList{byte}"/> on demand when writing to the stream.
    /// Reading and writing operations are supported at the same time.
    /// </summary>
    [DebuggerDisplay("LargeMemoryStream: Position = {Position}, Length = {Length}, Capacity = {Capacity}")]
    public class LargeMemoryStream : Stream
    {
        protected LargeArray<byte> _storage;

        protected bool _isReadonly;

        public override bool CanRead
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => true;
        }

        public override bool CanSeek
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => true;
        }

        public override bool CanWrite
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !_isReadonly;
        }

        public override bool CanTimeout
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => false;
        }

        protected long _length = 0L;
        public override long Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        protected long _position = 0L;
        public override long Position 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _position;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                // It is allowed to set position to a value one behind last valid value
                if(value < 0L || value > Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _position = value;
            }
        }

        public long Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _storage.Count;
            }
        }

        public long ReadableCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Length - Position;
            }
        }

        public LargeMemoryStream(long capacity = 0L)
        {
            if(capacity < 0L || capacity > LargeCollectionsConstants.MaxLargeCollectionCount)
            {
                _storage = new LargeArray<byte>();
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            _storage = new LargeArray<byte>(capacity);

            Position = 0L;
            _length = capacity;
            _isReadonly = false;
        }

        public LargeMemoryStream(byte[] buffer, bool isReadonly = false)
        {
            if (buffer == null)
            {
                _storage = new LargeArray<byte>();
                _length = _storage.Count;
                _isReadonly = isReadonly;
                throw new ArgumentNullException(nameof(buffer));
            }

            _storage = new LargeArray<byte>(buffer.LongLength);

            _storage.CopyFrom(buffer, buffer.Length);

            Position = 0L;
            _length = _storage.Count;
            _isReadonly = isReadonly;
        }

        public LargeMemoryStream(LargeArray<byte> buffer, bool useGivenBuffer = false, bool isReadonly = false)
        {
            if (buffer == null)
            {
                _storage = new LargeArray<byte>();
                _length = _storage.Count;
                _isReadonly = isReadonly;
                throw new ArgumentNullException(nameof(buffer));
            }

            if (useGivenBuffer)
            {
                _storage = buffer;
            }
            else
            {
                _storage = new LargeArray<byte>(buffer.Count);

                _storage.CopyFrom(buffer, buffer.Count);
            }

            Position = 0L;
            _length = _storage.Count;
            _isReadonly = isReadonly;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Flush()
        {
            // Do nothing
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentException("offset < 0 || count < 0 || offset + count > buffer.Length");
            }

            // Is Stream at the end of the stream?
            if (Position >= Length)
            {
                return 0;
            }

            int maxReadCount = (long)count > ReadableCount ? (int)ReadableCount : count;

            _storage.CopyTo(buffer, maxReadCount, Position, offset);

            Position += maxReadCount;

            return maxReadCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadByte()
        {
            // Is Stream at the end of the stream?
            if(Position >= Length)
            {
                return -1;
            }

            Position++;
            return _storage[Position - 1L];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Read(ILargeArray<byte> target, long offset = 0L)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            long count = target.Count - offset;

            // Checks are done in Read(ILargeArray<byte> target, long offset, long count)
            return Read(target, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Read(ILargeArray<byte> target, long offset, long count)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            if (offset < 0 || count < 0 || offset + count > target.Count)
            {
                throw new ArgumentException("offset < 0 || count < 0 || offset + count > target.Count");
            }

            // Is Stream at the end of the stream?
            if (Position >= Length)
            {
                return 0;
            }

            long maxReadCount = count > ReadableCount ? (int)ReadableCount : count;

            _storage.CopyTo(target, maxReadCount, Position, offset);

            Position += maxReadCount;

            return maxReadCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long Seek(long offset, SeekOrigin origin)
        {
            // Checks are done by Position setter
            if(origin == SeekOrigin.Current)
            {
                Position += offset;
            }
            else if (origin == SeekOrigin.End)
            {
                Position = Length + offset;
            }
            else if (origin == SeekOrigin.Begin)
            {
                Position = offset;
            }

            return Position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetLength(long value)
        {
            if (!CanWrite)
            {
                throw new NotSupportedException("The stream does not support writing.");
            }
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if(value > Capacity)
            {
                Resize(value);
            }

            _length = Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite)
            {
                throw new NotSupportedException("The stream does not support writing.");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentException("offset < 0 || count < 0 || offset + count > buffer.Length");
            }

            long newPosition = Position + count;

            if (newPosition >= Capacity)
            {
                long newCapacity = Capacity;
                while (newPosition >= newCapacity)
                {
                    newCapacity = LargeList<byte>.GetGrownCapacity(newCapacity);
                }

                Resize(newCapacity);
            }

            // Checks are done in CopyFrom()
            _storage.CopyFrom(buffer, count, Position, offset);

            if (newPosition >= Length)
            {
                _length = newPosition;
            }

            Position = newPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteByte(byte value)
        {
            if (!CanWrite)
            {
                throw new NotSupportedException("The stream does not support writing.");
            }

            long newPosition = Position + 1L;

            if (Position >= Capacity)
            {
                long newCapacity = LargeList<byte>.GetGrownCapacity(Capacity, LargeCollectionsConstants.DefaultCapacityGrowFactor, LargeCollectionsConstants.DefaultFixedCapacityGrowAmount, LargeCollectionsConstants.DefaultFixedCapacityGrowLimit);
                Resize(newCapacity);
            }

            // Checks are done by index operator
            _storage[Position] = value;

            if (newPosition >= Length)
            {
                _length = newPosition;
            }

            Position = newPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(IReadOnlyLargeArray<byte> source, long offset = 0L)
        {
            if (!CanWrite)
            {
                throw new NotSupportedException("The stream does not support writing.");
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            long count = source.Count - offset;

            // Checks are done in Write(IReadOnlyLargeArray<byte> source, long offset, long count)
            Write(source, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(IReadOnlyLargeArray<byte> source, long offset, long count)
        {
            if (!CanWrite)
            {
                throw new NotSupportedException("The stream does not support writing.");
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (offset < 0L || count < 0L || offset + count > source.Count)
            {
                throw new ArgumentException("offset < 0L || count < 0L || offset + count > source.Count");
            }

            long newPosition = Position + count;
            if (newPosition >= Capacity)
            {
                long newCapacity = Capacity;
                while (newPosition >= newCapacity)
                {
                    newCapacity = LargeList<byte>.GetGrownCapacity(newCapacity, LargeCollectionsConstants.DefaultCapacityGrowFactor, LargeCollectionsConstants.DefaultFixedCapacityGrowAmount, LargeCollectionsConstants.DefaultFixedCapacityGrowLimit);
                }

                Resize(newCapacity);
            }

            // Checks are done in CopyTo()
            source.CopyTo(_storage, count, offset, Position);

            if(newPosition >= Length)
            {
                _length = newPosition;
            }
            Position = newPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Resize(long capacity)
        {
            if (!CanWrite)
            {
                throw new NotSupportedException("The stream does not support writing.");
            }

            // Checks are done by storage.Resize()
            _storage.Resize(capacity);

            if(Position >= _storage.Count)
            {
                Position = _storage.Count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Shrink()
        {
            if (!CanWrite)
            {
                throw new NotSupportedException("The stream does not support writing.");
            }

            if (Capacity > Length)
            {
                Resize(Length);
            }
        }
    }
}
