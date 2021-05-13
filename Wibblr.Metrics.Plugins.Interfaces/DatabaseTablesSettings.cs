using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wibblr.Metrics.Plugins.Interfaces
{
    public class DatabaseTablesSettings
    {
        public string Counter { get; set; }
        public string Histogram { get; set; }
        public string Event { get; set; }
        public string Profile { get; set; }
    }
}
