using System;
using System.Diagnostics;
using Wibblr.Metrics.CockroachDb;
using Wibblr.Metrics.Core;

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

            sink.CreateTableIfNotExists();

            using (var metrics = new MetricsCollector(sink, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)))
            {
                metrics.RegisterThresholds("latency", new int[] {0, 50, 75, 100, 1000, 2000, 10000});
                Console.WriteLine("Press some keys; enter to exit");
                char key;
                var stopwatch = new Stopwatch();
                    
                do
                {
                    stopwatch.Start();
                    key = Console.ReadKey(true).KeyChar;
                    stopwatch.Stop();

                    metrics.IncrementCounter(key.ToString());
                    metrics.IncrementBucket("latency", stopwatch.ElapsedMilliseconds);

                    stopwatch.Reset();

                } while (key.ToString() != Environment.NewLine);
            }
        }
    }
}
