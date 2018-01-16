using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Event collector. Collects, aggregates and writes incoming
    /// events to a sink. 
    /// </summary>
    public sealed class EventCollector : IDisposable // 'sealed' allows simple implementation of IDisposable
    {
        private IMetricsSink sink;

        // These are overridden for unit testing
        private ITimer timer;
        private IDateTime dateTime;

        private int resolutionMillis = 1000;
        private int flushIntervalMillis = 3000;

        private DateTime flushTimestamp;

        // Value does not need to be a long, as a new key will be generated each second
        private Dictionary<EventKey, int> events = new Dictionary<EventKey, int>();

        private object lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Wibblr.Metrics.Core.EventCollector"/> class.
        /// </summary>
        /// <param name="sink">Sink.</param>
        /// <param name="timer">Timer.</param>
        /// <param name="dateTime">Datetime.</param>
        /// <param name="resolutionMillis">Resolution millis.</param>
        /// <param name="flushIntervalMillis">Flush interval millis.</param>
        internal EventCollector(IMetricsSink sink, ITimer timer, IDateTime dateTime, int resolutionMillis, int flushIntervalMillis)
        {
            this.sink = sink;
            this.timer = timer;
            this.dateTime = dateTime;
            this.resolutionMillis = resolutionMillis;
            this.flushIntervalMillis = flushIntervalMillis;

            flushTimestamp = dateTime.CurrentTimestamp().TruncateToSecond();

            timer.Initialize(Flush);
            timer.Start(flushIntervalMillis);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Wibblr.Metrics.Core.EventCollector"/> class.
        /// </summary>
        /// <param name="sink">The sink to write the events to.</param>
        /// <param name="flushIntervalMillis">Batch will be flushed when this amount of time has passed</param>
        public EventCollector(IMetricsSink sink, int resolutionMillis = 1000, int flushIntervalMillis = 5000)
            : this(sink, new SystemTimer(), new SystemDateTime(), resolutionMillis, flushIntervalMillis)
        {  
        }

        /// <summary>
        /// Records the event.
        /// </summary>
        /// <param name="name">Name.</param>
        public void RecordEvent(string name)
        {
            var startTime = dateTime.CurrentTimestamp().TruncateToSecond();
            var endTime = startTime.AddSeconds(1);
            var key = new EventKey(name, startTime, endTime);
                
            lock (lockObject)
            {
                if (events.TryGetValue(key, out int count))
                    events[key] = count + 1;
                else
                    events[key] = 1;
            }
        }

        /// <summary>
        /// Flush this instance.
        /// </summary>
        private void Flush()
        {
            bool isTimerCancelled = timer.IsCancelled();
            var copy = new Dictionary<EventKey, int>();

            var currentTimePeriod = dateTime.CurrentTimestamp().TruncateToSecond();

            lock (lockObject)
            {
                foreach (var eventKey in events.Keys.ToList())
                {
                    // Only take events before the current second
                    // timePeriod.end is exclusive; so all new events will have an end
                    // later than the current timePeriod.
                    if (isTimerCancelled || eventKey.timePeriod.end <= currentTimePeriod)
                    {
                        copy[eventKey] = events[eventKey];
                        events.Remove(eventKey);
                    }    
                }
            }

            if (copy != null && copy.Any())
                sink.RecordEvents(copy);

            if (!isTimerCancelled)
            {
                flushTimestamp = flushTimestamp.AddMilliseconds(flushIntervalMillis);
                var delay = (int)flushTimestamp.Subtract(dateTime.CurrentTimestamp()).TotalMilliseconds;

                if (delay < 0)
                    delay = 0;
                
                timer.Start(delay);    
            }
        }

        public void Dispose() 
        {
            timer.Cancel();
            Flush();
        }
    }
}
