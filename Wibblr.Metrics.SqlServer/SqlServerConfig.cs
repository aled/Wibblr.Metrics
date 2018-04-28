using System.Collections.Generic;
using Wibblr.Metrics.Core;

namespace Wibblr.Metrics.SqlServer
{
    public class SqlServerConfig : IDatabaseConfig
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public string CounterTable { get; set; }
        public string HistogramTable { get; set; }
        public string EventTable { get; set; }
        public string ProfileTable { get; set; }
    
        public int BatchSize { get; set; }
        public int MaxQueuedRows { get; set; }

        public bool IsValid(out List<string> validationErrors)
        {
            validationErrors = new List<string>();

            if (!Database.IsAlphanumeric())
                validationErrors.Add("Database name must be alphanumeric");

            if (!CounterTable.IsAlphanumeric())
                validationErrors.Add("Counter table must be alphanumeric");

            if (!HistogramTable.IsAlphanumeric())
                validationErrors.Add("Histogram table must be alphanumeric");

            if (!EventTable.IsAlphanumeric())
                validationErrors.Add("Event table must be alphanumeric");

            return validationErrors.IsEmpty();
        }
    }
}
