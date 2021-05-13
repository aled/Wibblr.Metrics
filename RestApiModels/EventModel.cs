using System;
using System.ComponentModel.DataAnnotations;

namespace Wibblr.Metrics.RestApiModels
{
    public class EventModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public DateTimeOffset Timestamp { get; set; }
    }
}
