using System;
using System.Collections.Generic;
using System.IO;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Write to text files
    /// </summary>
    public class FileSink : IMetricsSink
    {
        private IMetricsSerializer serializer;

        public FileSink(IMetricsSerializer serializer)
        {
            this.serializer = serializer;
        }

        public void Flush(IEnumerable<WindowedCounter> counters)
        {
        }

        public void Flush(IEnumerable<WindowedBucket> buckets)
        {
        }

        public void Flush(IEnumerable<TimestampedEvent> events)
        {
        }

        public void Flush(IEnumerable<Profile> profiles)
        {
            // Assume profiles are in time order (at least for all events on a single thread)
            // as required by the chrometracing spec.
            var writers = new Dictionary<string, TextWriter>();

            try
            {
                foreach (var p in profiles)
                {
                    try
                    {
                        if (!writers.TryGetValue(p.sessionId, out var writer))
                        {
                            writer = Open(p.sessionId);
                            writers[p.sessionId] = writer;
                        }

                        if (writer != null)
                            serializer.WriteProfile(p, writer);
                    }
                    catch (IOException)
                    {
                        // on error, stop writing to this particular file
                        writers[p.sessionId] = null;   
                    }
                }
            }
            finally
            {
                foreach (var writer in writers.Values)
                    writer.Close();                
            }
        }

        private TextWriter Open(string sessionId)
        {
            var fileName = $"{sessionId}.chrometracing.json";
            var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var writer = new StreamWriter(stream);

            var len = stream.Length;

            if (len == 0)
                serializer.WriteProfileHeader(writer);
            else
                stream.Seek(len, SeekOrigin.Begin);
            
            return writer;
        }
    }
}
