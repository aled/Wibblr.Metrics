using System;
using System.Collections.Generic;
using System.Linq;

namespace Wibblr.Metrics.Core
{
    public class ProfileData
    {
        // Record the start/stop time of each occurrence. True = start.
        public List<(DateTime, bool)> timestamps = new List<(DateTime, bool)>();

        /// <summary>
        /// Adds the start.
        /// </summary>
        /// <param name="timestamp">Timestamp.</param>
        public void AddStart(DateTime timestamp) => timestamps.Add((timestamp, true));

        /// <summary>
        /// Adds the end.
        /// </summary>
        /// <param name="timestamp">Timestamp.</param>
        public void AddEnd(DateTime timestamp) => timestamps.Add((timestamp, false));
    }
}
