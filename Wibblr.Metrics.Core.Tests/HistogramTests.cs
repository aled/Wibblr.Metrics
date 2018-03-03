using FluentAssertions;
using Xunit;

namespace Wibblr.Metrics.Core.Tests
{
    public class HistogramTests
    {
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
