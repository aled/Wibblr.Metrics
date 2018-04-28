using System;
using System.Collections.Generic;
using System.Text;

namespace Wibblr.Metrics.Core
{
    public interface IDatabaseConfig
    {
        string ConnectionString { get; }
        string Database { get; }

        string CounterTable { get; }
        string HistogramTable { get; }
        string EventTable { get; }
        string ProfileTable { get; }

        int BatchSize { get; }
        int MaxQueuedRows { get; }

        bool IsValid(out List<string> validationErrors);
    }
}
