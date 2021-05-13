using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Core
{
    public class LineSerializer : IMetricsSerializer
    {
        public string FileExtension => ".txt";

        private string Printable(string s) =>
            new string(s.SelectMany(c => Char.IsControl(c) ? ("0x" + ((int)c).ToString("X4")).ToCharArray() : new char[] { c }).ToArray());

        public void Write(IEnumerable<WindowedCounter> counters, TextWriter writer)
        {
            var lines = counters
                .OrderBy(x => x.from)
                .ThenBy(x => x.name)
                .Select(x => $"C {x.from.ToString("mm:ss.fff")} - {x.to.ToString("mm:ss.fff")} {Printable(x.name)} {x.count}");

            if (lines.Any())
            {
                foreach (var line in lines)
                    writer.WriteLine(line);

                writer.WriteLine("-");

                writer.Flush();
            }
        }

        public void Write(IEnumerable<TimestampedEvent> events, TextWriter writer)
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

        public void Write(IEnumerable<WindowedBucket> buckets, TextWriter writer)
        {
            var lines = buckets
              .OrderBy(x => x.timeFrom)
              .ThenBy(x => x.name)
              .ThenBy(x => x.valueFrom)
              .Select(x => $"H {x.timeFrom.ToString("mm:ss.fff")} - {x.timeTo.ToString("mm:ss.fff")} {Printable(x.name)} {x.valueFrom}-{x.valueTo} {x.count}");

            if (lines.Any())
            {
                foreach (var line in lines)
                    writer.WriteLine(line);

                writer.WriteLine("--");

                writer.Flush();
            }
        }

        public void Write(IEnumerable<Profile> profiles, TextWriter writer)
        {
            var lines = new List<string>();

            foreach (var p in profiles)
                lines.Add($"P {Printable(p.name)} {p.phase} {p.timestamp.ToString("mm:ss.fff")}");

            if (lines.Any())
            {
                foreach (var line in lines)
                    writer.WriteLine(line);

                writer.WriteLine("----");

                writer.Flush();
            }
        }

        public void WriteBucketHeader(TextWriter writer)
        {
            throw new NotImplementedException();
        }

        public void WriteCounterHeader(TextWriter writer)
        {
            throw new NotImplementedException();
        }

        public void WriteEventHeader(TextWriter writer)
        {
            throw new NotImplementedException();
        }

        public void WriteProfileHeader(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
