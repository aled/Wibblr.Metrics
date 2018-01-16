using System;

namespace Wibblr.Metrics.Core
{
    public interface IMetricsCollector
    {
        void Measurement(string name, decimal measurement);
    }
}
