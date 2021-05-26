using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Serializes as json objects, separated by newlines.
    /// </summary>
    public class JsonObjectsSerializer : MetricsTextSerializer
    {
        public override string FileExtension { get => "json"; }

        public override void WriteEventHeader(TextWriter writer) { }

        public override void Write(IEnumerable<TimestampedEvent> timestampedEvents, TextWriter writer)
        {
            foreach (var e in timestampedEvents)
                writer.WriteLine(JsonConvert.SerializeObject(e));
        }

        public override void WriteCounterHeader(TextWriter writer) { }

        public override void Write(IEnumerable<WindowedCounter> counters, TextWriter writer)
        {
            foreach (var c in counters)
                writer.WriteLine(JsonConvert.SerializeObject(c));
        }

        public override void WriteBucketHeader(TextWriter writer) { }

        public override void Write(IEnumerable<WindowedBucket> buckets, TextWriter writer)
        {
            foreach (var b in buckets)
                writer.WriteLine(JsonConvert.SerializeObject(b));
        }

        public override void WriteProfileHeader(TextWriter writer) { }

        public override void Write(IEnumerable<Profile> profiles, TextWriter writer)
        {
            foreach (var p in profiles)
                writer.WriteLine(JsonConvert.SerializeObject(p));
        }
    }
}
