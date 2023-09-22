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

using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LargeCollections;

/// <summary>
/// A dictionary-like collection that allows to limit the amount of memory (RAM) in MB that will be used.
/// Any memory requirement that exceeds this amount is automatically swapped out to disk.
/// Additionally it offers multi-threaded operations for performance improvements.
/// If <see cref="long"/>,<see cref="string"/> or <see cref="byte"/>[] is used for <see cref="TValue"/>
/// it will have a performance benefit due to a specialized implementaion.
/// If other types are used functions for serialization and deserialization must be provided so that values can be stored on disk if needed.
/// Type of key is limited to <see cref="long"/>.
/// Items may have a <see cref="BoundingBox"/> which allows spatial queries.
/// </summary>
[DebuggerDisplay("SpatialDiskCache")]
public class SpatialDiskCache<TValue> : DiskCache<long, TValue>, ISpatialDiskCache<TValue>
{
    protected SqliteCommand[] _upsertSpatialIndexCommands;
    protected SqliteParameter[] _upsertSpatialIndexIdParameters;
    protected SqliteParameter[] _upsertSpatialIndexMinXParameters;
    protected SqliteParameter[] _upsertSpatialIndexMaxXParameters;
    protected SqliteParameter[] _upsertSpatialIndexMinYParameters;
    protected SqliteParameter[] _upsertSpatialIndexMaxYParameters;

    protected SqliteCommand[] _querySpatialIndexCommands;
    protected SqliteParameter[] _querySpatialIndexMinXParameters;
    protected SqliteParameter[] _querySpatialIndexMaxXParameters;
    protected SqliteParameter[] _querySpatialIndexMinYParameters;
    protected SqliteParameter[] _querySpatialIndexMaxYParameters;

