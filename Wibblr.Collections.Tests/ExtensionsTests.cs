using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FluentAssertions;

namespace Wibblr.Collections.Tests
{
    public class ExtensionsTests
    {
        private List<int> L(params int[] values) => values.ToList();

        private List<int> Drop(List<int> list, int drop) {
            list.DropLast(drop);
            return list;
        }

        [Fact]
        public void Test1()
        {
            L(1, 2, 3).DropLast(0).Should().BeEquivalentTo(L(1, 2, 3));
            L(1, 2, 3).DropLast(1).Should().BeEquivalentTo(L(1, 2));
            L(1, 2, 3).DropLast(2).Should().BeEquivalentTo(L(1));
            L(1, 2, 3).DropLast(3).Should().BeEquivalentTo(L());
            L(1, 2, 3).DropLast(4).Should().BeEquivalentTo(L());

            L(1, 2).DropLast(0).Should().BeEquivalentTo(L(1, 2));
            L(1, 2).DropLast(1).Should().BeEquivalentTo(L(1));
            L(1, 2).DropLast(2).Should().BeEquivalentTo(L());
            L(1, 2).DropLast(3).Should().BeEquivalentTo(L());

            L(1).DropLast(0).Should().BeEquivalentTo(L(1));
            L(1).DropLast(1).Should().BeEquivalentTo(L());
            L(1).DropLast(2).Should().BeEquivalentTo(L());

            L().DropLast(0).Should().BeEquivalentTo(L());
            L().DropLast(1).Should().BeEquivalentTo(L());

        }
    }
}
