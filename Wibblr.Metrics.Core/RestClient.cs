using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using Wibblr.Metrics.Plugins.Interfaces;
using Wibblr.Collections;
using Wibblr.Metrics.RestApiModels;

namespace Wibblr.Metrics.Core
{
    internal class BufferedSender<T>
    {
        private static HttpClient _client = new HttpClient();

        private string _uri;
        private Func<IList<T>, MetricsModel> _builder;
        private BatchedQueue<T> _queue;
        private object _queueLock = new object();

        public BufferedSender(MetricsWriterSettings settings, string uri, Func<IList<T>, MetricsModel> builder)
        {
            _uri = uri;
            _builder = builder;
            _queue = new BatchedQueue<T>(settings.BatchSize, settings.MaxQueuedRows);
        }

        public void Send(IEnumerable<T> items)
        {
            lock(_queueLock)
            {
                _queue.Enqueue(items);

                if (_queue.Count() == 0)
                    return;
            }

            List<T> batch;
            do
            {
                batch = null;

                lock (_queueLock)
                {
                    if (_queue.Count() > 0)
                        batch = _queue.DequeueBatch();
                }

                try
                {
                    var json = JsonSerializer.Serialize(_builder(batch));
                    var request = new HttpRequestMessage(HttpMethod.Post, _uri);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = _client.Send(request);

                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Http send failed with code {response.StatusCode}: {response.Content}. Json: {json}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    lock (_queueLock)
                    {
                        _queue.EnqueueToFront(batch);
                    }
                    break;
                }
            } while (batch != null);
        }
    }

    public class RestClient : IMetricsSink
    {
        private BufferedSender<CounterModel> _counterSender;
        private BufferedSender<BucketModel> _bucketSender;
        private BufferedSender<EventModel> _eventSender;
        private BufferedSender<ProfileModel> _profileSender;

        public RestClient(MetricsWriterSettings writerSettings, string uri)
        {
            _counterSender = new BufferedSender<CounterModel>(writerSettings, uri, items => new MetricsModel { Counters = items });
            _bucketSender = new BufferedSender<BucketModel>(writerSettings, uri, items => new MetricsModel { Buckets = items });
            _eventSender = new BufferedSender<EventModel>(writerSettings, uri, items => new MetricsModel { Events = items });
            _profileSender = new BufferedSender<ProfileModel>(writerSettings, uri, items => new MetricsModel { Profiles = items });
        }

        public void Flush(IEnumerable<WindowedCounter> counters)
        {
            _counterSender.Send(counters.Select(c => new CounterModel { Name = c.name, From = c.from, To = c.to, Count = c.count }));
        }

        public void Flush(IEnumerable<WindowedBucket> buckets)
        {
            _bucketSender.Send(buckets.Select(b => new BucketModel { Name = b.name, TimeFrom = b.timeFrom, TimeTo = b.timeTo, ValueFrom = b.valueFrom ?? int.MinValue, ValueTo = b.valueTo ?? int.MaxValue, Count = b.count }));
        }

        public void Flush(IEnumerable<TimestampedEvent> events)
        {
            _eventSender.Send(events.Select(e => new EventModel { Name = e.name, Timestamp = e.timestamp }));
        }

        public void Flush(IEnumerable<Profile> profiles)
        {
            _profileSender.Send(profiles.Select(p => new ProfileModel { SessionId = p.sessionId, Process = p.process, Thread = p.thread, Name = p.name, Phase = p.phase.ToString(), Timestamp = p.timestamp }));
        }

        public void FlushComplete()
        {
            // TODO: batch up into a single call
        }
    }
}
