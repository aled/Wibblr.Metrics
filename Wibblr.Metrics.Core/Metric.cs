using System;

namespace Wibblr.Metrics.Core
{
    public struct Metric
    {
        public string name;
        public DateTime from;
        public DateTime to;

        public Metric(string name, DateTime from, DateTime to)
        {
            this.name = name;
            this.from = from;
            this.to = to;
        }
    }
}
