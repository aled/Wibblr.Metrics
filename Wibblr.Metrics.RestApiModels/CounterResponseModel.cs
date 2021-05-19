using System;
using System.Collections.Generic;
using System.Text;

namespace Wibblr.Metrics.RestApiModels
{
    public class CounterResponseModel
    {
        public string Name { get; set; }
        public DateTimeOffset From { get; set; }
        public int GroupBySeconds { get; set; }
        public IList<object> Values { get; set; }
    }
}
