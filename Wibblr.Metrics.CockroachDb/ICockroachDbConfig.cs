using System.Collections.Generic;

namespace Wibblr.Metrics.CockroachDb
{
    public interface ICockroachDbConfig
    {
        string ConnectionString { get; }
        string Database { get; }
        string CounterTable { get; }
        string HistogramTable { get; }
        int BatchSize { get; }
        int MaxQueuedRows { get; }

        bool IsValid(out List<string> validationErrors);
    }
}
