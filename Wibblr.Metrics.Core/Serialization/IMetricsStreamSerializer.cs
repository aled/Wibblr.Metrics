using System;
using System.Collections.Generic;
using System.IO;

using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Core
{
    public interface IMetricsStreamSerializer
    {
        public string FileExtension { get; }

        void WriteCounterHeader(Stream stream);
        void Write(IEnumerable<WindowedCounter> counters, Stream stream);
        
        void WriteEventHeader(Stream stream);
        void Write(IEnumerable<TimestampedEvent> timestampedEvents, Stream stream);

        void WriteBucketHeader(Stream stream);
        void Write(IEnumerable<WindowedBucket> buckets, Stream stream);

        void WriteProfileHeader(Stream stream);
        void Write(IEnumerable<Profile> profiles, Stream stream);
    }
}
