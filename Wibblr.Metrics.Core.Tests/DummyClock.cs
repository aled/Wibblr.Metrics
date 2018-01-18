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
        private int PeriodMillis { get; set; }

        private Action callback;
        private DateTime startTime;

        public void SetDelayedAction(Action callback) => this.callback = callback;

        public void ExecuteAfterDelay(int periodMillis)
        {
            Started = true;
            PeriodMillis = periodMillis;
            startTime = Current;
        }

        public void CancelDelayedAction() => Cancelled = true;

        public bool IsDelayedActionCancelled() => Cancelled;

        public void ClockChanged()
        {
            if (Started && !Cancelled && Current >= startTime.AddMilliseconds(PeriodMillis))
            {
                var task = new TaskFactory().StartNew(callback);
                task.Wait();
                startTime = Current;
            }
        }

        public void Set(string time)
        {
            Current = DateTime.ParseExact(time, "HH:mm:ss.fff", CultureInfo.InvariantCulture);
            ClockChanged();
        }

        public void AddMillis(int millis)
        {
            Current = Current.AddMilliseconds(millis);
        }
    }
}
