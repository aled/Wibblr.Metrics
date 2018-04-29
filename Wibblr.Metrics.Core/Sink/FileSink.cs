using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Write to text files
    /// </summary>
    public class FileSink : IMetricsSink
    {
        private FileSinkOptions options;

        public FileSink(FileSinkOptions options = null)
        {
            this.options = options ?? new FileSinkOptions();
        }

        public void Flush(IEnumerable<WindowedCounter> counters)
        {
            //throw new NotImplementedException();
        }

        public void Flush(IEnumerable<WindowedBucket> buckets)
        {
            //throw new NotImplementedException();
        }

        public void Flush(IEnumerable<TimestampedEvent> events)
        {
            
        }

        public IEnumerable<string> SerializeProfiles(IGrouping<string, Profile> group)
        {
            foreach (var p in group)
                foreach (var t in p.timestamps)
                    yield return JsonConvert.SerializeObject(
                        new Dictionary<string, object>
                        {
                            {"name", p.name},
                            {"ph", t.Item2.ToString()},
                            {"ts", t.Item1.Ticks / 10},
                            {"pid", p.process},
                            {"tid", int.Parse(p.thread)}
                        });
        }

        public void Flush(IEnumerable<Profile> profiles)
        {
            var profilesLookup = profiles.ToLookup(p => p.sessionId);

            foreach (var group in profilesLookup)
                Write($"{group.Key}.chrometracing.json", SerializeProfiles(group));      
        }

        private void Write(string fileName, IEnumerable<string> rows)
        {
            try
            {
                using (var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        var len = stream.Length;
                        if (len == 0)
                            writer.WriteLine("[");
                        else
                            stream.Seek(len, SeekOrigin.Begin);

                        foreach (var row in rows)
                        {
                            writer.Write(row);
                            writer.WriteLine(",");
                        }
                    }
                }
            }
            catch (IOException)
            { 
                // TODO: something
            }
        }
    }
}
