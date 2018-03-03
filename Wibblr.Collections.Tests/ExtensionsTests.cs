using System.Collections.Generic;
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
        public void Test1()
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
    }
}
