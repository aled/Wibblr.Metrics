using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wibblr.Metrics.Plugins.Interfaces
{
    public interface IDatabasePlugin : IMetricsSource, IMetricsSink
    {
        string Name { get; }
        string Version { get; }

        void Initialize(DatabaseConnectionSettings connectionSettings, DatabaseTablesSettings tables, MetricsWriterSettings writerSettings);
    }
}
