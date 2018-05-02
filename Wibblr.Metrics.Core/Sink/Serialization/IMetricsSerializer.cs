using System;
using System.IO;

namespace Wibblr.Metrics.Core
{
    public interface IMetricsSerializer
    {
        void WriteProfileHeader(TextWriter writer);

        void WriteProfile(Profile profile, TextWriter writer);
    }
}
