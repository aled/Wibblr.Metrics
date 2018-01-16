using System;
namespace Wibblr.Metrics.Core
{
    public class SystemDateTime : IDateTime
    {
        public DateTime CurrentTimestamp() => DateTime.UtcNow;
    }
}
