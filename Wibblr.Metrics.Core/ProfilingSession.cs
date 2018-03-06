using System.Collections.Generic;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Class to store a profiling run.
    /// </summary>
    public class ProfilingSession
    {
        private string sessionId;
        private List<ProfilingInterval> intervals = new List<ProfilingInterval>();
        private List<ProfilingEvent> events = new List<ProfilingEvent>();
    }
}
