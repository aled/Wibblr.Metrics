using System;
using Xunit;
using FluentAssertions;

namespace Wibblr.Metrics.Core.Tests
{
    public class ProfileTests
    {
        [Fact]
        public void Tests()
        {
            var sink = new DictionarySink();
            var clock = new DummyClock("12:00:00");

            var metrics = new MetricsCollector(sink, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30));
        }
    }
}
