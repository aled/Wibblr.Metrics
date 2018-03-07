using System;
using Wibblr.Metrics.Core;
using Wibblr.Metrics.SqlServer;

namespace Wibblr.Metrics.Examples
{
    class SqlServerExample
    {
        static void Main(string[] args)
        {
            var database = "Metrics";

            var config = new SqlServerConfig
            {
                ConnectionString = $"Data Source=(local);Initial Catalog={database};Integrated Security=SSPI",
                BatchSize = 5000,
                MaxQueuedRows = 20000,
                Database = database,
                CounterTable = "Counter",
                EventTable = "Event",
                HistogramTable = "Histogram"
            };

            var sink = new SqlServerSink(config);
            sink.EnsureTablesExist();

            new KeyPressMonitor().Run(sink);
        }
    }
}
