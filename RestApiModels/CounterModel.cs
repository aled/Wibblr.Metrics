using System;
using System.ComponentModel.DataAnnotations;

namespace Wibblr.Metrics.RestApiModels
{
    public class CounterModel
    { 
        [Required]
        public string Name { get; set; }

        [Required]
        public DateTimeOffset From { get; set; }
        
        [Required]
        public DateTimeOffset To { get; set; }
        
        [Required]
        public long Count { get; set; }
    }
}
