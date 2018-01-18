using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wibblr.Metrics.Core.XUnit
{
    public class UnitTest1
    {
        private Action<string, string> GetRecordEventAction(EventCollector e, DummyClock clock)
        {
            return new Action<string, string>((time, eventName) =>
            {
                clock.Set(time);
                e.RecordEvent(eventName);
            });
        }

        [Fact]
        public void EventShouldFlushAtMultiplesOf5000ms()
        {
            var delayer = new DummyDelayer();
            var clock = new DummyClock();
            clock.Set("11:00:00.000");

            clock.SetDelayer(delayer);
            delayer.SetClock(clock);

            var sink = new DictionarySink();

            using (var e = new EventCollector(sink, delayer, clock, 1000, 5000))
            {
                var recordEventAt = GetRecordEventAction(e, clock);
                recordEventAt("12:00:04.000", "myevent");

                // should not fire event yet
                clock.Set("12:00:04.999");
                Assert.Equal(0, sink.Events.Count);

                clock.Set("12:00:05.000");

                // should now fire event on another thread and then return
                Assert.Equal(1, sink.Events.Count);
            }
        }

        [Fact]
        public void ShouldNotCrashWhenEventRecordAfterDisposed()
        {
            var sink = new DictionarySink();
            var e = new EventCollector(sink, 1000, 5000);

            e.RecordEvent("myEvent");
            e.Dispose();
            Assert.Equal(1, sink.Events.Count);

            e.RecordEvent("myEvent");
        }

        [Fact]
        public void ShouldCountEventsFromMultipleThreads()
        {
            var sink = new DictionarySink();
            using (var e = new EventCollector(sink, 100, 200))
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = 5 };
                Parallel.For(0, 100000, options, (j, s) =>
                {
                    e.RecordEvent($"myevent-{Thread.CurrentThread.ManagedThreadId}");
                });
            }
            Assert.Equal(100000, sink.Events.Values.Sum());
        }

        [Fact]
        public void ShouldFlushOnDispose()
        {
            var delayer = new DummyDelayer();
            var clock = new DummyClock();
            clock.Set("11:00:00.000");

            clock.SetDelayer(delayer);
            delayer.SetClock(clock);

            var sink = new DictionarySink();

            using (var e = new EventCollector(sink, delayer, clock, 1000, 5000))
            {
                clock.Set("12:00:01.500");

                // 10000 events per second for 10 virtual seconds, using 3 threads
                var options = new ParallelOptions { MaxDegreeOfParallelism = 5 };
                for (int i = 0; i < 10; i++)
                {
                    Parallel.For(0, 10000, options, (j, s) =>
                    {
                        e.RecordEvent($"myevent-{Thread.CurrentThread.ManagedThreadId}");
                    });
                    clock.AddMillis(1000);
                }
            }

            // Dispose method on event collect ensures all events are flushed immediately
            Assert.Equal(100000, sink.Events.Values.Sum());
        }
    }
}
