using System;
using System.Collections.Generic;
using System.Linq;

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
        private Dictionary<string, int[]> thresholdDict = new Dictionary<string, int[]>();

        private readonly Dictionary<string, List<DateTime>> events = new Dictionary<string, List<DateTime>>();
        private readonly object eventsLock = new object();

        private readonly Dictionary<ProfileKey, ProfileData> profileData = new Dictionary<ProfileKey, ProfileData>();
        private readonly object profileLock = new object();

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
        /// with default values (window size and flush interval are 1 minute)
        /// </summary>
        /// <param name="sink">The sink to write the events to.</param>
        public MetricsCollector(IMetricsSink sink)
            : this(sink, new Clock(), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60), false) { }

        #region Counters
        /// <summary>
        /// Increments a counter.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="increment">Increment.</param>
        public void IncrementCounter(string name, long increment = 1)
        {
            if (name.Length > 8000)
                name = name.Substring(0, 8000);

            try
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
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }
        #endregion Counters

        #region Histograms
        public void RegisterThresholds(string histogramName, int[] thresholds)
        {
            thresholdDict[histogramName] = thresholds;
        }

        public void IncrementBucket(string name, float value)
        {
            if (name.Length > 8000)
                name = name.Substring(0, 8000);

            try
            {
                var key = new Metric(name, new Window(clock.Current, windowSize));

                // Default thresholds - useful for measuring web page latencies in milliseconds
                if (!thresholdDict.TryGetValue(name, out var thresholds))
                    thresholds = new int[] { 0, 1000, 2000, 3000, 4000, 5000, 7500, 10000, 15000, 30000, 60000, 120000, 300000, 600000, 1800000 };

                lock (histogramsLock)
                {
                    if (!histograms.TryGetValue(key, out Histogram existing))
                        histograms[key] = new Histogram(thresholds);

                    histograms[key].Add(value);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }
        #endregion Histograms

        #region Events
        public void Event(string name)
        {
            if (name.Length > 8000)
                name = name.Substring(0, 8000);

            try
            {
                var timestamp = DateTime.UtcNow;

                lock (eventsLock)
                {
                    if (!events.TryGetValue(name, out List<DateTime> existing))
                        events[name] = new List<DateTime>();

                    events[name].Add(timestamp);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }
        #endregion Events

        #region Profile
        /// <summary>
        /// Used to profile a block of code (by putting it in a 'using' statement)
        /// </summary>
        public IDisposable Profile(string sessionId, string name)
        {
            return new ProfileIntervalBuilder(this, sessionId, name, clock.Current);
        }

        /// <summary>
        /// Record the start timestamp of an interval.
        /// </summary>
        /// <param name="sessionId">Session identifier.</param>
        /// <param name="name">Name.</param>
        public void StartInterval(string sessionId, string name)
        {
            var key = new ProfileKey(sessionId, name);
            lock (profileLock)
            {
                if (!profileData.TryGetValue(key, out ProfileData data))
                    data = new ProfileData();
                
                data.AddStart(clock.Current);
                profileData[key] = data;
            } 
        }

        /// <summary>
        /// Record the end timestamp of an interval.
        /// </summary>
        /// <param name="sessionId">Session identifier.</param>
        /// <param name="name">Name.</param>
        public void EndInterval(string sessionId, string name)
        {
            var key = new ProfileKey(sessionId, name);
            lock (profileLock)
            {
                if (!profileData.TryGetValue(key, out ProfileData data))
                    data = new ProfileData();

                data.AddEnd(clock.Current);
                profileData[key] = data;
            }
        }
        #endregion Profiler

        /// <summary>
        /// Flush this instance.
        /// </summary>
        private void Flush()
        {
            try
            {
                bool isFlushCancelled = clock.IsDelayedActionCancelled();
                var tempCounters = new List<WindowedCounter>();
                var tempBuckets = new List<WindowedBucket>();
                var tempEvents = new List<TimestampedEvent>();
                var tempProfiles = new List<Profile>();

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
                            tempCounters.Add(new WindowedCounter
                            {
                                name = c.name,
                                window = c.window,
                                count = counters[c]
                            });
                            counters.Remove(c);
                        }
                    }
                }

                // Flush even if there are no events, so that any queued events
                // in the sink are flushed.
                if (tempCounters != null)
                    sink.Flush(tempCounters);

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
                if (tempBuckets != null)
                    sink.Flush(tempBuckets);

                lock (eventsLock)
                {
                    foreach (var name in events.Keys)
                    {
                        foreach (var timestamp in events[name])
                        {
                            tempEvents.Add(new TimestampedEvent
                            {
                                name = name,
                                timestamp = timestamp
                            });
                        }
                    }
                    events.Clear();
                }
                if (tempEvents != null)
                    sink.Flush(tempEvents);

                lock (profileLock)
                {
                    foreach (var key in profileData.Keys)
                    {
                        tempProfiles.Add(new Profile
                        {
                            sessionId = key.sessionId,
                            name = key.name,
                            process = key.process,
                            thread = key.thread,
                            timestamps = new List<(DateTime, char)>(profileData[key].timestamps),
                        });
                    }
                    profileData.Clear();
                }

                if (tempProfiles != null)
                    sink.Flush(tempProfiles);
            }
            catch (Exception e)
            {
                Console.Error.Write(e.Message);
            }

            try
            {
                if (!clock.IsDelayedActionCancelled())
                    clock.ExecuteAfterDelay(flushInterval); 
            }
            catch (Exception e)
            {
                Console.Error.Write(e.Message);
            }
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
