using System;
using System.Diagnostics;
using System.Threading;

namespace Wibblr.Metrics.Core
{
    public class ProfilingIntervalBuilder : IDisposable
    {
        private ProfilingInterval profilingInterval;
        private MetricsCollector metrics;

        internal ProfilingIntervalBuilder(MetricsCollector metrics, string sessionId, string name)
        {
            profilingInterval = new ProfilingInterval
            {
                sessionId = sessionId,
                name = name,
                processId = Process.GetCurrentProcess().Id,
                threadId = Thread.CurrentThread.ManagedThreadId,
                timestamp = DateTime.UtcNow
            };
          
            this.metrics = metrics;
        }

        public void Dispose()
        {
            profilingInterval.endTimestamp = DateTime.UtcNow;
            metrics.ProfileInterval(profilingInterval);
        }
    }
}
