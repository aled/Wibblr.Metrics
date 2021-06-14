using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Class only useful for writing 
    /// </summary>
    public class SessionIdNamingStrategy : IFileNamingStrategy
    {
        public string Basename(WindowedCounter counter) => null;

        public string Basename(TimestampedEvent timestampedEvent) => null;
 
        public string Basename(WindowedBucket bucket) => null;

        public string Basename(Profile profile) => profile.sessionId;

        public bool EqualNames(WindowedCounter a, WindowedCounter b) => true;

        public bool EqualNames(TimestampedEvent a, TimestampedEvent b) => true;

        public bool EqualNames(WindowedBucket a, WindowedBucket b) => true;

        public bool EqualNames(Profile a, Profile b) => a.sessionId == b.sessionId;
    }
}
