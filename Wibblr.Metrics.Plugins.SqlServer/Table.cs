using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using Wibblr.Metrics.Plugins.Interfaces;
using Wibblr.Utils;

namespace Wibblr.Metrics.Plugins.SqlServer
{
    public class Table
    {
        private static readonly int TIMEOUT_SECONDS = 60;

        internal string Name { get; set; }
        internal List<Column> Columns { get; set; }
        internal string PrimaryKey { get; set; }

        private string _connectionString;
        private MetricsWriterSettings _writerSettings;
        private string[] _columnsToInsert;

        private DataTable _dataTable;
        private object _lock = new object();

        public Table(string connectionString, MetricsWriterSettings writerSettings)
        {
            _connectionString = connectionString;
            _writerSettings = writerSettings;
            _dataTable = new DataTable();
        }

        private void ExecuteNonQuery(SqlCommand cmd, params SqlParameter[] parameters)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                cmd.Connection = con;
                
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);

                cmd.ExecuteNonQuery();
            }
        }

        private void ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (var cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.CommandTimeout = 30;
                ExecuteNonQuery(cmd, parameters);
            }
        }

        internal void EnsureExists()
        {
            ExecuteNonQuery($"IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @Name AND TABLE_TYPE = 'BASE TABLE')\nCREATE TABLE { Name.SqlQuote()} \n(\n  {string.Join(",\n  ", Columns)},\n  PRIMARY KEY({PrimaryKey})\n);", new SqlParameter("@Name", Name));
        }

        internal void Initialize()
        {
            EnsureExists();

            _columnsToInsert = Columns
                .Where(c => !c.Identity)
                .Select(c => c.Name)
                .ToArray();

            foreach (var c in Columns.Where(c => !c.Identity))
                _dataTable.Columns.Add(c.Name, c.Type);
        }

        public void Insert(IEnumerable<object[]> items)
        {
            lock (_lock)
            {
                foreach (var item in items)
                {
                    // Throw away new rows when we have too many queued, so that we
                    // don't consume unlimited memory if the database is down.
                    // TODO: aggregate them into coarser time periods instead.
                    if (_dataTable.Rows.Count >= _writerSettings.MaxQueuedRows)
                        break;

                    var row = _dataTable.NewRow();
                    for (int i = 0; i < _columnsToInsert.Length; i++)
                    {
                        var c = _columnsToInsert[i];
                        row[c] = item[i];
                    }
                    _dataTable.Rows.Add(row);
                }

                try
                {
                    using (var bc = new SqlBulkCopy(_connectionString, SqlBulkCopyOptions.UseInternalTransaction))
                    {
                        bc.DestinationTableName = Name;
                        bc.BatchSize = _writerSettings.BatchSize;
                        bc.BulkCopyTimeout = TIMEOUT_SECONDS;
                        bc.EnableStreaming = false;
                        
                        foreach (var c in _columnsToInsert)
                            bc.ColumnMappings.Add(c, c);
                        
                        bc.WriteToServer(_dataTable);
                    }
                    _dataTable.Clear();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        internal IEnumerable<WindowedCounter> Aggregate(IList<string> names, DateTime from, DateTime to, TimeSpan groupBy)
        {
            var groupBySeconds = (int)groupBy.TotalSeconds;
            var counters = new List<WindowedCounter>();
            var nameParameters = Enumerable.Range(0, names.Count()).Select(x => $"@n_{x}").ToList();
            var nameParametersClause = "(" + string.Join(" or ", nameParameters.Select(x => $"countername like {x}")) + ") ";

            var sql = ";with t as (select countername, count, dateadd(second, convert(int, datediff(second, convert(date, starttime), starttime) / @window) * @window, convert(datetime, convert(date, starttime))) as [from] " +
                      "from MetricsCounter where startTime >= @from and endtime <= @to AND " + nameParametersClause + ") " +
                      "select top 10000 countername, [from], sum(count) as count from t " +
                      "group by CounterName, [from] " +
                      "order by CounterName, [from] ";
            
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@window", groupBySeconds);
                    cmd.Parameters.AddWithValue("@from", from);
                    cmd.Parameters.AddWithValue("@to", to);

                    foreach (var nameParameter in nameParameters.ZipWithIndex())
                        cmd.Parameters.AddWithValue(nameParameter.Item1, names[nameParameter.Item2]);

                    cmd.CommandType = CommandType.Text;

                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                            counters.Add(new WindowedCounter { name = (string)rdr["countername"], from = (DateTime)rdr["from"], to = ((DateTime)rdr["from"]).Add(groupBy), count = Convert.ToInt64(rdr["count"]) });
                    }
                }
            }
            return counters;
        }
    }
}
