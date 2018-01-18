using System;
using System.Threading.Tasks;

namespace Wibblr.Metrics.Core.XUnit
{
    public class DummyDelayer : IDelayer
    {
        private bool Cancelled { get; set; } = false;
        private bool Started { get; set; } = false;
        private int PeriodMillis { get; set; }

        private Action callback;
        private DummyClock clock;
        private DateTime startTime;

        public void Cancel() => Cancelled = true;

        public void Initialize(Action callback) => this.callback = callback;

        public bool IsCancelled() => Cancelled;

        public void ExecuteAfterDelay(int periodMillis)
        {
            Started = true;
            PeriodMillis = periodMillis;
            startTime = clock.Current;
        }

        public void SetClock(DummyClock clock) 
        {
            this.clock = clock;
        }

        public void ClockChanged()
        {
            if (Started && !Cancelled && clock.Current >= startTime.AddMilliseconds(PeriodMillis))
            {
                var task = new TaskFactory().StartNew(callback);
                task.Wait();
                startTime = clock.Current;
            }
        }
    }
}
