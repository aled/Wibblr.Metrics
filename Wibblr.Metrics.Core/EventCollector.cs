using System;
using System.Linq;
using System.Collections.Generic;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Event collector. Collects, aggregates and writes incoming
    /// events to a sink. 
    /// </summary>
    public sealed class EventCollector : IDisposable // 'sealed' allows simple implementation of IDisposable
    {
        private IMetricsSink sink;

        // This is overridden for unit testing
        private IClock clock;

        private int resolutionMillis = 1000;
        private int flushIntervalMillis = 3000;

        private DateTime nextFlushTime = DateTime.MinValue;
        
        private Dictionary<EventKey, long> events = new Dictionary<EventKey, long>();

        private object lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Wibblr.Metrics.Core.EventCollector"/> class.
        /// </summary>
        /// <param name="sink">Sink.</param>
        /// <param name="clock">Clock.</param>
        /// <param name="resolutionMillis">Resolution millis.</param>
        /// <param name="flushIntervalMillis">Flush interval millis.</param>
        internal EventCollector(IMetricsSink sink, IClock clock, int resolutionMillis, int flushIntervalMillis)
        {
            if (resolutionMillis < 100)
                throw new ArgumentException("Must be 100ms or greater", "resolutionMillis");

            this.sink = sink;
            this.clock = clock;
            this.resolutionMillis = resolutionMillis;
            this.flushIntervalMillis = flushIntervalMillis;

            clock.SetDelayedAction(Flush);
            clock.ExecuteAfterDelay(flushIntervalMillis);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Wibblr.Metrics.Core.EventCollector"/> class.
        /// </summary>
        /// <param name="sink">The sink to write the events to.</param>
        /// <param name="flushIntervalMillis">Batch will be flushed when this amount of time has passed</param>
        public EventCollector(IMetricsSink sink, int resolutionMillis = 1000, int flushIntervalMillis = 5000)
            : this(sink, new Clock(), resolutionMillis, flushIntervalMillis)
        {  
        }

        /// <summary>
        /// Records the event.
        /// </summary>
        /// <param name="name">Name.</param>
        public void RecordEvent(string name, long count = 1)
        {
            var startTime = clock.Current.Truncate(TimeSpan.TicksPerMillisecond * resolutionMillis);
            var endTime = startTime.AddMilliseconds(resolutionMillis);
            var key = new EventKey(name, startTime, endTime);
                
            lock (lockObject)
            {
                if (events.TryGetValue(key, out long existingCount))
                    events[key] = existingCount + count;
                else
                    events[key] = count;
            }
        }

        /// <summary>
        /// Flush this instance.
        /// </summary>
        private void Flush()
        {
            bool isDelayerCancelled = clock.IsDelayedActionCancelled();
            var copy = new Dictionary<EventKey, long>();

            var currentTimePeriod = clock.Current.Seconds();

            lock (lockObject)
            {
                // This makes sure that the flush is exactly aligned to the start of the minute,
                // i.e. flush is done at 0, 5, 10 seconds and not 1, 6, 11.
                if (nextFlushTime == DateTime.MinValue)
                    nextFlushTime = clock.Current.Truncate(TimeSpan.TicksPerMillisecond * flushIntervalMillis);

                nextFlushTime = nextFlushTime.AddMilliseconds(flushIntervalMillis);
                
                foreach (var eventKey in events.Keys.ToList())
                {
                    // Only take events before the current second (unless 
                    // the delayer is cancelled, i.e. there will be no more flushes)
                    // timePeriod.end is exclusive; so all new events will have an end
                    // later than the current timePeriod.
                    if (isDelayerCancelled || eventKey.timePeriod.end <= currentTimePeriod)
                    {
                        copy[eventKey] = events[eventKey];
                        events.Remove(eventKey);
                    }    
                }
            }

            if (copy != null && copy.Any())
                sink.RecordEvents(copy);

            if (!isDelayerCancelled)
            {   
                var flushDelayMillis = (int)nextFlushTime.Subtract(clock.Current).TotalMilliseconds;

                // if the next flush time has already passed, just miss it out and wait until the next flush time.
                while (flushDelayMillis < 0)
                    flushDelayMillis += flushIntervalMillis;
                
                clock.ExecuteAfterDelay(flushDelayMillis);    
            }
        }

        public void Dispose() 
        {
            clock.CancelDelayedAction();
            Flush();
        }
    }
}
