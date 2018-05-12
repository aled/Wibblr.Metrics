using System;
using System.Collections.Generic;
using System.IO;

namespace Wibblr.Metrics.Core
{
    public interface IMetricsSerializer
    {
        string FileExtension { get; }

        void WriteCounterHeader(TextWriter writer);
        void Write(IEnumerable<WindowedCounter> counters, TextWriter writer);
        
        void WriteEventHeader(TextWriter writer);
        void Write(IEnumerable<TimestampedEvent> timestampedEvents, TextWriter writer);

        void WriteBucketHeader(TextWriter writer);
        void Write(IEnumerable<WindowedBucket> buckets, TextWriter writer);

        void WriteProfileHeader(TextWriter writer);
        void Write(IEnumerable<Profile> profiles, TextWriter writer);
    }
}
