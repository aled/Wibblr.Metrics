using System;
using Xunit;
using FluentAssertions;

namespace Wibblr.Collections.Tests
{
    public class LookAheadEnumeratorTests
    {
        [Fact]
        public void LookAheadEnumeratorShouldHandleEmptyList()
        {
            var e = new int[] { }.GetLookAheadEnumerator();

            e.HasCurrent.Should().Be(false);
            new Action(() => { var c = e.Current; }).Should().Throw<InvalidOperationException>();
            e.HasNext.Should().Be(false);
            new Action(() => { var n = e.Next; }).Should().Throw<InvalidOperationException>();

            e.MoveNext().Should().Be(false);

            e.HasCurrent.Should().Be(false);
            new Action(() => { var c = e.Current; }).Should().Throw<InvalidOperationException>();
            e.HasNext.Should().Be(false);
            new Action(() => { var n = e.Next; }).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void LookAheadEnumeratorShouldHandleSingletonList()
        {
            var e = new[] { 1 }.GetLookAheadEnumerator();

            e.HasCurrent.Should().Be(false);
            new Action(() => { var c = e.Current; }).Should().Throw<InvalidOperationException>();
            e.HasNext.Should().Be(true);
            e.Next.Should().Be(1);

            e.MoveNext().Should().Be(true);

            e.HasCurrent.Should().Be(true);
            e.Current.Should().Be(1);
            e.HasNext.Should().Be(false);
            new Action(() => { var n = e.Next; }).Should().Throw<InvalidOperationException>();

            e.MoveNext().Should().Be(false);

            e.HasCurrent.Should().Be(false);
            new Action(() => { var c = e.Current; }).Should().Throw<InvalidOperationException>();
            e.HasNext.Should().Be(false);
            new Action(() => { var n = e.Next; }).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void LookAheadEnumeratorHandleLongList()
        {
            var e = new[] { 1, 2, 3 }.GetLookAheadEnumerator();

            e.HasCurrent.Should().Be(false);
            new Action(() => { var c = e.Current; }).Should().Throw<InvalidOperationException>();
            e.HasNext.Should().Be(true);
            e.Next.Should().Be(1);

            e.MoveNext().Should().Be(true);

            e.HasCurrent.Should().Be(true);
            e.Current.Should().Be(1);
            e.HasNext.Should().Be(true);
            e.Next.Should().Be(2);

            e.MoveNext().Should().Be(true);

            e.HasCurrent.Should().Be(true);
            e.Current.Should().Be(2);
            e.HasNext.Should().Be(true);
            e.Next.Should().Be(3);

            e.MoveNext().Should().Be(true);

            e.HasCurrent.Should().Be(true);
            e.Current.Should().Be(3);
            e.HasNext.Should().Be(false);
            new Action(() => { var n = e.Next; }).Should().Throw<InvalidOperationException>();

            e.MoveNext().Should().Be(false);

            e.HasCurrent.Should().Be(false);
            new Action(() => { var c = e.Current; }).Should().Throw<InvalidOperationException>();
            e.HasNext.Should().Be(false);
            new Action(() => { var n = e.Next; }).Should().Throw<InvalidOperationException>();
        }
    }
}
