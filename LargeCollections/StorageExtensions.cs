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

internal static class StorageExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void CheckRange(long offset, long count, long maxCount)
    {
        if (offset < 0L || count < 0L || offset + count > maxCount)
        {
            throw new ArgumentException("offset < 0L || count < 0L || offset + count > maxCount");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StorageCheckRange<T>(this T[][] array, long offset, long count)
    {
        long maxCount = array.StorageGetCount();
        CheckRange(offset, count, maxCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void CheckIndex(long index, long count)
    {
        if (index < 0L || index >= count)
        {
            throw new IndexOutOfRangeException(nameof(index));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StorageCheckIndex<T>(this T[][] array, long index)
    {
        long count = array.StorageGetCount();
        CheckIndex(index, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long GetGrownCapacity(long capacity,
            double capacityGrowFactor = Constants.DefaultCapacityGrowFactor,
            long fixedCapacityGrowAmount = Constants.DefaultFixedCapacityGrowAmount,
            long fixedCapacityGrowLimit = Constants.DefaultFixedCapacityGrowLimit)
    {

        long newCapacity;
        try
        {
            if (capacity >= fixedCapacityGrowLimit)
            {
                newCapacity = capacity + fixedCapacityGrowAmount;
                newCapacity = newCapacity <= Constants.MaxLargeCollectionCount ? newCapacity : Constants.MaxLargeCollectionCount;
            }
            else
            {
                newCapacity = (long)(capacity * capacityGrowFactor) + 1L;
                newCapacity = newCapacity <= Constants.MaxLargeCollectionCount ? newCapacity : Constants.MaxLargeCollectionCount;
            }
        }
        catch
        {
            newCapacity = Constants.MaxLargeCollectionCount;
        }

        return newCapacity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static (long StorageIndex, long ItemIndex) StorageGetIndex(long index)
    {
        long storageIndex = index >> Constants.StorageIndexShiftAmount;
        long itemIndex = index & (Constants.MaxStorageCapacity - 1L);

        return (storageIndex, itemIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long StorageGetCount<T>(this T[][] array, long offset = 0L)
    {
        long count = (array.LongLength - 1L) * Constants.MaxStorageCapacity;
        count += array[array.LongLength - 1].LongLength;
        count -= offset;
        return count;
    }

    internal static T[][] StorageCreate<T>(long capacity = 0L)
    {
        if (capacity < 0L || capacity > Constants.MaxLargeCollectionCount)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        (long storageCount, long remainder) = StorageGetIndex(capacity);
        storageCount++;

        T[][] result = new T[storageCount][];

        for (long i = 0L; i < storageCount - 1L; i++)
        {
            result[i] = new T[Constants.MaxStorageCapacity];
        }
        result[storageCount - 1L] = new T[remainder];

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T StorageGet<T>(this T[][] array, long index)
    {
        (long storageIndex, long itemIndex) = StorageGetIndex(index);

        T result = array[storageIndex][itemIndex];
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T StorageSet<T>(this T[][] array, long index, T value)
    {
        (long storageIndex, long itemIndex) = StorageGetIndex(index);

        array[storageIndex][itemIndex] = value;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IEnumerable<T> StorageGetAll<T>(this T[][] array, long offset, long count)
    {
        if (count == 0L)
        {
            yield break;
        }

        (long storageIndex, long itemIndex) = StorageGetIndex(offset);

        long currentCount = 0L;

        T[] currentStorage = array[storageIndex];
        for (long j = itemIndex; j < currentStorage.LongLength; j++)
        {
            if (currentCount >= count)
            {
                yield break;
            }
            T item = currentStorage[j];
            yield return item;
            currentCount++;
        }

        for (long i = storageIndex + 1L; i < array.LongLength; i++)
        {
            if (currentCount >= count)
            {
                yield break;
            }
            currentStorage = array[i];
            for (long j = 0L; j < currentStorage.LongLength; j++)
            {
                if (currentCount >= count)
                {
                    yield break;
                }

                T item = currentStorage[j];
                yield return item;
                currentCount++;
            }
        }

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StorageDoForEach<T>(this T[][] array, RefAction<T> action, long offset, long count)
    {
        if (action == null)
        {
            return;
        }
        if (count == 0L)
        {
            return;
        }

        (long storageIndex, long itemIndex) = StorageGetIndex(offset);

        long currentCount = 0L;

        T[] currentStorage = array[storageIndex];
        for (long j = itemIndex; j < currentStorage.LongLength; j++)
        {
            if (currentCount >= count)
            {
                return;
            }
            ref T item = ref currentStorage[j];
            action.Invoke(ref item);
            currentCount++;
        }

        for (long i = storageIndex + 1L; i < array.LongLength; i++)
        {
            if (currentCount >= count)
            {
                return;
            }
            currentStorage = array[i];
            for (long j = 0L; j < currentStorage.LongLength; j++)
            {
                if (currentCount >= count)
                {
                    return;
                }

                ref T item = ref currentStorage[j];
                action.Invoke(ref item);
                currentCount++;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StorageSwap<T>(this T[][] array, long leftIndex, long rightIndex)
    {
        T leftItem = array.StorageGet(leftIndex);
        T rightItem = array.StorageGet(rightIndex);
        array.StorageSet(leftIndex, rightItem);
        array.StorageSet(rightIndex, leftItem);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool Contains<T>(this T[][] array, T item, long offset, long count, Func<T, T, bool> equalsFunction)
    {
        if (count == 0L)
        {
            return false;
        }

        (long storageIndex, long itemIndex) = StorageGetIndex(offset);

        long currentCount = 0L;

        T[] currentStorage = array[storageIndex];
        for (long j = itemIndex; j < currentStorage.LongLength; j++)
        {
            if (currentCount >= count)
            {
                return false;
            }
            T currentItem = currentStorage[j];
            if (equalsFunction.Invoke(item, currentItem))
            {
                return true;
            }
            currentCount++;
        }

        for (long i = storageIndex + 1L; i < array.LongLength; i++)
        {
            if (currentCount >= count)
            {
                return false;
            }
            currentStorage = array[i];
            for (long j = 0L; j < currentStorage.LongLength; j++)
            {
                if (currentCount >= count)
                {
                    return false;
                }

                T currentItem = currentStorage[j];
                if (equalsFunction.Invoke(item, currentItem))
                {
                    return true;
                }
                currentCount++;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StorageHeapify<T>(this T[][] array, long i, long left, long right, Func<T, T, int> comparer)
    {
        if (comparer == null)
        {
            return;
        }
        long maxIndex = i;
        long leftIndex = left + (2L * (i - left)) + 1L;
        long rightIndex = left + (2L * (i - left)) + 2L;

        if (leftIndex <= right && comparer.Invoke(array.StorageGet(maxIndex), array.StorageGet(leftIndex)) < 0)
        {
            maxIndex = leftIndex;
        }

        if (rightIndex <= right && comparer.Invoke(array.StorageGet(maxIndex), array.StorageGet(rightIndex)) < 0)
        {
            maxIndex = rightIndex;
        }

        if (maxIndex != i)
        {
            array.StorageSwap(i, maxIndex);

            array.StorageHeapify(maxIndex, left, right, comparer);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StorageSort<T>(this T[][] array, Func<T, T, int> comparer, long offset, long count)
    {
        if (count == 0L)
        {
            return;
        }
        if (comparer == null)
        {
            return;
        }

        long left = offset;
        long mid = (offset + count) / 2L;
        long right = (offset + count) - 1L;

        for (long i = mid; i >= left; i--)
        {
            array.StorageHeapify(i, left, right, comparer);
        }

        for (long i = right; i >= left; i--)
        {
            array.StorageSwap(i, left);

            array.StorageHeapify(left, left, i - 1L, comparer);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long StorageBinarySearch<T>(this T[][] array, T item, Func<T, T, int> comparer, long offset, long count)
    {
        if (count == 0L)
        {
            return -1L;
        }
        if (comparer == null)
        {
            return -1L;
        }

        if (count < 0L)
        {
            count = array.StorageGetCount(offset);
        }

        long left = offset;
        long right = offset + count - 1L;

        while (right >= left)
        {
            long mid = (right + left) / 2;

            T midItem = array.StorageGet(mid);

            int compareResult = comparer.Invoke(item, midItem);

            // item == midItem
            if (compareResult == 0)
            {
                return mid;
            }

            // item < midItem
            if (compareResult < 0)
            {
                right = mid - 1;
            }
            else // item > midItem
            {
                left = mid + 1;
            }
        }

        return -1L;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StorageCopyTo<T>(this T[][] source, T[][] target, long sourceOffset, long targetOffset, long count)
    {
        long currentCount = 0L;

        while (currentCount < count)
        {
            (long currentSourceStorageIndex, long currentSourceItemIndex) = StorageGetIndex(sourceOffset + currentCount);
            T[] currentSourceArray = source[currentSourceStorageIndex];

            (long currentTargetStorageIndex, long currentTargetItemIndex) = StorageGetIndex(targetOffset + currentCount);
            T[] currentTargetArray = target[currentTargetStorageIndex];

            long bytesToCopyCount = Math.Min(currentSourceArray.LongLength - currentSourceItemIndex, currentTargetArray.LongLength - currentTargetItemIndex);
            bytesToCopyCount = Math.Min(bytesToCopyCount, count - currentCount);

            Array.Copy(currentSourceArray, currentSourceItemIndex, currentTargetArray, currentTargetItemIndex, bytesToCopyCount);

            currentCount += bytesToCopyCount;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StorageCopyTo<T>(this T[][] source, Span<T> target, long sourceOffset, long count)
    {
        long currentCount = 0L;
        long currentTargetItemIndex = 0L;

        while (currentCount < count)
        {
            (long currentSourceStorageIndex, long currentSourceItemIndex) = StorageGetIndex(sourceOffset + currentCount);
            T[] currentSourceArray = source[currentSourceStorageIndex];

            long bytesToCopyCount = Math.Min(currentSourceArray.LongLength - currentSourceItemIndex, target.Length - currentTargetItemIndex);
            bytesToCopyCount = Math.Min(bytesToCopyCount, count - currentCount);

            ReadOnlySpan<T> sourceSpan = currentSourceArray.AsSpan((int)currentSourceItemIndex, (int)bytesToCopyCount);
            Span<T> targetSpan = target.Slice((int)currentTargetItemIndex, (int)bytesToCopyCount);
            sourceSpan.CopyTo(targetSpan);

            currentCount += bytesToCopyCount;
            currentTargetItemIndex += bytesToCopyCount;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StorageCopyFrom<T>(this T[][] target, T[][] source, long sourceOffset, long targetOffset, long count)
    {
        source.StorageCopyTo(target, sourceOffset, targetOffset, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void StorageCopyFrom<T>(this T[][] target, ReadOnlySpan<T> source, long targetOffset, long count)
    {
        long currentCount = 0L;
        long currentSourceItemIndex = 0L;

        while (currentCount < count)
        {
            (long currentTargetStorageIndex, long currentTargetItemIndex) = StorageGetIndex(targetOffset + currentCount);
            T[] currentTargetArray = target[currentTargetStorageIndex];

            long bytesToCopyCount = Math.Min(currentTargetArray.LongLength - currentTargetItemIndex, source.Length - currentSourceItemIndex);
            bytesToCopyCount = Math.Min(bytesToCopyCount, count - currentCount);

            ReadOnlySpan<T> sourceSpan = source.Slice((int)currentSourceItemIndex, (int)bytesToCopyCount);
            Span<T> targetSpan = currentTargetArray.AsSpan((int)currentTargetItemIndex, (int)bytesToCopyCount);
            sourceSpan.CopyTo(targetSpan);

            currentCount += bytesToCopyCount;
            currentSourceItemIndex += bytesToCopyCount;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T[][] StorageResize<T>(this T[][] array, long capacity)
    {
        (long storageCount, long remainder) = StorageGetIndex(capacity);
        storageCount++;

        T[][] result = new T[storageCount][];

        for (long i = 0L; i < storageCount - 1L; i++)
        {
            result[i] = new T[Constants.MaxStorageCapacity];
        }
        result[storageCount - 1L] = new T[remainder];

        long count = StorageGetCount(array);
        long bytesToCopy = Math.Min(count, capacity);

        array.StorageCopyTo(result, 0L, 0L, bytesToCopy);
        return result;
    }
}
