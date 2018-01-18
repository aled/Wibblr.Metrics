using System;
using System.Threading;

namespace Wibblr.Metrics.Core
{
    public class Delayer : IDelayer
    {
        private Timer timer;
        private ManualResetEvent isCancelled;
        private object disposeLock = new object();

        /// <summary>
        /// Initialize the specified callback.
        /// </summary>
        /// <returns>The initialize.</returns>
        /// <param name="callback">Callback.</param>
        public void Initialize(Action callback)
        {
            timer = new Timer(o => callback(), null, Timeout.Infinite, Timeout.Infinite);
            isCancelled = new ManualResetEvent(false);
        }

        /// <summary>
        /// Executes the after delay.
        /// </summary>
        /// <param name="periodMillis">Period millis.</param>
        public void ExecuteAfterDelay(int periodMillis) => timer.Change(periodMillis, Timeout.Infinite);

        /// <summary>
        /// Cancel the timer. Waits for all scheduled callbacks to be executed before returning.
        /// </summary>
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

        /// <summary>
        /// Check if delayer is cancelled. No further callbacks will be issued if this is true.
        /// </summary>
        /// <returns><c>true</c>, if cancelled, <c>false</c> otherwise.</returns>
        /// <returns><c>true</c>, if cancelled was ised, <c>false</c> otherwise.</returns>
        public bool IsCancelled() => isCancelled.WaitOne(0);
    }
}
