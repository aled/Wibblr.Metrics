using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Writes metrics to a single textwriter (e.g. console)
    /// </summary>
    public class TextWriterSink : IMetricsSink
    {
        private TextWriter _writer;
        private IMetricsSerializer _serializer;

        public TextWriterSink(TextWriter writer, IMetricsSerializer serializer)
        {
            _writer = writer;
            _serializer = serializer;
        }

        public void Flush(IEnumerable<WindowedCounter> counters)
        {
            if (counters.Any())
            {
                _serializer.WriteCounterHeader(_writer);
                _serializer.Write(counters, _writer);
            }
        }

        public void Flush(IEnumerable<WindowedBucket> buckets)
        {
            if (buckets.Any())
            {
                _serializer.WriteBucketHeader(_writer);
                _serializer.Write(buckets, _writer);
            }
        }

        public void Flush(IEnumerable<TimestampedEvent> events)
        {
            if (events.Any())
            {
                _serializer.WriteEventHeader(_writer);
                _serializer.Write(events, _writer);
            }
        }

        public void Flush(IEnumerable<Profile> profiles)
        {
            if (profiles.Any())
            {
                _serializer.WriteProfileHeader(_writer);
                _serializer.Write(profiles, _writer);
            }
        }

        public void FlushComplete()
        {
            // no op
        }
    }
}
