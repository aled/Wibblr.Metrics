using System;
using System.Collections.Generic;

namespace Wibblr.Metrics.Core
{
    public class ProfileData
    {
        // Record the start/stop time of each occurrence. 'B'=begin, 'E'=end
        public List<(DateTime, char)> timestamps = new List<(DateTime, char)>();

        /// <summary>
        /// Adds the start.
        /// </summary>
        /// <param name="timestamp">Timestamp.</param>
        public void AddStart(DateTime timestamp) => timestamps.Add((timestamp, 'B'));

        /// <summary>
        /// Adds the end.
        /// </summary>
        /// <param name="timestamp">Timestamp.</param>
        public void AddEnd(DateTime timestamp) => timestamps.Add((timestamp, 'E'));
    }
}
