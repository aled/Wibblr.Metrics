using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

using Npgsql;
using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Plugins.CockroachDb
{
    public class CockroachDbPlugin : IDatabasePlugin
    {
        private DatabaseConnectionSettings _connectionSettings;
        private string _connectionString;

        private Table counterTable;
        private Table histogramTable;
        private Table eventTable;
        private Table profileTable;

        private string Quote(string s) => "\"" + s.Replace("\"", "\\\"") + "\"";

        public string Name => "CockroachDb";

        public string Version => AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString();

        public void Initialize(DatabaseConnectionSettings connectionSettings, DatabaseTablesSettings tables, MetricsWriterSettings writerSettings)
        {
            _connectionSettings = connectionSettings;

            _connectionString = new NpgsqlConnectionStringBuilder
            {
                Host = connectionSettings.Host,
                Port = connectionSettings.Port,
                Username = connectionSettings.Username,
                Password = connectionSettings.Password,
                Database = connectionSettings.Database,
                SslMode = connectionSettings.RequireSsl ? SslMode.Require : SslMode.Prefer,
                RootCertificate = connectionSettings.CaCertFile
            }.ConnectionString;

            counterTable = new Table(writerSettings)
            {
                Name = tables.Counter,
                Columns = new List<Column> {
                    new Column{ Name = "Id", DataType = "UUID", DefaultFunction = "gen_random_uuid()" },
                    new Column{ Name = "CounterName", DataType = "VARCHAR(8000)" },
                    new Column{ Name = "StartTime", DataType = "TIMESTAMP" },
                    new Column{ Name = "EndTime", DataType = "TIMESTAMP" },
                    new Column{ Name = "Count", DataType = "INT" },
                },
                PrimaryKey = "Id",
            };

            histogramTable = new Table(writerSettings)
            {
                Name = tables.Histogram,
                Columns = new List<Column> {
                    new Column{ Name = "Id", DataType = "UUID", DefaultFunction = "gen_random_uuid()" },
                    new Column{ Name = "HistogramName", DataType = "VARCHAR(8000)" },
                    new Column{ Name = "StartTime", DataType = "TIMESTAMP" },
                    new Column{ Name = "EndTime", DataType = "TIMESTAMP" },
                    new Column{ Name = "BucketFrom", DataType = "INT4" },
                    new Column{ Name = "BucketTo", DataType = "INT4" },
                    new Column{ Name = "Count", DataType = "INT" },
                },
                PrimaryKey = "Id",
            };

            eventTable = new Table(writerSettings)
            {
                Name = tables.Event,
                Columns = new List<Column> {
                    new Column{ Name = "Id", DataType = "UUID", DefaultFunction = "gen_random_uuid()" },
                    new Column{ Name = "EventName", DataType = "VARCHAR(8000)" },
                    new Column{ Name = "Timestamp", DataType = "TIMESTAMP" }
                },
                PrimaryKey = "Id",       
            };

            profileTable = new Table(writerSettings)
            {
                Name = tables.Profile,
                Columns = new List<Column> {
                    new Column{ Name = "Id", DataType = "UUID", DefaultFunction = "gen_random_uuid()" },
                    new Column{ Name = "SessionId", DataType = "VARCHAR(8000)" },
                    new Column{ Name = "ProfileName", DataType = "VARCHAR(8000)" },
                    new Column{ Name = "Process", DataType = "INT4" },
                    new Column{ Name = "Thread", DataType = "VARCHAR(200)" },
                    new Column{ Name = "Timestamp", DataType = "TIMESTAMP" },
                    new Column{ Name = "Phase", DataType = "CHAR" },
                },
                PrimaryKey = "Id",
            };

            EnsureTablesExist();
        }

        public void EnsureTablesExist()
        {
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.Text;

                    cmd.CommandText = $"CREATE DATABASE IF NOT EXISTS {Quote(_connectionSettings.Database)};";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = counterTable.CreateTableSql(Quote(_connectionSettings.Database));
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = histogramTable.CreateTableSql(Quote(_connectionSettings.Database));
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = eventTable.CreateTableSql(Quote(_connectionSettings.Database));
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = profileTable.CreateTableSql(Quote(_connectionSettings.Database));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Flush(IEnumerable<WindowedCounter> counters)
        {
            counterTable.Insert(
                Quote(_connectionSettings.Database),
                _connectionString,
                counters.Select(b => new object[] {
                    b.name,
                    b.from,
                    b.to,
                    b.count }));
        }

        public void Flush(IEnumerable<WindowedBucket> buckets)
        {
            histogramTable.Insert(
                Quote(_connectionSettings.Database),
                _connectionString,
                buckets.Select(b => new object[] {
                    b.name,
                    b.timeFrom,
                    b.timeTo,
                    b.valueFrom ?? int.MinValue,
                    b.valueTo ?? int.MaxValue,
                    b.count }));
        }

        public void Flush(IEnumerable<TimestampedEvent> events)
        {
            eventTable.Insert(
                Quote(_connectionSettings.Database), 
                _connectionString, 
                events.Select(e => new object[] { 
                    e.name, 
                    e.timestamp }));
        }

        public void Flush(IEnumerable<Profile> profiles)
        {
            profileTable.Insert(
                Quote(_connectionSettings.Database),
                _connectionString,
                profiles.Select(p => new object[] {
                        p.sessionId,
                        p.name,
                        p.process,
                        p.thread,
                        p.timestamp,
                        p.phase }));
        }

        /// <summary>
        /// Retrieve all counter names that have recorded values between the given timestamps.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IEnumerable<string> GetCounterNames(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public void FlushComplete()
        {
            // no op
        }
    }
}

