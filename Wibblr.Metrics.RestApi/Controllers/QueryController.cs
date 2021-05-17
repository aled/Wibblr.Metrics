using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Wibblr.Collections;
using Wibblr.Metrics.Core;
using Wibblr.Metrics.Plugins.Interfaces;
using Wibblr.Metrics.RestApiModels;

namespace Wibblr.Metrics.RestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueryController : ControllerBase
    {
        private IDatabasePlugin _database;

        public QueryController(IDatabasePlugin database)
        {
            _database = database;
        }

        [HttpGet]
        public ActionResult Get(DateTimeOffset from, DateTimeOffset to, int groupBySeconds)
        {
            var names = new[] { "%" };

            var groupBy = TimeSpan.FromSeconds(groupBySeconds);

            // must be a whole number of groupBySeconds per day.
            if (!groupBy.IsDivisorOf(TimeSpan.FromDays(1)))
                return new JsonResult("Invalid groupBySeconds");

            DateTime fromUtc = from.UtcDateTime.RoundDown(groupBy);
            DateTime toUtc = to.UtcDateTime.RoundUp(groupBy);

            // Database must order by Name, From.
            var aggregatedCounters = _database.GetAggregatedCounters(names, fromUtc, toUtc, groupBy);

            var counterResponses = new List<CounterResponseModel>();

            // partition by counter name
            foreach (var partition in aggregatedCounters.Partition((a, b) => a.name != b.name))
            {
                counterResponses.Add(new CounterResponseModel { Name = partition.First().name, From = from, GroupBySeconds = groupBySeconds, Values = new List<long>() });

                foreach (var c in partition)
                    for (var expected = fromUtc; expected < toUtc; expected = expected + groupBy)
                        counterResponses.Last().Values.Add(expected == c.from ? c.count : 0);
            }

            return new JsonResult(counterResponses);
        }
    }
}
