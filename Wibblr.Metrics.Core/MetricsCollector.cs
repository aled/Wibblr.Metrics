using System;
using System.Linq;
using System.Collections.Generic;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Event collector. Collects, aggregates and writes incoming
    /// events to a sink. 
    /// </summary>
    public sealed class MetricsCollector : IDisposable // 'sealed' allows simple implementation of IDisposable
    {
        private readonly IMetricsSink sink;
        private readonly IClock clock; 
        private readonly Dictionary<Metric, long> counters = new Dictionary<Metric, long>();
        private readonly object countersLock = new object();

        private TimeSpan windowSize;
        private TimeSpan flushInterval;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Wibblr.Metrics.Core.EventCollector"/> class.
        /// This particular constructor is internal as it contains a parameter for the IClock,
        /// which is only used during testing.
        /// </summary>
        /// <param name="sink">Sink.</param>
        /// <param name="clock">Clock.</param>
        /// <param name="windowSize">Resolution.</param>
        /// <param name="flushInterval">Flush interval.</param>
        internal MetricsCollector(IMetricsSink sink, IClock clock, TimeSpan windowSize, TimeSpan flushInterval)
        {
            if (windowSize.TotalMilliseconds < 100)
                throw new ArgumentException("Must be 100ms or greater", nameof(windowSize));

            if (flushInterval.TotalMilliseconds < 200)
                throw new ArgumentException("Must be 200ms or greater", nameof(flushInterval));

            if (flushInterval < windowSize)
                throw new ArgumentException("Cannot be less than the window size", nameof(flushInterval));

            if (!windowSize.IsDivisorOf(TimeSpan.FromDays(1)))
                throw new ArgumentException("Must be whole number of windows per day", nameof(windowSize));
  
            this.sink = sink;
            this.clock = clock;
            this.windowSize = windowSize;
            this.flushInterval = flushInterval;

            clock.SetDelayedAction(Flush);
            clock.ExecuteAfterDelay(flushInterval);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Wibblr.Metrics.Core.CounterCollector"/> class.
        /// </summary>
        /// <param name="sink">The sink to write the events to.</param>
        /// <param name="windowSize">Resolution.</param>
        /// <param name="flushInterval">Batch will be flushed when this amount of time has passed</param>
        public MetricsCollector(IMetricsSink sink, TimeSpan windowSize, TimeSpan flushInterval)
            : this(sink, new Clock(), windowSize, flushInterval) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Wibblr.Metrics.Core.CounterCollector"/> class.
        /// </summary>
        /// <param name="sink">The sink to write the events to.</param>
        public MetricsCollector(IMetricsSink sink)
            : this(sink, new Clock(), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3)) { }

        /// <summary>
        /// Increments a counter.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="name">Increment.</param>
        public void IncrementCounter(string name, long increment = 1)
        {
            var key = new Metric(name, new Window(clock.Current, windowSize));
                
            lock (countersLock)
            {
                if (counters.TryGetValue(key, out long existingCount))
                    counters[key] = existingCount + increment;
                else
                    counters[key] = increment;
            }
        }

        /// <summary>
        /// Flush this instance.
        /// </summary>
        private void Flush()
        {
            bool isFlushCancelled = clock.IsDelayedActionCancelled();
            var copy = new List<AggregatedCounter>();

            // Any events that are recorded after this line will have a start time equal
            // or later to this.
            var currentTimePeriodStart = clock.Current.RoundDown(windowSize);

            //Console.WriteLine("MetricsCollector: Waiting for lock");
            lock (countersLock)
            {

                //Console.WriteLine("MetricsCollector: Lock acquired");
                foreach (var counter in counters.Keys.ToList())
                {
                    // Only take counts before the current timeperiod (unless 
                    // the delayedAction is cancelled, i.e. there will be no more flushes),
                    // so that each counter is only written once for each window.
                    if (isFlushCancelled || counter.window.start < currentTimePeriodStart)
                    {
                        copy.Add(new AggregatedCounter 
                        { 
                            name = counter.name,
                            window = counter.window,
                            count = counters[counter] 
                        });
                        counters.Remove(counter);
                    }    
                }
            }

            // Flush even if there are no events, so that any queued events
            // in the sink are flushed.
            if (copy != null)
                sink.Flush(copy);

            if (!isFlushCancelled)
                clock.ExecuteAfterDelay(flushInterval);    
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:Wibblr.Metrics.Core.EventCollector"/> object.
        /// </summary>
        public void Dispose() 
        {
            clock.CancelDelayedAction();
            Flush();
        }
    }
}
