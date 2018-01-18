using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wibblr.Metrics.Core.Tests
{
    public class EventTests
    {
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
       
    }
}
