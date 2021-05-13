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
        public string Name { get; set; }
        public List<Column> Columns { get; set; }
        public string PrimaryKey { get; set; }

        private BatchedQueue<object[]> queue;
        private object queueLock = new object();

        public Table(MetricsWriterSettings writerSettings)
        {
            queue = new BatchedQueue<object[]>(writerSettings.BatchSize, writerSettings.MaxQueuedRows);
        }

        public string CreateTableSql(string database) =>
            $"CREATE TABLE IF NOT EXISTS {database}.{Name}\n(\n  {string.Join(",\n  ", Columns)},\n  PRIMARY KEY({PrimaryKey})\n);";

        private IEnumerable<string> InsertColumns
        {
            get => Columns
                .Where(c => string.IsNullOrEmpty(c.DefaultFunction))
                .Select(c => c.Name);
        }

        public NpgsqlCommand GetInsertCommand(string database, IEnumerable<object[]> batch)
        {
            var cmd = new NpgsqlCommand();
            var sql = new StringBuilder(1024);

            foreach (var (item, i) in batch.ZipWithIndex())
            {
                if (i == 0)
                    sql.Append($"INSERT INTO {database}.{Name} (\n  {string.Join(",\n  ", InsertColumns)}) VALUES\n  ");
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
            cmd.CommandTimeout = 900;

            return cmd; 
        }

        public void Insert(string database, string connectionString, IEnumerable<object[]> items)
        {
            lock(queueLock)
            {
                queue.Enqueue(items);
                if (queue.Count() == 0)
                    return;
            }
               
            try
            {
                using (var con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    List<object[]> batch;
                    do
                    {
                        batch = null;
                        
                        lock (queueLock)
                        {
                            if (queue.Count() > 0)
                                batch = queue.DequeueBatch();
                        }

                        try
                        {
                            if (batch != null)
                            {
                                using (var cmd = GetInsertCommand(database, batch))
                                {
                                    cmd.Connection = con;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            lock (queueLock)
                            {
                                queue.EnqueueToFront(batch);
                            }
                            break;
                        }
                    } while (batch != null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
