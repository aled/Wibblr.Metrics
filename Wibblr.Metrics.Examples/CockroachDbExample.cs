using Wibblr.Metrics.CockroachDb;

namespace Wibblr.Metrics.Examples
{
    public class CockroachDbExample
    {
        static void Main(string[] args)
        {
            var sink = new CockroachDbSinkBuilder
            {
                Host = "192.168.0.7",
                Port = 26257,
                Username = "root",
                Password = "",
                Database = "test",
                CounterTable = "MetricsCounter",
                HistogramTable = "MetricsHistogram",
                BatchSize = 1000,
                MaxQueuedRows = 10000
            }.Build();

            sink.EnsureTablesExist();

            new KeyPressMonitor().Run(sink);
        }
    }
}
