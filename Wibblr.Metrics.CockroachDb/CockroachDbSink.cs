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
        private string connectionString;
        private string tableName;
        private string database;
        private BatchedQueue<AggregatedCounter> queue;
        private object queueLock = new object();

        public CockroachDbSink(string host, int port, string username, string password, string database, string tableName, int batchSize, int maxQueuedRows)
        {
            connectionString = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = port,
                Username = username,
                Password = password,
                Database = database
            }.ConnectionString;

            // only allow letters and digits in the table name
            if (tableName.Where(c => !char.IsLetterOrDigit(c)).Any())
                throw new ArgumentException("Table name must be letters and digits only", tableName);

            this.tableName = tableName;
            this.database = database;

            queue = new BatchedQueue<AggregatedCounter>(batchSize, maxQueuedRows);
        }

        public void CreateTableIfNotExists()
        {
            using (var con = new NpgsqlConnection(connectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = $"CREATE DATABASE IF NOT EXISTS {database}; CREATE TABLE IF NOT EXISTS {tableName} (EventName VARCHAR(200), StartTime TIMESTAMP WITHOUT TIME ZONE, EndTime TIMESTAMP WITHOUT TIME ZONE, Count BIGINT);";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal string BuildSql(IEnumerable<AggregatedCounter> batch, NpgsqlCommand cmd)
        {
            var sql = new StringBuilder(1024);

            foreach (var (item, i) in batch.ZipWithIndex())
            {
                if (i == 0)
                    sql.Append("INSERT INTO ")
                       .Append(database)
                       .Append(".")
                       .Append(tableName)
                       .Append("(EventName, StartTime, EndTime, Count) VALUES\n");
                else
                    sql.Append(",\n");

                var parameterEventName = $"@eventName_{i:000}";
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

        public void Flush(IEnumerable<AggregatedCounter> counters)
        {
            // Any counters that would make the queue go longer than MaxQueuedRows
            // will be discarded.
            lock (queueLock)
            {
                queue.Enqueue(counters);
            }

            // I hope there is a better way to do bulk inserts in cockroach db...
            try
            {
                using (var con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 900;

                        List<AggregatedCounter> batch = null;
                        lock(queueLock)
                        {
                            if (queue.Count() > 0)
                                batch = queue.DequeueBatch();
                        }

                        if (batch != null)
                        {
                            cmd.CommandText = BuildSql(batch, cmd);
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (NpgsqlException)
                            {
                                lock (queueLock)
                                {
                                    queue.EnqueueToFront(batch);
                                }
                            }
                        }
                    }
                }
            } 
            catch (NpgsqlException e)
            {
                
            }
        }
    }
}
