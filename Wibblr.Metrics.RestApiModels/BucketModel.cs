using System;
using System.ComponentModel.DataAnnotations;

namespace Wibblr.Metrics.RestApiModels
{
    public class BucketModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public DateTimeOffset TimeFrom { get; set; }

        [Required]
        public DateTimeOffset TimeTo { get; set; }

        [Required]
        public int ValueFrom { get; set; }

        [Required]
        public int ValueTo { get; set; }

        [Required]
        public long Count { get; set; }
    }
}
