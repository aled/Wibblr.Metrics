using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Npgsql;
using Wibblr.Collections;
using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Plugins.CockroachDb
{
    public class Table
    {
        internal string Name { get; set; }
        internal List<Column> Columns { get; set; }
        internal string PrimaryKey { get; set; }

        private BatchedQueue<object[]> _queue;
        private object _queueLock = new object();

        private string _connectionString;
        private string _databaseName;

        public Table(string connectionString, string databaseName, MetricsWriterSettings writerSettings)
        {
            _connectionString = connectionString;
            _databaseName = databaseName;
            _queue = new BatchedQueue<object[]>(writerSettings.BatchSize, writerSettings.MaxQueuedRows);
        }

        private void ExecuteNonQuery(NpgsqlCommand cmd)
        {
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                cmd.Connection = con;
                cmd.ExecuteNonQuery();
            }
        }

        private void ExecuteNonQuery(string sql)
        {
            using (var cmd = new NpgsqlCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.CommandTimeout = 30;
                ExecuteNonQuery(cmd);
            }
        }

        internal void EnsureExists()
        {
            ExecuteNonQuery($"CREATE TABLE IF NOT EXISTS {_databaseName.SqlQuote()}.{Name.SqlQuote()}\n(\n  {string.Join(",\n  ", Columns)},\n  PRIMARY KEY({PrimaryKey})\n);");
        }
        
        private IEnumerable<string> InsertColumns
        {
            get => Columns
                .Where(c => string.IsNullOrEmpty(c.DefaultFunction))
                .Select(c => c.Name);
        }

        private NpgsqlCommand GetInsertCommand(IEnumerable<object[]> batch)
        {
            var cmd = new NpgsqlCommand();
            var sql = new StringBuilder(1024);

            foreach (var (item, i) in batch.ZipWithIndex())
            {
                if (i == 0)
                    sql.Append($"INSERT INTO {_databaseName.SqlQuote()}.{Name.SqlQuote()} (\n  {string.Join(",\n  ", InsertColumns)}) VALUES\n  ");
                 else
                    sql.Append(",\n  ");

                sql.Append("(");
                for (int j = 0; j < InsertColumns.Count(); j++)
                {
                    if (j > 0)
                        sql.Append(", ");

                    var parameterName = $"@p{j}_{i}";
                    sql.Append(parameterName);
                    cmd.Parameters.AddWithValue(parameterName, item[j]);
                }
                sql.Append(")");
            }
            sql.Append(";");

            cmd.CommandText = sql.ToString();
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 30;

            return cmd; 
        }

        private bool TryDequeueBatch(out List<object[]> batch)
        {
            lock (_queueLock)
                batch = _queue.Count() > 0
                    ? _queue.DequeueBatch()
                    : null;

            return batch != null;
        }

        internal void Insert(IEnumerable<object[]> items)
        {
            lock(_queueLock)
            {
                _queue.Enqueue(items);

                if (_queue.Count() == 0)
                    return;
            }

            List<object[]> batch = null;
            try
            {
                while (TryDequeueBatch(out batch))
                    ExecuteNonQuery(GetInsertCommand(batch));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                if (batch != null)
                    lock (_queueLock)
                        _queue.EnqueueToFront(batch);
            }
        }

        internal IEnumerable<WindowedCounter> Aggregate(IList<string> names, DateTime from, DateTime to, TimeSpan groupBy)
        {
            var groupBySeconds = (int) groupBy.TotalSeconds;
            var counters = new List<WindowedCounter>();
            var nameParameters = Enumerable.Range(0, names.Count()).Select(x => $"@n_{x}").ToList();
            var nameParametersClause = "(" + string.Join(" or ", nameParameters.Select(x => $"countername like {x}")) + ") ";

            var sql = $"select countername, (starttime::date)::timestamp + (((extract(epoch from starttime) % 86400) / @window)::int * @window::int) * interval '1 second' as from, sum(count) as count from {_databaseName.SqlQuote()}.{Name.SqlQuote()} " + 
                $"where starttime >= @from and endtime <= @to and " +
                nameParametersClause +
                "group by 1,2 " +
                "order by 1,2 " +
                "limit 1000;";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@window", groupBySeconds);
                    cmd.Parameters.AddWithValue("@from", from);
                    cmd.Parameters.AddWithValue("@to", to);

                    foreach(var nameParameter in nameParameters.ZipWithIndex())
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
