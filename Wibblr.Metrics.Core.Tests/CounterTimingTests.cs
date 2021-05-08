using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wibblr.Metrics.Core.Tests
{
    public class CounterTimingTests
    {
        private static readonly TimeSpan oneSecond = TimeSpan.FromSeconds(1);

        [Fact]
        public void EventShouldFlushAtMultiplesOf5000ms()
        {
            var clock = new DummyClock("11:00:00.000");

            var sink = new DictionarySink();

            using (var e = new MetricsCollector(sink, clock, oneSecond, TimeSpan.FromSeconds(5), false))
            {
                clock.Set("12:00:04.000");
                e.IncrementCounter("myevent");

                // should not fire event yet
                clock.Set("12:00:04.999");
                Assert.Empty(sink.Counters);

                clock.Set("12:00:05.000");

                // should now fire event on another thread and then return
                Assert.Single(sink.Counters);
            }
        }

        [Fact]
        public void ShouldFlushOnDispose()
        {
            var clock = new DummyClock("11:00:00.000");
            var sink = new DictionarySink();

            using (var e = new MetricsCollector(sink, clock, oneSecond, TimeSpan.FromSeconds(5), false))
            {
                clock.Set("12:00:01.500");

                // 10000 events per second for 10 virtual seconds, using 3 threads
                var options = new ParallelOptions { MaxDegreeOfParallelism = 5 };
                for (int i = 0; i < 10; i++)
                {
                    Parallel.For(0, 10000, options, (j, s) =>
                    {
                        e.IncrementCounter($"myevent-{Thread.CurrentThread.ManagedThreadId}");
                    });
                    clock.Advance(oneSecond);
                }
            }

            // Dispose method on event collect ensures all events are flushed immediately
            Assert.Equal(100000, sink.Counters.Values.Sum());
        }
    }
}
