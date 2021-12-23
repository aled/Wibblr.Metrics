using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using static Wibblr.Collections.Tests.ListBuilder<int>;

namespace Wibblr.Collections.Tests
{
    public static class ExtentionsTestsExtenstions
    {
        public static List<int> WithDroppedLast(this List<int> items, int n)
        {
            items.DropLast(n);
            return items;
        }
    }

    public class ExtensionsTests
    {
        [Fact]
        public void WithDroppedLast()
        {
            L(1, 2, 3).WithDroppedLast(0).Should().BeEquivalentTo(L(1, 2, 3));
            L(1, 2, 3).WithDroppedLast(1).Should().BeEquivalentTo(L(1, 2));
            L(1, 2, 3).WithDroppedLast(2).Should().BeEquivalentTo(L(1));
            L(1, 2, 3).WithDroppedLast(3).Should().BeEquivalentTo(L());
            L(1, 2, 3).WithDroppedLast(4).Should().BeEquivalentTo(L());

            L(1, 2).WithDroppedLast(0).Should().BeEquivalentTo(L(1, 2));
            L(1, 2).WithDroppedLast(1).Should().BeEquivalentTo(L(1));
            L(1, 2).WithDroppedLast(2).Should().BeEquivalentTo(L());
            L(1, 2).WithDroppedLast(3).Should().BeEquivalentTo(L());

            L(1).WithDroppedLast(0).Should().BeEquivalentTo(L(1));
            L(1).WithDroppedLast(1).Should().BeEquivalentTo(L());
            L(1).WithDroppedLast(2).Should().BeEquivalentTo(L());

            L().WithDroppedLast(0).Should().BeEquivalentTo(L());
            L().WithDroppedLast(1).Should().BeEquivalentTo(L());
        }

        [Fact]
        public void PartitionedListShouldHandleEmptyList()
        {
            var partitionedList = L()
                .SplitAt((x, y) => true)
                .Select(sublist => sublist.ToList())
                .ToList();

            partitionedList.Should().BeEquivalentTo(new List<List<int>> { });
        }

        [Fact]
        public void PartitionedListShouldHandleSingletonList()
        {
            var partitionedList = L(1)
                .SplitAt((x, y) => true)
                .Select(sublist => sublist.ToList())
                .ToList();

            partitionedList.Should().BeEquivalentTo(new List<List<int>> { L(1) });
        }

        [Fact]
        public void PartitionedListShouldPartitionEvensAndOdds()
        {
            var partitionedList = L(1, 3, 5, 2, 4, 7, 8)
                .SplitAt((x, y) => x % 2 != y % 2)
                .Select(sublist => sublist.ToList())
                .ToList();

            partitionedList.Should().BeEquivalentTo(new List<List<int>> { L(1, 3, 5), L(2, 4), L(7), L(8) });
        }
    }
}
