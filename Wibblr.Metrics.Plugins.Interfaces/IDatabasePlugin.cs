namespace Wibblr.Metrics.Plugins.Interfaces
{
    public interface IDatabasePlugin : IMetricsSource, IMetricsSink
    {
        string Name { get; }
        string Version { get; }

        void Initialize(DatabaseConnectionSettings connectionSettings, DatabaseTablesSettings tables, MetricsWriterSettings writerSettings);
    }
}
