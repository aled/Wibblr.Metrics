using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wibblr.Metrics.Plugins.Interfaces
{
    public class DatabaseConnectionSettings
    {
        public string ConnectionString { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public bool RequireSsl { get; set; }
        public string CaCertFile { get; set; }
    }
}
