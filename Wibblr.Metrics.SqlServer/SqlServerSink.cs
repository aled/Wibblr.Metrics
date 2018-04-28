using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Wibblr.Metrics.Core;

namespace Wibblr.Metrics.SqlServer
{
    // TODO: refactor out common code in various Flush() methods

    public class SqlServerSink : IMetricsSink
    {
        private IDatabaseConfig config;

        const int TIMEOUT_SECONDS = 60;
        
        internal DataTable counterDataTable;
        private object counterDataTableLock = new object();

        internal DataTable histogramDataTable;
        private object histogramDataTableLock = new object();

        internal DataTable eventDataTable;
        private object eventDataTableLock = new object();

        public SqlServerSink(IDatabaseConfig config)
        {
            this.config = config;

            counterDataTable = new DataTable();
            counterDataTable.Columns.Add("CounterName", typeof(string));
            counterDataTable.Columns.Add("StartTime", typeof(DateTime));
            counterDataTable.Columns.Add("EndTime", typeof(DateTime));
            counterDataTable.Columns.Add("Count", typeof(long));

            histogramDataTable = new DataTable();
            histogramDataTable.Columns.Add("HistogramName", typeof(string));
            histogramDataTable.Columns.Add("StartTime", typeof(DateTime));
            histogramDataTable.Columns.Add("EndTime", typeof(DateTime));
            histogramDataTable.Columns.Add("BucketFrom", typeof(int));
            histogramDataTable.Columns.Add("BucketTo", typeof(int));
            histogramDataTable.Columns.Add("Count", typeof(long));

            eventDataTable = new DataTable();
            eventDataTable.Columns.Add("EventName", typeof(string));
            eventDataTable.Columns.Add("Timestamp", typeof(DateTime));
        }

        private void ExecuteSql(string sql)
        {
            try 
            {
                using (var con = new SqlConnection(config.ConnectionString))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.CommandTimeout = 30;
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void EnsureTablesExist()
        {
            ExecuteSql($"CREATE TABLE {config.CounterTable} (Id BIGINT IDENTITY, CounterName VARCHAR(8000) NOT NULL, StartTime DATETIME2(7) NOT NULL, EndTime DATETIME2(7) NOT NULL, Count BIGINT NOT NULL, PRIMARY KEY(Id));");
            ExecuteSql($"CREATE TABLE {config.HistogramTable} (Id BIGINT IDENTITY, HistogramName VARCHAR(8000) NOT NULL, StartTime DATETIME2(7) NOT NULL, EndTime DATETIME2(7) NOT NULL, BucketFrom INT NOT NULL, BucketTo INT NOT NULL, Count BIGINT NOT NULL, PRIMARY KEY(Id));");
            ExecuteSql($"CREATE TABLE {config.EventTable} (Id BIGINT IDENTITY, EventName VARCHAR(8000) NOT NULL, Timestamp DATETIME2(7) NOT NULL, PRIMARY KEY(Id));");
        }

        public void Flush(IEnumerable<WindowedCounter> counters)
        {
            lock (counterDataTableLock)
            {
                foreach (var c in counters)
                {
                    // Throw away new rows when we have too many queued, so that we
                    // don't consume unlimited memory if the database is down.
                    // TODO: aggregate them into coarser time periods instead.
                    if (counterDataTable.Rows.Count >= config.MaxQueuedRows)
                        break;

                    var row = counterDataTable.NewRow();
                    row["CounterName"] = c.name;
                    row["StartTime"] = c.window.start;
                    row["EndTime"] = c.window.start.Add(c.window.size);
                    row["Count"] = c.count;
                    counterDataTable.Rows.Add(row);
                }

                try
                {
                    using (var bc = new SqlBulkCopy(config.ConnectionString, SqlBulkCopyOptions.UseInternalTransaction))
                    {
                        bc.DestinationTableName = config.CounterTable;
                        bc.BatchSize = config.BatchSize;
                        bc.BulkCopyTimeout = TIMEOUT_SECONDS;
                        bc.EnableStreaming = false;
                        bc.ColumnMappings.Add("CounterName", "CounterName");
                        bc.ColumnMappings.Add("StartTime", "StartTime");
                        bc.ColumnMappings.Add("EndTime", "EndTime");
                        bc.ColumnMappings.Add("Count", "Count");
                        bc.WriteToServer(counterDataTable);
                    }
                    counterDataTable.Clear();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void Flush(IEnumerable<WindowedBucket> buckets)
        {
            try
            {
                lock (histogramDataTableLock)
                {
                    foreach (var b in buckets)
                    {
                        // Throw away new rows when we have too many queued, so that we
                        // don't consume unlimited memory if the database is down.
                        // TODO: aggregate them into coarser time periods instead.
                        if (histogramDataTable.Rows.Count >= config.MaxQueuedRows)
                            break;

                        var row = histogramDataTable.NewRow();
                        row["HistogramName"] = b.name;
                        row["StartTime"] = b.window.start;
                        row["EndTime"] = b.window.end;
                        row["BucketFrom"] = b.from ?? int.MinValue;
                        row["BucketTo"] = b.to ?? int.MaxValue;
                        row["Count"] = b.count;
                        histogramDataTable.Rows.Add(row);
                    }

                    using (var bc = new SqlBulkCopy(config.ConnectionString, SqlBulkCopyOptions.UseInternalTransaction))
                    {
                        bc.DestinationTableName = config.HistogramTable;
                        bc.BatchSize = config.BatchSize;
                        bc.BulkCopyTimeout = TIMEOUT_SECONDS;
                        bc.EnableStreaming = false;
                        bc.ColumnMappings.Add("HistogramName", "HistogramName");
                        bc.ColumnMappings.Add("StartTime", "StartTime");
                        bc.ColumnMappings.Add("EndTime", "EndTime");
                        bc.ColumnMappings.Add("BucketFrom", "BucketFrom");
                        bc.ColumnMappings.Add("BucketTo", "BucketTo");
                        bc.ColumnMappings.Add("Count", "Count");
                        bc.WriteToServer(histogramDataTable);
                    }
                    histogramDataTable.Clear();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Flush(IEnumerable<TimestampedEvent> events)
        {
            try
            {
                lock (eventDataTableLock)
                {
                    foreach (var e in events)
                    {
                        // Throw away new rows when we have too many queued, so that we
                        // don't consume unlimited memory if the database is down.
                        // TODO: aggregate them into coarser time periods instead.
                        if (eventDataTable.Rows.Count >= config.MaxQueuedRows)
                            break;

                        var row = eventDataTable.NewRow();
                        row["EventName"] = e.name;
                        row["Timestamp"] = e.timestamp;
                        eventDataTable.Rows.Add(row);
                    }

                    using (var bc = new SqlBulkCopy(config.ConnectionString, SqlBulkCopyOptions.UseInternalTransaction))
                    {
                        bc.DestinationTableName = config.EventTable;
                        bc.BatchSize = config.BatchSize;
                        bc.BulkCopyTimeout = TIMEOUT_SECONDS;
                        bc.EnableStreaming = false;
                        bc.ColumnMappings.Add("EventName", "EventName");
                        bc.ColumnMappings.Add("Timestamp", "Timestamp");
                        bc.WriteToServer(eventDataTable);
                    }
                    eventDataTable.Clear();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Flush(IEnumerable<Profile> profiles)
        {
            throw new NotImplementedException();
        }
    }
}
