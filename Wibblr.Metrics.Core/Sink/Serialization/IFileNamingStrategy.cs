using System;

namespace Wibblr.Metrics.Core
{
    public interface IFileNamingStrategy
    {
        string BaseName(WindowedCounter counter);
        bool EqualNames(WindowedCounter a, WindowedCounter b);

        string BaseName(TimestampedEvent timestampedEvent);
        bool EqualNames(TimestampedEvent a, TimestampedEvent b);

        string BaseName(WindowedBucket bucket);
        bool EqualNames(WindowedBucket a, WindowedBucket b);

        string BaseName(Profile profile);
        bool EqualNames(Profile a, Profile b);
    }
}
