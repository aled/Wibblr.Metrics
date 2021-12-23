using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Wibblr.Collections;
using Wibblr.Utils;
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
        [Route("counter")]
        public ActionResult Counter([FromQuery]string[] name, DateTimeOffset from, DateTimeOffset to, int groupBySeconds)
        {
            var groupBy = TimeSpan.FromSeconds(groupBySeconds);

            // must be a whole number of groupBySeconds per day.
            if (!groupBy.IsDivisorOf(TimeSpan.FromDays(1)))
                return new JsonResult("Invalid groupBySeconds");

            DateTime fromUtc = from.UtcDateTime.RoundDown(groupBy);
            DateTime toUtc = to.UtcDateTime.RoundUp(groupBy);

            // Database must order by Name, From.
            var aggregatedCounters = _database.GetAggregatedCounters(name, fromUtc, toUtc, groupBy);

            var counterResponses = new List<CounterResponseModel>();

            // partition by counter name
            foreach (var partition in aggregatedCounters.SplitAt((a, b) => a.name != b.name))
            {
                counterResponses.Add(new CounterResponseModel { Name = partition.First().name, From = from, GroupBySeconds = groupBySeconds, Values = new List<object>() });

                // Make dictionary of from => count
                var dict = partition.ToDictionary(x => x.from, x => x.count);

                for (var expected = fromUtc; expected < toUtc; expected = expected + groupBy)
                    counterResponses.Last().Values.Add(dict.ContainsKey(expected) ? dict[expected] : 0);
            }

            return new JsonResult(counterResponses);
        }
    }
}
