using System.IO;
using System.Text.Json;
using Wibblr.Metrics.CockroachDb;

namespace Wibblr.Metrics.Examples
{
    public class CockroachDbExample
    {
        static void Main(string[] args)
        {
            var sinkBuilder = JsonSerializer.Deserialize<CockroachDbSinkBuilder>(File.ReadAllText("CockroachDb-connection.json"));

            sinkBuilder.CounterTable = "MetricsCounter";
            sinkBuilder.HistogramTable = "MetricsHistogram";
            sinkBuilder.EventTable = "MetricsEvent";
            sinkBuilder.ProfileTable = "MetricsProfile";
            sinkBuilder.BatchSize = 1000;
            sinkBuilder.MaxQueuedRows = 10000;

            var sink = sinkBuilder.Build();

            sink.EnsureTablesExist();

            new KeyPressMonitor().Run(sink);
        }
    }
}
