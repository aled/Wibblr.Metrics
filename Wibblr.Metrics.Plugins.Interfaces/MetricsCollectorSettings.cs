namespace Wibblr.Metrics.Plugins.Interfaces
{
    public class MetricsCollectorSettings
    {
        public string WindowSize { get; set; }
        public string FlushInterval { get; set; }
        public bool IgnoreEmptyBuckets { get; set; }
    }
}
