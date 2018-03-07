using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Npgsql;
using Wibblr.Metrics.Core;

namespace Wibblr.Metrics.CockroachDb
{
    public class CockroachDbSink : IMetricsSink
    {
        private IDatabaseConfig config;

        private Table counterTable;
        private Table histogramTable;
        private Table eventTable;

        public CockroachDbSink(IDatabaseConfig config)
        {
            if (!config.IsValid(out var validationErrors))
                throw new ArgumentException($"Invalid config: { validationErrors.Join() }", nameof(config));

            counterTable = new Table(config.BatchSize, config.MaxQueuedRows)
            {
                Name = config.CounterTable,
                Columns = new Column[] {
                    new Column{ Name = "Id", DataType = "UUID", DefaultFunction = "gen_random_uuid()" },
                    new Column{ Name = "CounterName", DataType = "VARCHAR(1000)" },
                    new Column{ Name = "StartTime", DataType = "TIMESTAMP" },
                    new Column{ Name = "EndTime", DataType = "TIMESTAMP" },
                    new Column{ Name = "Count", DataType = "INT" },
                },
                PrimaryKey = "Id",
            };

            histogramTable = new Table(config.BatchSize, config.MaxQueuedRows)
            {
                Name = config.HistogramTable,
                Columns = new Column[] {
                    new Column{ Name = "Id", DataType = "UUID", DefaultFunction = "gen_random_uuid()" },
                    new Column{ Name = "HistogramName", DataType = "VARCHAR(1000)" },
                    new Column{ Name = "StartTime", DataType = "TIMESTAMP" },
                    new Column{ Name = "EndTime", DataType = "TIMESTAMP" },
                    new Column{ Name = "BucketFrom", DataType = "INT4" },
                    new Column{ Name = "BucketTo", DataType = "INT4" },
                    new Column{ Name = "Count", DataType = "INT" },
                },
                PrimaryKey = "Id",
            };

            eventTable = new Table(config.BatchSize, config.MaxQueuedRows)
            {
                Name = config.EventTable,
                Columns = new Column[] {
                    new Column{ Name = "Id", DataType = "UUID", DefaultFunction = "gen_random_uuid()" },
                    new Column{ Name = "EventName", DataType = "VARCHAR(1000)" },
                    new Column{ Name = "Timestamp", DataType = "TIMESTAMP" }
                },
                PrimaryKey = "Id",       
            };

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
                    cmd.CommandText = $"CREATE DATABASE IF NOT EXISTS {config.Database};";

                    cmd.ExecuteNonQuery();

                    cmd.CommandText = counterTable.CreateTableSql(config.Database);
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = histogramTable.CreateTableSql(config.Database);
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = eventTable.CreateTableSql(config.Database);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Flush(IEnumerable<WindowedCounter> counters)
        {
            counterTable.Insert(
                config.Database,
                config.ConnectionString,
                counters.Select(b => new object[] {
                    b.name,
                    b.window.start,
                    b.window.end,
                    b.count }));
        }

        public void Flush(IEnumerable<WindowedBucket> buckets)
        {
            histogramTable.Insert(
                config.Database,
                config.ConnectionString,
                buckets.Select(b => new object[] {
                    b.name,
                    b.window.start,
                    b.window.end,
                    b.from ?? int.MinValue,
                    b.to ?? int.MaxValue,
                    b.count }));
        }

        public void Flush(IEnumerable<TimestampedEvent> events)
        {
            eventTable.Insert(
                config.Database, 
                config.ConnectionString, 
                events.Select(e => new object[] { 
                    e.name, 
                    e.timestamp }));
        }
    }
}

