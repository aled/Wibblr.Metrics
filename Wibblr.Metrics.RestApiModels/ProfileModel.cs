using System;
using System.ComponentModel.DataAnnotations;

namespace Wibblr.Metrics.RestApiModels
{
    public class ProfileModel
    {
        [Required]
        public string SessionId { get; set; }

        [Required]
        public int Process { get; set; }

        [Required]
        public string Thread { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Phase { get; set; }

        [Required]
        public DateTimeOffset Timestamp { get; set; }
    }
}
