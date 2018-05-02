using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Chrome tracing serializer. Not actual JSON, as the array
    /// doesn't need to be finished
    /// </summary>
    public class ChromeTracingSerializer : IMetricsSerializer
    {
        public void WriteProfileHeader(TextWriter writer) =>
            writer.WriteLine("[");

        public void WriteProfile(Profile profile, TextWriter writer)
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

            writer.WriteLine(",");
        }
    }
}
