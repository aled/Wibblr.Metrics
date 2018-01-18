using System;
using Wibblr.Metrics.Core;
using Wibblr.Metrics.SqlServer;

namespace Wibblr.Metrics.Examples
{
    class SqlServerExample
    {
        static void Main(string[] args)
        {
            var connectionString = "Data Source=(local)\\SQLEXPRESS;Initial Catalog=Test;Integrated Security=SSPI";
            var tableName = "MetricsEvent";

            var sink = new SqlServerSink(connectionString, tableName);
            using (var eventCollector = new EventCollector(sink, resolutionMillis: 500, flushIntervalMillis: 2000))
            {
                Console.WriteLine("Press some keys; enter to exit");
                char key;
                do
                {
                    key = Console.ReadKey(true).KeyChar;
                    eventCollector.RecordEvent(key.ToString());
                } while (key != '\r');
            }
        }
    }
}
