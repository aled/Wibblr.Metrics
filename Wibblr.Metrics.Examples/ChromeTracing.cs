using System;
using System.Threading;
using Wibblr.Metrics.Core;

namespace Wibblr.Metrics.Examples
{
    public class ChromeTracing
    {
        static void Main(string[] args)
        {
            using (var metrics = new MetricsCollector(new FileSink(),
                                                      windowSize: TimeSpan.FromSeconds(1),
                                                      flushInterval: TimeSpan.FromSeconds(2),
                                                      ignoreEmptyBuckets: true))
            {
                var sessionId = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

                using (metrics.Profile(sessionId, "block1"))
                {
                    Thread.Sleep(20);
                    for (int i = 0; i < 100; i++)
                    {
                        metrics.StartInterval(sessionId, "block2");
                        Thread.Sleep(50);
                        metrics.EndInterval(sessionId, "block2");
                        Thread.Sleep(10);
                    }
                }
            }
        }
    }
}
