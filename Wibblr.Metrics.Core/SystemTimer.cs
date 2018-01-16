using System;
using System.Threading;

namespace Wibblr.Metrics.Core
{
    public class SystemTimer : ITimer
    {
        private Timer timer;
        private ManualResetEvent isCancelled;
        private object disposeLock = new object();

        public void Initialize(Action callback)
        {
            timer = new Timer(o => callback(), null, Timeout.Infinite, Timeout.Infinite);
            isCancelled = new ManualResetEvent(false);
        }

        public void Start(int periodMillis)
        {
            timer.Change(periodMillis, Timeout.Infinite);
        }

        public bool IsCancelled() => isCancelled.WaitOne(0);

        public void Cancel()
        {
            lock (disposeLock)
            {
                if (!IsCancelled())
                {
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                    timer.Dispose(isCancelled);
                    isCancelled.WaitOne(Timeout.Infinite);
                }
            }
        }
    }
}
