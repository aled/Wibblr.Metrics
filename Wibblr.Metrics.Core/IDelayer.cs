using System;

namespace Wibblr.Metrics.Core
{
    public interface IDelayer
    {
        /// <summary>
        /// Initialize the timer.
        /// </summary>
        /// <param name="callback">Action to execute when timer fires.</param>
        void Initialize(Action callback);

        /// <summary>
        /// Start the timer; wait for a period of time, and then execute the callback.
        /// </summary>
        /// <param name="periodMillis">Number of milliseconds to wait.</param>
        void ExecuteAfterDelay(int periodMillis);

        /// <summary>
        /// Cancel the timer. Waits for all scheduled callbacks to be executed before returning.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Check if delayer is cancelled. No further callbacks will be issued if this is true.
        /// </summary>
        /// <returns><c>true</c>, if cancelled, <c>false</c> otherwise.</returns>
        bool IsCancelled();
    }
}
