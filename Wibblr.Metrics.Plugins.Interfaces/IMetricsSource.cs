using System;
using System.Collections.Generic;
using System.Text;

namespace Wibblr.Metrics.Plugins.Interfaces
{
    public interface IMetricsSource
    {
        IEnumerable<string> GetCounterNames(DateTime start, DateTime end);

       // IEnumerable<AggregatedCounter> GetAggregatedCounter(string name, DateTime start, DateTime end, TimeSpan aggregationTime, bool includeZeroValues, string prefix = "");
    }
}
