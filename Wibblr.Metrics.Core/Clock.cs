using System;
using System.Threading;

using Wibblr.Utils;

namespace Wibblr.Metrics.Core
{
    internal class Clock : IClock
    {
        private Timer timer;
        private ManualResetEvent cancelRequested;
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
            cancelRequested = new ManualResetEvent(false);
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

            // ensure we do not call timer.Change after it has been disposed.
            lock (disposeLock)
            {
                if (!cancelRequested.WaitOne(0))
                    timer.Change((int)delay, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Cancel the timer. Waits for all scheduled callbacks to be executed before returning.
        /// </summary>
        public void CancelDelayedAction()
        {
            if (!cancelRequested.WaitOne(0))
            {
                cancelRequested.Set();

                lock (disposeLock)
                {
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                    timer.Dispose(isCancelled);
                }

                isCancelled.WaitOne(Timeout.Infinite);
            }
        }

        /// <summary>
        /// Check if delayedAction is cancelled. No further callbacks will be issued if this is true.
        /// </summary>
        /// <returns><c>true</c>, if cancelled, <c>false</c> otherwise.</returns>
        public bool IsDelayedActionCancelled() => isCancelled.WaitOne(0);
    }
}
