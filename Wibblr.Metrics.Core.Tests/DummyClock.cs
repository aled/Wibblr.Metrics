using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Wibblr.Metrics.Core.Tests
{
    public class DummyClock : IClock
    {
        public DateTime Current { get; private set; }

        private readonly string[] formats = { "HH:mm:ss.fff", "HH:mm:ss" };

        private bool Cancelled { get; set; } = false;
        private bool Started { get; set; } = false;

        private Action callback;
        private DateTime earliestExecutionTime;

        public DummyClock(string initialTime)
        {
            Set(initialTime);
        }

        public void SetDelayedAction(Action callback) => this.callback = callback;

        public void ExecuteAfterDelay(TimeSpan tickResolution)
        {
            Started = true;
            earliestExecutionTime = Current.RoundUp(tickResolution);
        }

        public void CancelDelayedAction() => Cancelled = true;

        public bool IsDelayedActionCancelled() => Cancelled;

        public void ClockChanged()
        {
            if (Started && !Cancelled && Current >= earliestExecutionTime)
                new TaskFactory().StartNew(callback).GetAwaiter().GetResult();
        }

        public void Set(string time)
        {
            Current = DateTime.ParseExact(time, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            ClockChanged();
        }

        public void Advance(TimeSpan timeSpan)
        {
            Current = Current.Add(timeSpan);
            ClockChanged();
        }
    }
}
