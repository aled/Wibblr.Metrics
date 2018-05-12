using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Serializes as json objects, separated by newlines.
    /// </summary>
    public class JsonObjectsSerializer : IMetricsSerializer
    {
        public string FileExtension { get => "json"; }

        public void WriteEventHeader(TextWriter writer) { }

        public void Write(IEnumerable<TimestampedEvent> timestampedEvents, TextWriter writer)
        {
            foreach (var e in timestampedEvents)
                writer.WriteLine(JsonConvert.SerializeObject(e));
        }

        public void WriteCounterHeader(TextWriter writer) { }

        public void Write(IEnumerable<WindowedCounter> counters, TextWriter writer)
        {
            foreach (var c in counters)
                writer.WriteLine(JsonConvert.SerializeObject(c));
        }

        public void WriteBucketHeader(TextWriter writer) { }

        public void Write(IEnumerable<WindowedBucket> buckets, TextWriter writer)
        {
            foreach (var b in buckets)
                writer.WriteLine(JsonConvert.SerializeObject(b));
        }

        public void WriteProfileHeader(TextWriter writer) { }

        public void Write(IEnumerable<Profile> profiles, TextWriter writer)
        {
            foreach (var p in profiles)
                writer.WriteLine(JsonConvert.SerializeObject(p));
        }
    }
}
