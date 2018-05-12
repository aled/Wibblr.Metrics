using System;
using System.Diagnostics;
using System.Threading;

namespace Wibblr.Metrics.Core
{
    public class Profile
    {
        public string sessionId;
        public int process;
        public string thread;
        public string name;
        public char phase; // 'B' = beginning, 'E' = end, 'I' = instant
        public DateTime timestamp;

        public Profile(string sessionId, string name, DateTime timestamp, char phase)
        {
            this.sessionId = sessionId;
            this.name = name;
            this.phase = phase;
            this.timestamp = timestamp;
            process = Process.GetCurrentProcess().Id;

            // TODO: have separate fields for thread ID and name?
            //       Chrome tracing requires the ID.
            thread = Thread.CurrentThread.ManagedThreadId.ToString();
        }
    }
}
