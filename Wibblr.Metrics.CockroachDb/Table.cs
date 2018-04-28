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
    public class Table
    {
        public string Name { get; set; }
        public Column[] Columns { get; set; }
        public string PrimaryKey { get; set; }

        private int batchSize;
        private int maxQueuedRows;

        private BatchedQueue<object[]> queue;
        private object queueLock = new object();

        public Table(int batchSize, int maxQueuedRows)
        {
            this.batchSize = batchSize;
            this.maxQueuedRows = maxQueuedRows;
            queue = new BatchedQueue<object[]>(batchSize, maxQueuedRows);
        }

        public string CreateTableSql(string database)
        {
            var sb = new StringBuilder(1024);
            sb.AppendLine($"CREATE TABLE IF NOT EXISTS {database}.{Name} (");

            foreach (var c in Columns)
                sb.Append($"\n  {c},");

            sb.Append($"\n  PRIMARY KEY({PrimaryKey})");
            sb.Append("\n);");

            return sb.ToString();
        }

        private IEnumerable<string> InsertColumns
        {
            get => Columns.Where(c => string.IsNullOrEmpty(c.DefaultFunction))
                          .Select(c => c.Name);
        }

        private string InsertColumnsClause
        {
            get => string.Join(", ", InsertColumns);
        }

        public NpgsqlCommand GetInsertCommand(string database, IEnumerable<object[]> batch)
        {
            var cmd = new NpgsqlCommand();
            var sql = new StringBuilder(1024);

            foreach (var (item, i) in batch.ZipWithIndex())
            {
                if (i == 0)
                    sql.Append($"INSERT INTO {database}.{Name} ({InsertColumnsClause}) VALUES\n");
                 else
                    sql.Append(",\n");

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
