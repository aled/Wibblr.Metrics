using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wibblr.Metrics.Plugins.Interfaces
{
    public class MetricsWriterSettings
    {  
        public int BatchSize { get; set; }
        public int MaxQueuedRows { get; set; }
    }
}
