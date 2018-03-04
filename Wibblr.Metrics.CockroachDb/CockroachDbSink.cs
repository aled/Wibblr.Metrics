using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Npgsql;
using Wibblr.Collections;
using Wibblr.Metrics.Core;

namespace Wibblr.Metrics.CockroachDb
{
    public class CockroachDbSink : IMetricsSink
    {
        private ICockroachDbConfig config;

        private BatchedQueue<WindowedCounter> counterQueue;
        private object counterLock = new object();

        private BatchedQueue<WindowedBucket> histogramQueue;
        private object histogramLock = new object();

        public CockroachDbSink(ICockroachDbConfig config)
        {
            if (!config.IsValid(out var validationErrors))
                throw new ArgumentException($"Invalid config: { validationErrors.Join() }", nameof(config));

            counterQueue = new BatchedQueue<WindowedCounter>(config.BatchSize, config.MaxQueuedRows);
            histogramQueue = new BatchedQueue<WindowedBucket>(config.BatchSize, config.MaxQueuedRows);

            this.config = config;
        }

        public void EnsureTablesExist()
        {
            using (var con = new NpgsqlConnection(config.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText =
                        $"CREATE DATABASE IF NOT EXISTS {config.Database};";

                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        $"CREATE TABLE IF NOT EXISTS {config.Database}.{config.CounterTable} (" +
                        "Id UUID PRIMARY KEY DEFAULT gen_random_uuid(), " +
                        "CounterName VARCHAR(1000), " +
                        "StartTime TIMESTAMP WITHOUT TIME ZONE, " +
                        "EndTime TIMESTAMP WITHOUT TIME ZONE, " +
                        "Count INT);";

                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        $"CREATE TABLE IF NOT EXISTS {config.Database}.{config.HistogramTable} (" +
                        "Id UUID PRIMARY KEY DEFAULT gen_random_uuid(), " +
                        "HistogramName VARCHAR(1000), " +
                        "StartTime TIMESTAMP WITHOUT TIME ZONE, " +
                        "EndTime TIMESTAMP WITHOUT TIME ZONE, " +
                        "BucketFrom INT4, " +
                        "BucketTo INT4, " +
                        "Count INT);";

                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal string BuildCounterSql(IEnumerable<WindowedCounter> batch, NpgsqlCommand cmd)
        {
            var sql = new StringBuilder(1024);

            foreach (var (item, i) in batch.ZipWithIndex())
            {
                if (i == 0)
                    sql.Append("INSERT INTO ")
                       .Append(config.Database)
                       .Append(".")
                       .Append(config.CounterTable)
                       .Append("(CounterName, StartTime, EndTime, Count) VALUES\n");
                else
                    sql.Append(",\n");

                var parameterEventName = $"@counterName_{i:000}";
                var parameterStartTime = $"@startTime_{i:000}";
                var parameterEndTime = $"@endTime_{i:000}";
                var parameterCount = $"@count_{i:000}";

                sql.Append("(")
                   .Append(parameterEventName).Append(", ")
                   .Append(parameterStartTime).Append(", ")
                   .Append(parameterEndTime).Append(", ")
                   .Append(parameterCount).Append(")");

                cmd.Parameters.AddWithValue(parameterEventName, item.name);
                cmd.Parameters.AddWithValue(parameterStartTime, item.window.start);
                cmd.Parameters.AddWithValue(parameterEndTime, item.window.end);
                cmd.Parameters.AddWithValue(parameterCount, item.count);
            }
            sql.Append(";");
            return sql.ToString();
        }

        public void Flush(IEnumerable<WindowedCounter> counters)
        {
            // Any counters that would make the queue go longer than MaxQueuedRows
            // will be discarded.
            lock (counterLock)
            {
                counterQueue.Enqueue(counters);
            }

            // I hope there is a better way to do bulk inserts in cockroach db...
            try
            {
                using (var con = new NpgsqlConnection(config.ConnectionString))
                {
                    con.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 900;

                        List<WindowedCounter> batch = null;
                        lock (counterLock)
                        {
                            if (counterQueue.Count() > 0)
                                batch = counterQueue.DequeueBatch();
                        }

                        if (batch != null)
                        {
                            cmd.CommandText = BuildCounterSql(batch, cmd);
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                lock (counterLock)
                                {
                                    counterQueue.EnqueueToFront(batch);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        internal string BuildHistogramSql(IEnumerable<WindowedBucket> batch, NpgsqlCommand cmd)
        {
            var sql = new StringBuilder(1024);

            foreach (var (item, i) in batch.ZipWithIndex())
            {
                if (i == 0)
                    sql.Append("INSERT INTO ")
                       .Append(config.Database)
                       .Append(".")
                       .Append(config.HistogramTable)
                       .Append("(HistogramName, StartTime, EndTime, BucketFrom, BucketTo, Count) VALUES\n");
                else
                    sql.Append(",\n");

                var parameterHistogramName = $"@histogramName_{i:000}";
                var parameterStartTime = $"@startTime_{i:000}";
                var parameterEndTime = $"@endTime_{i:000}";
                var parameterBucketFrom = $"@bucketFrom_{i:000}";
                var parameterBucketTo = $"@bucketTo_{i:000}";
                var parameterCount = $"@count_{i:000}";

                sql.Append("(")
                   .Append(parameterHistogramName).Append(", ")
                   .Append(parameterStartTime).Append(", ")
                   .Append(parameterEndTime).Append(", ")
                   .Append(parameterBucketFrom).Append(", ")
                   .Append(parameterBucketTo).Append(", ")
                   .Append(parameterCount).Append(")");

                cmd.Parameters.AddWithValue(parameterHistogramName, item.name);
                cmd.Parameters.AddWithValue(parameterStartTime, item.window.start);
                cmd.Parameters.AddWithValue(parameterEndTime, item.window.end);
                cmd.Parameters.AddWithValue(parameterBucketFrom, item.from ?? int.MinValue);
                cmd.Parameters.AddWithValue(parameterBucketTo, item.to ?? int.MaxValue);
                cmd.Parameters.AddWithValue(parameterCount, item.count);
            }
            sql.Append(";");
            return sql.ToString();
        }

        public void Flush(IEnumerable<WindowedBucket> buckets)
        {
            // Any counters that would make the queue go longer than MaxQueuedRows
            // will be discarded.
            lock (histogramLock)
            {
                histogramQueue.Enqueue(buckets);
            }

            // I hope there is a better way to do bulk inserts in cockroach db...
            try
            {
                using (var con = new NpgsqlConnection(config.ConnectionString))
                {
                    con.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 900;

                        List<WindowedBucket> batch = null;
                        lock (histogramLock)
                        {
                            if (histogramQueue.Count() > 0)
                                batch = histogramQueue.DequeueBatch();
                        }

                        if (batch != null)
                        {
                            cmd.CommandText = BuildHistogramSql(batch, cmd);
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                lock (histogramLock)
                                {
                                    histogramQueue.EnqueueToFront(batch);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
