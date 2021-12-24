using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Plugins.SqlServer
{
    public class SqlServerPlugin : IDatabasePlugin
    {        
        public string Name => "SqlServer";

        public string Version => AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString();

        private DatabaseConnectionSettings _connectionSettings;

        private Table counterTable;
        private Table histogramTable;
        private Table eventTable;
        private Table profileTable;

        public void Initialize(DatabaseConnectionSettings connectionSettings, DatabaseTablesSettings tables, MetricsWriterSettings writerSettings)
        {
            _connectionSettings = connectionSettings;

            counterTable = new Table(_connectionSettings.ConnectionString, writerSettings)
            {
                Name = tables.Counter,
                Columns = new List<Column> {
                    new Column("Id", "BIGINT", identity: true),
                    new Column("CounterName", "VARCHAR(8000)", typeof(string)),
                    new Column("StartTime", "DATETIME2(7)", typeof(DateTime)),
                    new Column("EndTime", "DATETIME2(7)", typeof(DateTime)),
                    new Column("Count", "BIGINT", typeof(long))
                },         
                PrimaryKey = "Id",
            };

            histogramTable = new Table(_connectionSettings.ConnectionString, writerSettings)
            {
                Name = tables.Histogram,
                    Columns = new List<Column> {
                        new Column("Id", "BIGINT", identity: true),
                        new Column("HistogramName", "VARCHAR(8000)", typeof(string)),
                        new Column("StartTime", "DATETIME2(7)", typeof(DateTime)),
                        new Column("EndTime", "DATETIME2(7)", typeof(DateTime)),
                        new Column("BucketFrom", "INT", typeof(int)),
                        new Column("BucketTo", "INT", typeof(int)),
                        new Column("Count", "BIGINT", typeof(long))
                    },
                    PrimaryKey = "Id",
                };

            eventTable = new Table(_connectionSettings.ConnectionString, writerSettings)
            {
                Name = tables.Event,
                    Columns = new List<Column> {
                        new Column("Id", "BIGINT", identity: true),
                        new Column("EventName", "VARCHAR(8000)", typeof(string)),
                        new Column("Timestamp", "DATETIME2(7)", typeof(DateTime))
                    },
                    PrimaryKey = "Id",       
                };

            profileTable = new Table(_connectionSettings.ConnectionString, writerSettings)
            {
                Name = tables.Profile,
                    Columns = new List<Column> {
                        new Column("Id", "BIGINT", identity: true),
                        new Column("SessionId", "VARCHAR(8000)", typeof(string)),
                        new Column("ProfileName", "VARCHAR(8000)", typeof(string)),
                        new Column("Process", "INT", typeof(int)),
                        new Column("Thread", "VARCHAR(200)", typeof(string)),
                        new Column("Timestamp", "DATETIME2(7)", typeof(DateTime)),
                        new Column("Phase", "CHAR", typeof(string)),
                    },
                    PrimaryKey = "Id",
                };

            counterTable.Initialize();
            histogramTable.Initialize();
            eventTable.Initialize();
            profileTable.Initialize();
        }

        public void Flush(IEnumerable<WindowedCounter> counters)
        {
            counterTable.Insert(counters.Select(c => new object[] {
                c.name, 
                c.from, 
                c.to, 
                c.count
            }));
        }

        public void Flush(IEnumerable<WindowedBucket> buckets)
        {
            histogramTable.Insert(buckets.Select(b => new object[] {
                b.name,
                b.timeFrom,
                b.timeTo,
                b.valueFrom ?? int.MinValue,
                b.valueTo ?? int.MaxValue,
                b.count
            }));
        }

        public void Flush(IEnumerable<TimestampedEvent> events)
        {
            eventTable.Insert(events.Select(e => new object[] {
                e.name,
                e.timestamp
            }));
        }

        public void Flush(IEnumerable<Profile> profiles)
        {
            profileTable.Insert(profiles.Select(p => new object[] {
                p.sessionId,
                p.name,
                p.process,
                p.thread,
                p.timestamp,
                p.phase 
            }));
        }

        public IEnumerable<string> GetCounterNames(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public void FlushComplete()
        {
            // no op
        }

        public IEnumerable<WindowedCounter> GetAggregatedCounters(IList<string> names, DateTime from, DateTime to, TimeSpan groupBy)
        {
            return counterTable.Aggregate(names, from, to, groupBy);
        }
    }
}
