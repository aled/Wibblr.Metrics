using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using static Wibblr.Collections.Tests.ListBuilder<int>;

namespace Wibblr.Collections.Tests
{
    public static class BatchedQueueTestsExtentions
    {
        public static string AsString(this List<int> items)
        {
            return "(" + string.Join(",", items) + ")";
        }
    }

    public class BatchedQueueTests
    {
        [Fact]
        public void Tests()
        {
            var batchSize = 2;
            var maxCapacity = 5;
            var q = new BatchedQueue<int>(batchSize, maxCapacity);

            q.AsString().Should().Be("[()]");

            q.Enqueue(1);
            q.AsString().Should().Be("[(1)]");

            q.Enqueue(2);
            q.AsString().Should().Be("[(1,2)]");

            q.Enqueue(3);
            q.AsString().Should().Be("[(1,2)(3)]");

            q.Enqueue(4);
            q.AsString().Should().Be("[(1,2)(3,4)]");

            q.Enqueue(5);
            q.AsString().Should().Be("[(1,2)(3,4)(5)]");

            q.Enqueue(6);
            q.AsString().Should().Be("[(1,2)(3,4)(5)]");

            var firstBatch = q.DequeueBatch();
            firstBatch.Should().BeEquivalentTo(L(1,2));
            q.AsString().Should().Be("[(3,4)(5)]");

            q.Enqueue(7);
            q.AsString().Should().Be("[(3,4)(5,7)]");

            q.EnqueueToFront(firstBatch);
            q.AsString().Should().Be("[(1,2)(3,4)(5)]");

            q.EnqueueToFront(L(8,9,10));
            q.AsString().Should().Be("[(8,9,10)(1,2)]");
        }
    }
}
