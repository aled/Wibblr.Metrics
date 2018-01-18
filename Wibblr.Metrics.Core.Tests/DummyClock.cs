using System;
using System.Globalization;

namespace Wibblr.Metrics.Core.XUnit
{
    public class DummyClock : IClock
    {
        public DateTime Current { get; private set; }
        public DateTime CurrentSeconds { get => Current.Truncate(TimeSpan.TicksPerSecond); }

        private DummyDelayer delayer;

        public void SetDelayer(DummyDelayer delayer)
        {
            this.delayer = delayer;
        }

        public void Set(string time)
        {
            Current = DateTime.ParseExact(time, "HH:mm:ss.fff", CultureInfo.InvariantCulture);
            if (delayer != null) 
                delayer.ClockChanged();
        }

        public void AddMillis(int millis)
        {
            Current = Current.AddMilliseconds(millis);
        }
    }

}
