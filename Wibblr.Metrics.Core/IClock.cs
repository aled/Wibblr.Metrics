using System;

namespace Wibblr.Metrics.Core
{
    public interface IClock
    {
        DateTime Current { get; }
        DateTime CurrentSeconds { get; }
    }
}
