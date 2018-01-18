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

        // These are overridden for unit testing
        private IDelayer delayer;
        private IClock clock;

        private int resolutionMillis = 1000;
        private int flushIntervalMillis = 3000;

        private DateTime nextFlushTime = DateTime.MinValue;

        // Value does not need to be a long, as a new key will be generated each second
        private Dictionary<EventKey, int> events = new Dictionary<EventKey, int>();

        private object lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Wibblr.Metrics.Core.EventCollector"/> class.
        /// </summary>
        /// <param name="sink">Sink.</param>
        /// <param name="delayer">Timer.</param>
        /// <param name="clock">Clock.</param>
        /// <param name="resolutionMillis">Resolution millis.</param>
        /// <param name="flushIntervalMillis">Flush interval millis.</param>
        internal EventCollector(IMetricsSink sink, IDelayer delayer, IClock clock, int resolutionMillis, int flushIntervalMillis)
        {
            this.sink = sink;
            this.delayer = delayer;
            this.clock = clock;
            this.resolutionMillis = resolutionMillis;
            this.flushIntervalMillis = flushIntervalMillis;

            delayer.Initialize(Flush);
            delayer.ExecuteAfterDelay(flushIntervalMillis);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Wibblr.Metrics.Core.EventCollector"/> class.
        /// </summary>
        /// <param name="sink">The sink to write the events to.</param>
        /// <param name="flushIntervalMillis">Batch will be flushed when this amount of time has passed</param>
        public EventCollector(IMetricsSink sink, int resolutionMillis = 1000, int flushIntervalMillis = 5000)
            : this(sink, new Delayer(), new Clock(), resolutionMillis, flushIntervalMillis)
        {  
        }

        /// <summary>
        /// Records the event.
        /// </summary>
        /// <param name="name">Name.</param>
        public void RecordEvent(string name)
        {
            var startTime = clock.CurrentSeconds;
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
            bool isDelayerCancelled = delayer.IsCancelled();
            var copy = new Dictionary<EventKey, int>();

            var currentTimePeriod = clock.CurrentSeconds;

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
                
                delayer.ExecuteAfterDelay(flushDelayMillis);    
            }
        }

        public void Dispose() 
        {
            delayer.Cancel();
            Flush();
        }
    }
}