    public SpatialDiskCache(
        string baseFilePath,
        string fileExtension = DiskCacheConstants.DefaultFileExtension,
        byte degreeOfParallelism = DiskCacheConstants.DefaultDegreeOfParallelism,
        long maxMemorySize = DiskCacheConstants.DefaultMaxMemorySize,
        bool overwriteExistingFiles = true,
        bool deleteFilesOnDispose = true,
        bool isReadOnly = false,
        Func<TValue, byte[]> serializeValueFunction = null,
        Func<byte[], TValue> deserializeValueFunction = null
        )
        : base(
             baseFilePath,
             fileExtension: fileExtension,
             degreeOfParallelism: degreeOfParallelism,
             maxMemorySize: maxMemorySize,
             overwriteExistingFiles: overwriteExistingFiles,
             deleteFilesOnDispose: deleteFilesOnDispose,
             isReadOnly: isReadOnly,
             serializeKeyFunction: null,
             deserializeKeyFunction: null,
             serializeValueFunction: serializeValueFunction,
             deserializeValueFunction: deserializeValueFunction
             )
    {
        RecreateIndexTable(OverwriteExistingFiles);

        _upsertSpatialIndexCommands = new SqliteCommand[DegreeOfParallelism];
        _upsertSpatialIndexIdParameters = new SqliteParameter[DegreeOfParallelism];
        _upsertSpatialIndexMinXParameters = new SqliteParameter[DegreeOfParallelism];
        _upsertSpatialIndexMaxXParameters = new SqliteParameter[DegreeOfParallelism];
        _upsertSpatialIndexMinYParameters = new SqliteParameter[DegreeOfParallelism];
        _upsertSpatialIndexMaxYParameters = new SqliteParameter[DegreeOfParallelism];
        _querySpatialIndexCommands = new SqliteCommand[DegreeOfParallelism];
        _querySpatialIndexMinXParameters = new SqliteParameter[DegreeOfParallelism];
        _querySpatialIndexMaxXParameters = new SqliteParameter[DegreeOfParallelism];
        _querySpatialIndexMinYParameters = new SqliteParameter[DegreeOfParallelism];
        _querySpatialIndexMaxYParameters = new SqliteParameter[DegreeOfParallelism];

        Enumerable.Range(0, DegreeOfParallelism).ToParallel(DegreeOfParallelism, i =>
        {
            _deleteItemCommands[i].CommandText = $"DELETE FROM index_table WHERE id = @id;" + _deleteItemCommands[i].CommandText;
            _deleteItemCommands[i].Prepare();

            _clearItemsCommands[i].CommandText = $"DELETE FROM index_table;" + _clearItemsCommands[i].CommandText;
            _clearItemsCommands[i].Prepare();

            _upsertSpatialIndexCommands[i] = _connections[i].CreateCommand();
            _upsertSpatialIndexCommands[i].CommandText = $"DELETE FROM index_table WHERE id = @id; INSERT INTO index_table VALUES (@id, @min_x, @max_x, @min_y, @max_y)";
            _upsertSpatialIndexIdParameters[i] = new SqliteParameter("@id", SqliteType.Integer);
            _upsertSpatialIndexMinXParameters[i] = new SqliteParameter("@min_x", SqliteType.Real);
            _upsertSpatialIndexMaxXParameters[i] = new SqliteParameter("@max_x", SqliteType.Real);
            _upsertSpatialIndexMinYParameters[i] = new SqliteParameter("@min_y", SqliteType.Real);
            _upsertSpatialIndexMaxYParameters[i] = new SqliteParameter("@max_y", SqliteType.Real);
            _upsertSpatialIndexCommands[i].Parameters.Add(_upsertSpatialIndexIdParameters[i]);
            _upsertSpatialIndexCommands[i].Parameters.Add(_upsertSpatialIndexMinXParameters[i]);
            _upsertSpatialIndexCommands[i].Parameters.Add(_upsertSpatialIndexMaxXParameters[i]);
            _upsertSpatialIndexCommands[i].Parameters.Add(_upsertSpatialIndexMinYParameters[i]);
            _upsertSpatialIndexCommands[i].Parameters.Add(_upsertSpatialIndexMaxYParameters[i]);
            _upsertSpatialIndexCommands[i].Transaction = _transactions[i];
            _upsertSpatialIndexCommands[i].Prepare();

            _querySpatialIndexCommands[i] = _connections[i].CreateCommand();
            _querySpatialIndexCommands[i].CommandText = $"SELECT items.id, items.item FROM index_table, items WHERE index_table.id = items.id"
                + " AND index_table.min_x >= @min_x AND index_table.max_x <= @max_x"
                + " AND index_table.min_y >= @min_y AND index_table.max_y <= @max_y;";
            _querySpatialIndexMinXParameters[i] = new SqliteParameter("@min_x", SqliteType.Real);
            _querySpatialIndexMaxXParameters[i] = new SqliteParameter("@max_x", SqliteType.Real);
            _querySpatialIndexMinYParameters[i] = new SqliteParameter("@min_y", SqliteType.Real);
            _querySpatialIndexMaxYParameters[i] = new SqliteParameter("@max_y", SqliteType.Real);
            _querySpatialIndexCommands[i].Parameters.Add(_querySpatialIndexMinXParameters[i]);
            _querySpatialIndexCommands[i].Parameters.Add(_querySpatialIndexMaxXParameters[i]);
            _querySpatialIndexCommands[i].Parameters.Add(_querySpatialIndexMinYParameters[i]);
            _querySpatialIndexCommands[i].Parameters.Add(_querySpatialIndexMaxYParameters[i]);
            _querySpatialIndexCommands[i].Transaction = _transactions[i];
            _querySpatialIndexCommands[i].Prepare();

        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void RecreateIndexTable(bool overwriteExistingFiles)
    {
        Enumerable.Range(0, DegreeOfParallelism).ToParallel(DegreeOfParallelism, i =>
        {
            using SqliteCommand command = _connections[i].CreateCommand();

            if (overwriteExistingFiles)
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("Modifications are not allowed when opened in read only mode.");
                }

                command.CommandText = $"DROP TABLE IF EXISTS index_table;";
                command.ExecuteNonQuery();
            }
            command.CommandText = $"CREATE VIRTUAL TABLE IF NOT EXISTS index_table USING rtree(id, min_x, max_x, min_y, max_y);";
            command.ExecuteNonQuery();

        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(long key, TValue value, BoundingBox boundingBox)
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Modifications are not allowed when opened in read only mode.");
        }
        if (boundingBox.MinX > boundingBox.MaxX)
        {
            throw new Exception("Minimum X of bounding box is greater than maximum X.");
        }
        if (boundingBox.MinY > boundingBox.MaxY)
        {
            throw new Exception("Minimum Y of bounding box is greater than maximum Y.");
        }

        int index = GetParallelIndex(key, DegreeOfParallelism);
        lock (_connections[index])
        {
            base.Set(key, value);

            _upsertSpatialIndexIdParameters[index].Value = key;
            _upsertSpatialIndexMinXParameters[index].Value = boundingBox.MinX;
            _upsertSpatialIndexMaxXParameters[index].Value = boundingBox.MaxX;
            _upsertSpatialIndexMinYParameters[index].Value = boundingBox.MinY;
            _upsertSpatialIndexMaxYParameters[index].Value = boundingBox.MaxY;
            _upsertSpatialIndexCommands[index].ExecuteNonQuery();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(KeyValuePair<long, TValue> item, BoundingBox boundingBox)
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Modifications are not allowed when opened in read only mode.");
        }

        Set(item.Key, item.Value, boundingBox);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(IEnumerable<(KeyValuePair<long, TValue>, BoundingBox boundingBox)> items)
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Modifications are not allowed when opened in read only mode.");
        }

        foreach ((KeyValuePair<long, TValue> item, BoundingBox boundingBox) in items)
        {
            Set(item.Key, item.Value, boundingBox);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddParallel(IEnumerable<(KeyValuePair<long, TValue> item, BoundingBox boundingBox)>[] parallelItems)
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Modifications are not allowed when opened in read only mode.");
        }
        if (parallelItems == null)
        {
            return;
        }

        parallelItems.DoParallel(item => Set(item.item.Key, item.item.Value, item.boundingBox));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Clear()
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Modifications are not allowed when opened in read only mode.");
        }

        RecreateTable(true);
        RecreateIndexTable(true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected IEnumerable<KeyValuePair<long, TValue>> QueryInternal(SqliteConnection connection,
        SqliteCommand querySpatialIndexCommand,
        SqliteParameter querySpatialIndexMinXParameter,
        SqliteParameter querySpatialIndexMaxXParameter,
        SqliteParameter querySpatialIndexMinYParameter,
        SqliteParameter querySpatialIndexMaxYParameter,
        BoundingBox boundingBox)
    {
        if (connection == null)
        {
            yield break;
        }
        if (querySpatialIndexCommand == null)
        {
            yield break;
        }
        if (querySpatialIndexMinXParameter == null)
        {
            yield break;
        }
        if (querySpatialIndexMaxXParameter == null)
        {
            yield break;
        }
        if (querySpatialIndexMinYParameter == null)
        {
            yield break;
        }
        if (querySpatialIndexMaxYParameter == null)
        {
            yield break;
        }

        lock (connection)
        {
            querySpatialIndexMinXParameter.Value = boundingBox.MinX;
            querySpatialIndexMaxXParameter.Value = boundingBox.MaxX;
            querySpatialIndexMinYParameter.Value = boundingBox.MinY;
            querySpatialIndexMaxYParameter.Value = boundingBox.MaxY;


            using SqliteDataReader reader = querySpatialIndexCommand.ExecuteReader();

            while (reader.Read())
            {
                long key = (long)reader.GetValue(0);

                TValue value = default(TValue);
                if (typeof(TValue) == typeof(long) || typeof(TValue) == typeof(string) || typeof(TValue) == typeof(byte[]) || typeof(TValue) == typeof(double))
                {
                    value = (TValue)reader.GetValue(1);
                }
                else
                {
                    if (_deserializeValueFunction == null)
                    {
                        continue;
                    }

                    byte[] serializedValue = (byte[])reader.GetValue(1);

                    value = _deserializeValueFunction(serializedValue);
                }

                yield return new KeyValuePair<long, TValue>(key, value);
            }

        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<KeyValuePair<long, TValue>> Query(BoundingBox boundingBox)
    {
        for (byte i = 0; i < DegreeOfParallelism; i++)
        {
            foreach (KeyValuePair<long, TValue> item in QueryInternal(_connections[i],
                _querySpatialIndexCommands[i],
                _querySpatialIndexMinXParameters[i],
                _querySpatialIndexMaxXParameters[i],
                _querySpatialIndexMinYParameters[i],
                _querySpatialIndexMaxYParameters[i],
                boundingBox))
            {
                yield return item;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<KeyValuePair<long, TValue>>[] QueryParallel(BoundingBox boundingBox)
    {
        IEnumerable<KeyValuePair<long, TValue>>[] parallelItems = new IEnumerable<KeyValuePair<long, TValue>>[DegreeOfParallelism];
        for (int i = 0; i < DegreeOfParallelism; i++)
        {
            parallelItems[i] = QueryInternal(_connections[i],
                _querySpatialIndexCommands[i],
                _querySpatialIndexMinXParameters[i],
                _querySpatialIndexMaxXParameters[i],
                _querySpatialIndexMinYParameters[i],
                _querySpatialIndexMaxYParameters[i],
                boundingBox);
        }

        return parallelItems;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Dispose()
    {
        for (int i = 0; i < DegreeOfParallelism; i++)
        {
            lock (_connections[i])
            {
                if (_upsertSpatialIndexCommands != null && _upsertSpatialIndexCommands[i] != null)
                {
                    _upsertSpatialIndexCommands[i].Dispose();
                    _upsertSpatialIndexCommands[i] = null;
                }

                if (_querySpatialIndexCommands != null && _querySpatialIndexCommands[i] != null)
                {
                    _querySpatialIndexCommands[i].Dispose();
                    _querySpatialIndexCommands[i] = null;
                }
            }
        };

        _upsertSpatialIndexCommands = null;
        _querySpatialIndexCommands = null;

        base.Dispose();
    }
}
