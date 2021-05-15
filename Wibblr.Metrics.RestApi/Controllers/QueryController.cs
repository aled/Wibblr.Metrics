using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

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

            string expectedName = null;
            DateTime expectedFrom = fromUtc;

            foreach (var c in aggregatedCounters)
            {
                if (c.name != expectedName)
                {
                    // pad out the previous counter's values with zeros up to the end time
                    if (counterResponses.Any())
                    {
                        while (expectedFrom < to.Subtract(groupBy))
                        {
                            expectedFrom = expectedFrom.Add(groupBy);
                            counterResponses.Last().Values.Add(0);
                        }
                    }

                    counterResponses.Add(new CounterResponseModel { Name = c.name, From = from, GroupBySeconds = groupBySeconds, Values = new List<long>() });
                    expectedFrom = fromUtc;
                }

                // pad out the current counter's values with zeros up to the next non-zero time
                while (c.from > expectedFrom)
                {
                    expectedFrom = expectedFrom.Add(groupBy);
                    counterResponses.Last().Values.Add(0);
                }
                counterResponses.Last().Values.Add(c.count);
                expectedFrom = expectedFrom.Add(groupBy);
            }

            if (counterResponses.Any())
            {
                while (expectedFrom < to.Subtract(groupBy))
                {
                    expectedFrom = expectedFrom.Add(groupBy);
                    counterResponses.Last().Values.Add(0);
                }
            }

            // pad out the last counter's values with zeros up to the end time
            while (expectedFrom < to.Subtract(groupBy))
            {
                expectedFrom = expectedFrom.Add(groupBy);
                counterResponses.Last().Values.Add(0);
            }

            return new JsonResult(counterResponses);
        }
    }
}
