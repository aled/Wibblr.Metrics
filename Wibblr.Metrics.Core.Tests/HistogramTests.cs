using System;
using FluentAssertions;
using Xunit;
using Wibblr.Utils;

namespace Wibblr.Metrics.Core.Tests
{
    public class HistogramTests
    {
        [Fact]
        public void HistogramTimingTest()
        {
            var sink = new DictionarySink();
            var clock = new DummyClock("12:00:00.000");
            var windowSize = TimeSpan.FromSeconds(1);
            var flushInterval = TimeSpan.FromSeconds(1);
            var metrics = new MetricsCollector(sink, clock, windowSize, flushInterval, false);

            metrics.RegisterThresholds("h", new[] { 0, 10, 20 });
            metrics.RegisterThresholds("g", new[] { 30, 40 });

            var window1From = clock.Current.RoundDown(windowSize);
            var window1To = window1From + windowSize;
            metrics.IncrementBucket("h", 0);
            clock.Advance(windowSize);
            sink.Buckets[new Metric("h", window1From, window1From + windowSize)][(0, 10)].Should().Be(1);

            var window2From = clock.Current.RoundDown(windowSize);
            var window2To = window2From + windowSize;
            metrics.IncrementBucket("h", 10);
            metrics.IncrementBucket("g", 29);
            clock.Advance(windowSize);
            sink.Buckets[new Metric("h", window1From, window1To)][(0, 10)].Should().Be(1);
            sink.Buckets[new Metric("h", window2From, window2To)][(10, 20)].Should().Be(1);
            sink.Buckets[new Metric("g", window2From, window2To)][(int.MinValue, 30)].Should().Be(1);

            var window3From = clock.Current.RoundDown(windowSize);
            var window3To = window3From + windowSize;
            metrics.IncrementBucket("h", 10);
            metrics.IncrementBucket("h", 11);
            metrics.IncrementBucket("h", 20);
            metrics.IncrementBucket("h", 21);
            metrics.IncrementBucket("g", 30);
            metrics.IncrementBucket("g", 31);
            metrics.IncrementBucket("g", 40);
            clock.Advance(windowSize);
            sink.Buckets[new Metric("h", window3From, window3To)][(10, 20)].Should().Be(2);
            sink.Buckets[new Metric("h", window3From, window3To)][(20, int.MaxValue)].Should().Be(2);
            sink.Buckets[new Metric("g", window3From, window3To)][(30, 40)].Should().Be(2);
            sink.Buckets[new Metric("g", window3From, window3To)][(40, int.MaxValue)].Should().Be(1);
        }

        [Fact]
        public void HistogramTest()
        {
            var h = new Histogram(1, 2, 3);

            h.Add(1);
            h.AsString().Should().Be("0 |1| 1 |2| 0 |3| 0");
            h.Percentile(0f).Should().Be(1.5f);
            h.Percentile(1f).Should().Be(1.5f);

            h.Add(2);
            h.AsString().Should().Be("0 |1| 1 |2| 1 |3| 0");
            h.Percentile(0f).Should().Be(1.5f);
            h.Percentile(0.5f).Should().Be(2f);
            h.Percentile(1f).Should().Be(2.5f);

            h.Add(2);
            h.Add(2);
            h.AsString().Should().Be("0 |1| 1 |2| 3 |3| 0");
            h.Percentile(0f).Should().Be(1.5f);
            h.Percentile(0.5f).Should().Be(2.375f); // halfway between 2.25 and 2.5
            h.Percentile(1f).Should().Be(2.75f);
        }

        [Fact]
        public void ThresholdPercentagesTest()
        {
            var h = new Histogram(10, 20);
            h.Add(9);
            h.AsString().Should().Be("1 |10| 0 |20| 0");
            h.ThresholdPercentages().Should().BeEquivalentTo(new[] { (10, 1), (20, 1) });

            h.Add(10);
            h.AsString().Should().Be("1 |10| 1 |20| 0");
            h.ThresholdPercentages().Should().BeEquivalentTo(new[] { (10, 0.5), (20, 1) });

            h.Add(15);
            h.Add(20);
            h.AsString().Should().Be("1 |10| 2 |20| 1");
            h.ThresholdPercentages().Should().BeEquivalentTo(new[] { (10, 0.25), (20, 0.75) });
        }
    }
}
