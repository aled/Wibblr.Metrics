using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace Wibblr.Metrics.Core
{
    public class TextWriterSink : IMetricsSink
    {
        private TextWriter writer;

        public TextWriterSink(TextWriter writer)
        {
            this.writer = writer;
        }

        public void RecordEvents(IDictionary<EventKey, long> events)
        {
            var lines = events
                .Select(kv => new { 
                    Name = kv.Key.name, 
                    Start = kv.Key.timePeriod.start, 
                    End = kv.Key.timePeriod.end, 
                    Count = kv.Value })
                .OrderBy(x => x.Start)
                .ThenBy(x => x.Name)
                .Select(x => $"{x.Start.ToString("mm:ss.fff")} - {x.End.ToString(x.End.ToString("mm:ss.fff"))} {x.Name} {x.Count}");

            foreach (var line in lines)
                writer.WriteLine(line);

            writer.WriteLine("---");

            writer.Flush();
        }
    }
}