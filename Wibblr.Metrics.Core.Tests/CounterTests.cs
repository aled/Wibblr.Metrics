using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wibblr.Metrics.Core.Tests
{
    public class CounterTests
    {
        [Fact]
        public void ShouldNotCrashWhenEventRecordAfterDisposed()
        {
            var sink = new DictionarySink();
            var e = new MetricsCollector(sink, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));

            e.IncrementCounter("myCounter");
            e.Dispose();
            Assert.Equal(1, sink.Counters.Count);

            e.IncrementCounter("myCounter");
        }

        [Fact]
        public void ShouldCountEventsFromMultipleThreads()
        {
            var sink = new DictionarySink();
            using (var e = new MetricsCollector(sink, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200)))
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = 5 };
                Parallel.For(0, 100000, options, (j, s) =>
                {
                    e.IncrementCounter($"mycounter-{Thread.CurrentThread.ManagedThreadId}");
                });
            }
            Assert.Equal(100000, sink.Counters.Values.Sum());
        }
       
    }
}
