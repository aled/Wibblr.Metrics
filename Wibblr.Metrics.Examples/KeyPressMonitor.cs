using System;
using System.Diagnostics;
using Wibblr.Metrics.Core;

namespace Wibblr.Metrics.Examples
{
    public class KeyPressMonitor
    {
        public void Run(IMetricsSink sink)
        {
            using (var metrics = new MetricsCollector(sink,
                                                      windowSize: TimeSpan.FromSeconds(2),
                                                      flushInterval: TimeSpan.FromSeconds(5),
                                                      ignoreEmptyBuckets: true))
            {
                metrics.RegisterThresholds("latency", new int[] { 0, 50, 75, 100, 1000, 2000, 10000 });
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
                    metrics.Event(key.ToString());

                    // letters a-z: lowercase is start of interval, uppercase is end.
                    if (key >= 'a' && key <= 'z')
                        metrics.StartInterval("session", key.ToString());
                    else if (key >= 'A' && key <= 'Z')
                        metrics.EndInterval("session", key.ToString().ToLower());
             
                    stopwatch.Reset();

                } while (key != '\r' && key != '\n');
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
