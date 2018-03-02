using System;

namespace Wibblr.Metrics.Core
{
    internal interface IClock
    {
        /// <summary>
        /// Get current date/time (UTC)
        /// </summary>
        DateTime Current { get; }

        /// <summary>
        /// Initialize the timer.
        /// </summary>
        /// <param name="callback">Action to execute when timer fires.</param>
        void SetDelayedAction(Action callback);

        /// <summary>
        /// Start the timer; wait for a period of time, and then execute the callback.
        /// </summary>
        /// <param name="delay">time to wait.</param>
        void ExecuteAfterDelay(TimeSpan delay);

        /// <summary>
        /// Cancel the timer. Waits for all scheduled callbacks to be executed before returning.
        /// </summary>
        void CancelDelayedAction();

        /// <summary>
        /// Check if delayer is cancelled. No further callbacks will be issued if this is true.
        /// </summary>
        /// <returns><c>true</c>, if cancelled, <c>false</c> otherwise.</returns>
        bool IsDelayedActionCancelled();
    }
}
