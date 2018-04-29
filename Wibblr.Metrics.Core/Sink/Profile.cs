using System;
using System.Collections.Generic;

namespace Wibblr.Metrics.Core
{
    public class Profile
    {
        public string sessionId;
        public string name;
        public int process;
        public string thread;

        // 'B' = beginning of interval, 'E' = end of interval
        public List<(DateTime, char)> timestamps;
    }
}
