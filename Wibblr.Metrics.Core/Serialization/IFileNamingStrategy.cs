using System;

using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Core
{
    public interface IFileNamingStrategy
    {
        string Basename(WindowedCounter counter);
        bool EqualNames(WindowedCounter a, WindowedCounter b);

        string Basename(TimestampedEvent timestampedEvent);
        bool EqualNames(TimestampedEvent a, TimestampedEvent b);

        string Basename(WindowedBucket bucket);
        bool EqualNames(WindowedBucket a, WindowedBucket b);

        string Basename(Profile profile);
        bool EqualNames(Profile a, Profile b);
    }
}
