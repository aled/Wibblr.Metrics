using System;
using System.Threading;
using System.Threading.Tasks;
using Wibblr.Metrics.Core;

namespace Wibblr.Metrics.Examples
{
    public class ChromeTracing
    {
        static MetricsCollector metrics;

        static string time = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        static void Main(string[] args)
        {
            using (metrics = new MetricsCollector(
                new FileSink(new ChromeTracingSerializer(), new SessionIdNamingStrategy()),
                windowSize: TimeSpan.FromSeconds(1),
                flushInterval: TimeSpan.FromSeconds(1),
                ignoreEmptyBuckets: true))
            {
                var tasks = new Task[]
                {
                    new TaskFactory().StartNew(() => new ChromeTracing().Run(time + "_1")),
                    new TaskFactory().StartNew(() => new ChromeTracing().Run(time + "_2")),
                    new TaskFactory().StartNew(() => new ChromeTracing().Run(time + "_3")),
                };

                Task.WaitAll(tasks);
            }
        }

        void Run(string sessionId)
        {   
            using (metrics.Profile(sessionId, "block1"))
            {
                Thread.Sleep(23);
                for (int i = 0; i < 50; i++)
                {
                    metrics.StartInterval(sessionId, "block2");
                    Thread.Sleep(21);
                    DoSomeStuff(i, sessionId);
                    metrics.EndInterval(sessionId, "block2");
                    Thread.Sleep(11);
                }
            }
        }

        static void DoSomeStuff(int i, string sessionId)
        {
            using (metrics.Profile(sessionId, $"DoSomeStuff({i})"))
            {
                Thread.Sleep(104);
            }
        }
    }
}
