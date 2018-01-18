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

        private string Printable(string s) =>
            new String(s.SelectMany(c => Char.IsControl(c) ? ("0x" + ((int)c).ToString("X4")).ToCharArray() : new char[] { c }).ToArray());

        public void RecordEvents(IEnumerable<AggregatedEvent> events)
        {
            var lines = events
                .OrderBy(x => x.startTime)
                .ThenBy(x => x.name)
                .Select(x => $"{x.startTime.ToString("mm:ss.fff")} - {x.endTime.ToString("mm:ss.fff")} {Printable(x.name)} {x.count}");

            foreach (var line in lines)
                writer.WriteLine(line);

            writer.WriteLine("---");

            writer.Flush();
        }
    }
}