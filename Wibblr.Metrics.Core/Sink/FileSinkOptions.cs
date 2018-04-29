using System;
namespace Wibblr.Metrics.Core
{
    public class FileSinkOptions
    {
        // Currently only 'json' supported
        public string Encoding { get; set; } = "json";

        public bool ChromeTracing { get; set; } = true;
    }
}
