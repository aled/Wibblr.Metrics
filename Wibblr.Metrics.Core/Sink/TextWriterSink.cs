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

        public void Flush(IEnumerable<WindowedCounter> events)
        {
            var lines = events
                .OrderBy(x => x.window.start)
                .ThenBy(x => x.name)
                .Select(x => $"C {x.window.start.ToString("mm:ss.fff")} - {x.window.end.ToString("mm:ss.fff")} {Printable(x.name)} {x.count}");

            if (lines.Any())
            {
                foreach (var line in lines)
                    writer.WriteLine(line);

                writer.WriteLine("-");

                writer.Flush();
            }
        }

        public void Flush(IEnumerable<WindowedBucket> buckets)
        {
            var lines = buckets
                .OrderBy(x => x.window.start)
                .ThenBy(x => x.name)
                .ThenBy(x => x.from)
                .Select(x => $"H {x.window.start.ToString("mm:ss.fff")} - {x.window.end.ToString("mm:ss.fff")} {Printable(x.name)} {x.from}-{x.to} {x.count}");

            if (lines.Any())
            {
                foreach (var line in lines)
                    writer.WriteLine(line);

                writer.WriteLine("--");

                writer.Flush();
            }
        }

        public void Flush(IEnumerable<TimestampedEvent> events)
        {
            var lines = events
                .OrderBy(x => x.timestamp)
                .ThenBy(x => x.name)
                .Select(x => $"E {x.timestamp.ToString("mm:ss.fff")} {Printable(x.name)}");

            if (lines.Any())
            {
                foreach (var line in lines)
                    writer.WriteLine(line);

                writer.WriteLine("---");

                writer.Flush();
            }
        }

        public void Flush(IEnumerable<Profile> profiles)
        {
            var lines = new List<string>();

            foreach (var p in profiles)
                lines.Add($"P {Printable(p.name)} {string.Join(" ", p.timestamps.Select(t => t.Item2 + t.Item1.ToString("mm:ss.fff")))}");

            if (lines.Any())
            {
                foreach (var line in lines)
                    writer.WriteLine(line);

                writer.WriteLine("----");

                writer.Flush();
            }
        }
    }
}