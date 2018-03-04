using System;
using System.Linq;
using System.Collections.Generic;

namespace Wibblr.Metrics.Core
{
    /// <summary>
    /// Metrics collector. Collects, aggregates and writes incoming
    /// events to a sink. 
    /// 
    /// Events can be counters or histograms.
    /// </summary>
    public sealed class MetricsCollector : IDisposable // 'sealed' allows simple implementation of IDisposable
    {
        private readonly IMetricsSink sink;
        private readonly IClock clock; 

        private readonly Dictionary<Metric, long> counters = new Dictionary<Metric, long>();
        private readonly object countersLock = new object();

        private readonly Dictionary<Metric, Histogram> histograms = new Dictionary<Metric, Histogram>();
        private readonly object histogramsLock = new object();

        private TimeSpan windowSize;
        private TimeSpan flushInterval;
        private bool ignoreEmptyBuckets;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Wibblr.Metrics.Core.EventCollector"/> class.
        /// This particular constructor is internal as it contains a parameter for the IClock,
        /// which is only used during testing.
        /// </summary>
        /// <param name="sink">Sink.</param>
        /// <param name="clock">Clock.</param>
        /// <param name="windowSize">Resolution.</param>
        /// <param name="flushInterval">Flush interval.</param>
        internal MetricsCollector(IMetricsSink sink, IClock clock, TimeSpan windowSize, TimeSpan flushInterval, bool ignoreEmptyBuckets)
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
            this.ignoreEmptyBuckets = true;

            clock.SetDelayedAction(Flush);
            clock.ExecuteAfterDelay(flushInterval);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Wibblr.Metrics.Core.CounterCollector"/> class.
        /// </summary>
        /// <param name="sink">The sink to write the events to.</param>
        /// <param name="windowSize">Resolution.</param>
        /// <param name="flushInterval">Batch will be flushed when this amount of time has passed</param>
        public MetricsCollector(IMetricsSink sink, TimeSpan windowSize, TimeSpan flushInterval, bool ignoreEmptyBuckets = false)
            : this(sink, new Clock(), windowSize, flushInterval, ignoreEmptyBuckets) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Wibblr.Metrics.Core.CounterCollector"/> class.
        /// </summary>
        /// <param name="sink">The sink to write the events to.</param>
        public MetricsCollector(IMetricsSink sink)
            : this(sink, new Clock(), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60), false) { }

        /// <summary>
        /// Increments a counter.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="increment">Increment.</param>
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

        private Dictionary<string, int[]> thresholdDict = new Dictionary<string, int[]>();

        public void RegisterThresholds(string histogramName, int[] thresholds)
        {
            thresholdDict[histogramName] = thresholds;
        }

        public void IncrementBucket(string name, float value)
        {
            var key = new Metric(name, new Window(clock.Current, windowSize));

            if (!thresholdDict.TryGetValue(name, out var thresholds))
                thresholds = new int[] { 0, 10, 100, 1000, 10000, 100000, 1000000 };

            lock (histogramsLock)
            {
                if (!histograms.TryGetValue(key, out Histogram existing))
                    histograms[key] = new Histogram(thresholds);

                histograms[key].Add(value);                    
            }
        }

        /// <summary>
        /// Flush this instance.
        /// </summary>
        private void Flush()
        {
            bool isFlushCancelled = clock.IsDelayedActionCancelled();
            var tempCounters = new List<AggregatedCounter>();
            var tempBuckets = new List<WindowedBucket>();

            // Any events that are recorded after this line will have a start time equal
            // or later to this.
            var currentTimePeriodStart = clock.Current.RoundDown(windowSize);

            lock (countersLock)
            {
                foreach (var c in counters.Keys.ToList())
                {
                    // Only take counts before the current timeperiod (unless 
                    // the delayedAction is cancelled, i.e. there will be no more flushes),
                    // so that each counter is only written once for each window.
                    if (isFlushCancelled || c.window.start < currentTimePeriodStart)
                    {
                        tempCounters.Add(new AggregatedCounter 
                        { 
                            name = c.name,
                            window = c.window,
                            count = counters[c] 
                        });
                        counters.Remove(c);
                    }    
                }
            }
            lock (histogramsLock)
            {
                foreach (var h in histograms.Keys.ToList())
                {
                    if (isFlushCancelled || h.window.start < currentTimePeriodStart)
                    {
                        foreach (var bucket in histograms[h].Buckets())
                        {
                            if (ignoreEmptyBuckets && bucket.count == 0)
                                continue;
                            
                            tempBuckets.Add(new WindowedBucket
                            {
                                name = h.name,
                                window = h.window,
                                from = bucket.from,
                                to = bucket.to,
                                count = bucket.count
                            });
                        }
                        histograms.Remove(h);
                    }
                }
            }

            // Flush even if there are no events, so that any queued events
            // in the sink are flushed.
            if (tempCounters != null)
                sink.Flush(tempCounters);

            if (tempBuckets != null)
                sink.Flush(tempBuckets);

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
