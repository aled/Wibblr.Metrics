using System;
using System.Collections.Generic;
using System.Text;

namespace Wibblr.Metrics.Plugins.Interfaces
{
    public interface IMetricsSource
    {
        IEnumerable<string> GetCounterNames(DateTime start, DateTime end);

        IEnumerable<WindowedCounter> GetAggregatedCounters(IList<string> names, DateTime from, DateTime to, TimeSpan groupBy);
    }
}
