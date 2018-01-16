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

        public void RecordEvents(IDictionary<EventKey, int> events)
        {
            var lines = events
                .Select(kv => new { 
                    Name = kv.Key.name, 
                    Start = kv.Key.timePeriod.start, 
                    End = kv.Key.timePeriod.end, 
                    Count = kv.Value })
                .OrderBy(x => x.Start)
                .ThenBy(x => x.Name)
                .Select(x => $"{x.Start.ToIsoString()} - {x.End.ToString("ss")} {x.Name} {x.Count}");

            foreach (var line in lines)
                writer.WriteLine(line);

            writer.Flush();
        }
    }
}