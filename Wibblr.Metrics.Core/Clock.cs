using System;
using System.Threading;

namespace Wibblr.Metrics.Core
{
    internal class Clock : IClock
    {
        private Timer timer;
        private ManualResetEvent isCancelled;
        private object disposeLock = new object();

        /// <summary>
        /// Return the current date/time
        /// </summary>
        public DateTime Current { get => DateTime.UtcNow; }

        /// <summary>
        /// Specify an action to be called later.
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void SetDelayedAction(Action callback)
        {
            timer = new Timer(o => callback(), null, Timeout.Infinite, Timeout.Infinite);
            isCancelled = new ManualResetEvent(false);
        }

        /// <summary>
        /// Executes the pre-specified action a delay. The actual delay is 
        /// calculated so the action executes at the next 'tick'
        /// </summary>
        /// <param name="tickResolution">Tick resolution.</param>
        public void ExecuteAfterDelay(TimeSpan tickResolution) 
        {
            var now = Current;
            var delay = now.RoundUp(tickResolution).Subtract(now).TotalMilliseconds;

            if (delay > int.MaxValue)
                delay = int.MaxValue;

            // need to synchronize this call, in case the timer is disposed
            // by the time we call the Change() method
            lock (disposeLock)
            {
                if (!IsDelayedActionCancelled())
                    timer.Change((int)delay, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Cancel the timer. Waits for all scheduled callbacks to be executed before returning.
        /// </summary>
        public void CancelDelayedAction()
        {
            lock (disposeLock)
            {
                if (!IsDelayedActionCancelled())
                {
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                    timer.Dispose(isCancelled);
                    isCancelled.WaitOne(Timeout.Infinite);
                }
            }
        }

        /// <summary>
        /// Check if delayedAction is cancelled. No further callbacks will be issued if this is true.
        /// </summary>
        /// <returns><c>true</c>, if cancelled, <c>false</c> otherwise.</returns>
        public bool IsDelayedActionCancelled() => isCancelled.WaitOne(0);
    }
}
