using System.Collections.Generic;

namespace Wibblr.Metrics.RestApiModels
{
    public class MetricsModel
    {
        public IList<CounterModel> Counters { get; set; }
        public IList<EventModel> Events { get; set; }
        public IList<BucketModel> Buckets { get; set; }
        public IList<ProfileModel> Profiles { get; set; }
    }
}
