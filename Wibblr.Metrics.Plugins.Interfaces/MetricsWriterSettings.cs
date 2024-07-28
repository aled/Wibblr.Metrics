namespace Wibblr.Metrics.Plugins.Interfaces
{
    public class MetricsWriterSettings
    {
        public int BatchSize { get; set; }
        public int MaxQueuedRows { get; set; }
    }
}
