using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Core
{
    public abstract class MetricsTextSerializer : IMetricsStreamSerializer
    {
        public abstract string FileExtension { get; }

        public abstract void Write(IEnumerable<WindowedCounter> counters, TextWriter writer);

        public abstract void Write(IEnumerable<TimestampedEvent> timestampedEvents, TextWriter writer);

        public abstract void Write(IEnumerable<WindowedBucket> buckets, TextWriter writer);

        public abstract void Write(IEnumerable<Profile> profiles, TextWriter writer);

        public abstract void WriteBucketHeader(TextWriter writer);

        public abstract void WriteCounterHeader(TextWriter writer);

        public abstract void WriteEventHeader(TextWriter writer);

        public abstract void WriteProfileHeader(TextWriter writer);

        public void WriteCounterHeader(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
                WriteCounterHeader(writer);
        }

        public void Write(IEnumerable<WindowedCounter> counters, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
                Write(counters, writer);
        }

        public void WriteEventHeader(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
                WriteEventHeader(writer);
        }

        public void Write(IEnumerable<TimestampedEvent> timestampedEvents, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
                Write(timestampedEvents, writer);
        }

        public void WriteBucketHeader(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
                WriteBucketHeader(writer);
        }

        public void Write(IEnumerable<WindowedBucket> buckets, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
                Write(buckets, writer);
        }

        public void WriteProfileHeader(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
                WriteProfileHeader(writer);
        }

        public void Write(IEnumerable<Profile> profiles, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
                Write(profiles, writer);
        }
    }
}
