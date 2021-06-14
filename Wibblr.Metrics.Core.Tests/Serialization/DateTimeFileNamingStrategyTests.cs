using System;

using Wibblr.Metrics.Plugins.Interfaces;
using FluentAssertions;
using Xunit;

namespace Wibblr.Metrics.Core.Tests.Serialization
{
    public class DateTimeFileNamingStrategyTests
    {
        [Fact]
        public void DefaultFilenames()
        {
            // default format if empty, null or missing format given is metrictype-yyyyMMdd-HHmm
            var s = new DateTimeFileNamingStrategy();

            var dt = new DateTime(2001, 01, 02, 13, 45, 30);

            var counterBasename = s.Basename(new WindowedCounter {name = "a", from = dt, to = dt.AddMinutes(1), count = 1 });
            counterBasename.Should().Be("counter-20010102-1345");

            var eventBasename = s.Basename(new TimestampedEvent { name = "a", timestamp = dt });
            eventBasename.Should().Be("event-20010102-1345");

            var histogramBasename = s.Basename(new WindowedBucket { name = "a", timeFrom = dt, timeTo = dt.AddMinutes(1), count = 1 });
            histogramBasename.Should().Be("histogram-20010102-1345");

            var profileBasename = s.Basename(new Profile(sessionId: "a", name: "a", dt, 'B'));
            profileBasename.Should().Be("profile-20010102-1345");
        }

        [Fact]
        public void CustomFilenames()
        {
            var s = new DateTimeFileNamingStrategy("yyyyMMdd");

            var dt1 = new DateTime(2001, 01, 02, 13, 45, 30);
            var dt2 = new DateTime(2001, 01, 02, 23, 15, 31); // same day
            var dt3 = new DateTime(2001, 01, 03, 13, 45, 30); // different day

            var counter1 = new WindowedCounter { name = "a", from = dt1, to = dt1.AddDays(1), count = 1 };
            var counter2 = new WindowedCounter { name = "b", from = dt2, to = dt2.AddDays(1), count = 1 };
            var counter3 = new WindowedCounter { name = "c", from = dt3, to = dt3.AddDays(1), count = 1 };

            s.EqualNames(counter1, counter2).Should().BeTrue();
            s.EqualNames(counter1, counter3).Should().BeFalse();

            var event1 = new TimestampedEvent { name = "a", timestamp = dt1 };
            var event2 = new TimestampedEvent { name = "a", timestamp = dt2 };
            var event3 = new TimestampedEvent { name = "a", timestamp = dt3 };

            s.EqualNames(event1, event2).Should().BeTrue();
            s.EqualNames(event1, event3).Should().BeFalse();

            var bucket1 = new WindowedBucket { name = "a", timeFrom = dt1, timeTo = dt1.AddMinutes(1), count = 1 };
            var bucket2 = new WindowedBucket { name = "a", timeFrom = dt2, timeTo = dt2.AddMinutes(1), count = 1 };
            var bucket3 = new WindowedBucket { name = "a", timeFrom = dt3, timeTo = dt3.AddMinutes(1), count = 1 };

            s.EqualNames(bucket1, bucket2).Should().BeTrue();
            s.EqualNames(bucket1, bucket3).Should().BeFalse();

            var profile1 = new Profile(sessionId: "a", name: "a", dt1, 'B');
            var profile2 = new Profile(sessionId: "a", name: "a", dt2, 'B');
            var profile3 = new Profile(sessionId: "a", name: "a", dt3, 'B');

            s.EqualNames(profile1, profile2).Should().BeTrue();
            s.EqualNames(profile1, profile3).Should().BeFalse();
        }
    }
}
