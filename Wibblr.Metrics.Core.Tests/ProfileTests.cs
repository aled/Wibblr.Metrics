using System;
using Xunit;
using FluentAssertions;
using System.Threading;

namespace Wibblr.Metrics.Core.Tests
{
    public class ProfileTests
    {
        [Fact]
        public void Tests()
        {
            var sink = new DictionarySink();
            var clock = new DummyClock("12:00:00");
            var windowSize = TimeSpan.FromSeconds(1);
            var flushInterval = TimeSpan.FromSeconds(5);

            var metrics = new MetricsCollector(sink, clock, windowSize, flushInterval, false);

          
            using (metrics.Profile("session1", "block1"))
            {
                Thread.Sleep(20);
            }

            for (int i = 0; i < 100; i++)
            {
                metrics.StartInterval("session1", "block2");
                metrics.EndInterval("session1", "block2");
            }
        }
    }
}
