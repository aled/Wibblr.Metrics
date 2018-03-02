using System;
using Wibblr.Metrics.CockroachDb;
using Wibblr.Metrics.Core;

namespace Wibblr.Metrics.Examples
{
    public class CockroachDbExample
    {
        static void Main(string[] args)
        {
            var databaseName = "test";
            var tableName = "MetricsEvent";
            var batchSize = 100;
            var maxQueuedRows = 450;

            var sink = new CockroachDbSink("192.168.0.7", 26257, "root", "", databaseName, tableName, batchSize, maxQueuedRows);

            sink.CreateTableIfNotExists();

            using (var counterCollector = new MetricsCollector(sink, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(2000)))
            {
                Console.WriteLine("Press some keys; enter to exit");
                char key;
                do
                {
                    key = Console.ReadKey(true).KeyChar;

                    // amplify the number of events
                    for (int i = 0; i < 200; i++)
                    {
                        counterCollector.IncrementCounter(key.ToString() + $" ({i})");
                    }

                } while (key.ToString() != Environment.NewLine);
            }
        }
    }
}
