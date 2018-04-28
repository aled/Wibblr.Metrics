using System.Collections.Generic;
using Npgsql;
using Wibblr.Metrics.Core;

namespace Wibblr.Metrics.CockroachDb
{
    public class CockroachDbSinkBuilder : IDatabaseConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string CounterTable { get; set; }
        public string HistogramTable { get; set; }
        public string EventTable { get; set; }
        public string ProfileTable { get; set; }
        public int BatchSize { get; set; }
        public int MaxQueuedRows { get; set; }

        public string ConnectionString
        {
            get => new NpgsqlConnectionStringBuilder
            {
                Host = Host,
                Port = Port,
                Username = Username,
                Password = Password,
                Database = Database
            }.ConnectionString;
        }
         
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
       
            if (!ProfileTable.IsAlphanumeric())
                validationErrors.Add("Profile table must be alphanumeric");

            return validationErrors.IsEmpty();
        }

        public CockroachDbSink Build() => 
            new CockroachDbSink(this);
    }
}
