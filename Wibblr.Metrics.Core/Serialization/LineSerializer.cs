using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Wibblr.Metrics.Plugins.Interfaces;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Simple text format for metrics (not including profiles):
    /// time = YYYYMMDDHHMMSS[.FFFFFFF]       (fractional seconds are optional)
    /// window = N[s|m|h|d]                   (defaults to seconds if no units)
    /// 
    /// C = counter
    /// H = histogram
    /// V = event
    /// S = sample
    /// 
    /// e.g.
    /// C 20210403T120000 60 a.b.c 14         <-- counter 'a.b.c' has value 14 during the interval between 2021-04-03 12:00:00 and 2021-04-03 12:01:00
    /// H 20210403T120000 60 a.b.c 0-200 14   <-- histogram 'a.b.c' has value 14 in the bucket 0-200
    /// E 20210403T120000 x.y.z               <-- event
    /// S 20210403T120000 d.e.f 15            <-- sample
    /// </summary>
    public class LineSerializer : IMetricsSerializer
    {
        public string FileExtension => ".txt";

        private string Printable(string s) =>
            new string(s.SelectMany(c =>  char.IsControl(c) ? ("0x" + ((int)c).ToString("X4")).ToCharArray() : new char[] { c }).ToArray());

        private string FormatTimestamp(DateTime dt)
        {
            int fractionalTicks = (int)(dt.Ticks % 10000000);

            if (fractionalTicks == 0)
                return dt.ToString("yyyyMMddTHHmmss");

            var s = dt.ToString("yyyyMMddTHHmmss.fffffff");

            // remove trailing zeros.
            var i = s.Length - 1;
            while (s[i] == '0')
                i--;

            return s.Substring(0, i + 1);
        }

        public void Write(IEnumerable<WindowedCounter> counters, TextWriter writer)
        {
            if (!counters.Any())
                return;

            foreach (var c in counters)
                writer.WriteLine($"C {FormatTimestamp(c.from)} {(c.to - c.from).TotalSeconds} {Printable(c.name)} {c.count}");

            writer.Flush();
        }

        public void Write(IEnumerable<TimestampedEvent> events, TextWriter writer)
        {
            if (!events.Any())
                return;

            foreach (var e in events)
                writer.WriteLine($"V {FormatTimestamp(e.timestamp)} {e.name}");

            writer.Flush();
        }

        public void Write(IEnumerable<WindowedBucket> buckets, TextWriter writer)
        {
            if (!buckets.Any())
                return;

            foreach (var b in buckets)
                writer.WriteLine($"H {FormatTimestamp(b.timeFrom)} {(b.timeTo - b.timeFrom).TotalSeconds} {Printable(b.name)} {"" + b.valueFrom}-{"" + b.valueTo} {b.count}");

            writer.Flush();
        }

        public void Write(IEnumerable<Profile> profiles, TextWriter writer)
        {
        }

        public void WriteBucketHeader(TextWriter writer)
        {
        }

        public void WriteCounterHeader(TextWriter writer)
        {
        }

        public void WriteEventHeader(TextWriter writer)
        {
        }

        public void WriteProfileHeader(TextWriter writer)
        {
        }
    }
}
