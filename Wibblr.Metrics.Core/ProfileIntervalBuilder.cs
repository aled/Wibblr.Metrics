using System;

namespace Wibblr.Metrics.Core
{
    public class ProfileIntervalBuilder : IDisposable
    {
        private MetricsCollector metrics;
        private string sessionId;
        private string name;
       
        internal ProfileIntervalBuilder(MetricsCollector metrics, string sessionId, string name)
        {
            this.metrics = metrics;
            this.sessionId = sessionId;
            this.name = name;
            metrics.StartInterval(sessionId, name);
        }

        public void Dispose()
        {
            metrics.EndInterval(sessionId, name);
        }
    }
}
