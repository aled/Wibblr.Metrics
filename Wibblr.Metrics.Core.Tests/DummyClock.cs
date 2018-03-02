using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Wibblr.Metrics.Core.Tests
{
    public class DummyClock : IClock
    {
        public DateTime Current { get; private set; }

        private bool Cancelled { get; set; } = false;
        private bool Started { get; set; } = false;

        private Action callback;
        private DateTime earliestExecutionTime;

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
            {
                var task = new TaskFactory().StartNew(callback);
                task.Wait();
            }
        }

        public void Set(string time)
        {
            Current = DateTime.ParseExact(time, "HH:mm:ss.fff", CultureInfo.InvariantCulture);
            ClockChanged();
        }

        public void Add(TimeSpan timeSpan)
        {
            Current = Current.Add(timeSpan);
        }
    }
}
