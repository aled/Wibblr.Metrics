using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Newtonsoft.Json;

using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Chrome tracing serializer. Not actual JSON, as the array
    /// doesn't need to be finished
    /// </summary>
    public class ChromeTracingSerializer : MetricsTextSerializer, IFileNamingStrategy
    {
        public override string FileExtension { get => "json"; }

        public override void WriteCounterHeader(TextWriter writer) { }

        public override void Write(IEnumerable<WindowedCounter> counters, TextWriter writer) { }

        public override void WriteEventHeader(TextWriter writer) { }

        public override void Write(IEnumerable<TimestampedEvent> events, TextWriter writer) { }

        public override void WriteBucketHeader(TextWriter writer) { }

        public override void Write(IEnumerable<WindowedBucket> buckets, TextWriter writer) { }

        public override void WriteProfileHeader(TextWriter writer) =>
            writer.Write('[');

        public override void Write(IEnumerable<Profile> profiles, TextWriter writer)
        {
            // Assume profiles are in time order (at least for all events on a single thread)
            // as required by the chrometracing spec.
            foreach (var profile in profiles)
            {
                if (!int.TryParse(profile.thread, out int threadId))
                    threadId = 0;

                writer.Write(JsonConvert.SerializeObject(
                    new Dictionary<string, object>
                    {
                        {"name", profile.name},
                        {"ph", profile.phase},
                        {"ts", profile.timestamp.Ticks / 10},
                        {"pid", profile.process},
                        {"tid", threadId}
                    }));

                writer.WriteLine(',');
            }
        }

        public string BaseName(WindowedCounter counter) => null;

        public bool EqualNames(WindowedCounter a, WindowedCounter b) => true;

        public string BaseName(TimestampedEvent timestampedEvent) => null;

        public bool EqualNames(TimestampedEvent a, TimestampedEvent b) => true;

        public string BaseName(WindowedBucket bucket) => null;

        public bool EqualNames(WindowedBucket a, WindowedBucket b) => true;

        public string BaseName(Profile profile) => $"{profile.sessionId}.chrometracing";

        public bool EqualNames(Profile a, Profile b) => a.sessionId == b.sessionId;
    }
}
