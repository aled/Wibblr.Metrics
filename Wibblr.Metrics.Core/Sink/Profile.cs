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

        // true = start of interval, false = end of interval
        public List<(DateTime, bool)> timestamps;
    }
}
