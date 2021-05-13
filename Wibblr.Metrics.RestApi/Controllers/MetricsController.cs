using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Wibblr.Metrics.Plugins.Interfaces;
using Wibblr.Metrics.RestApiModels;

namespace Wibblr.Metrics.RestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MetricsController : ControllerBase
    {
        private readonly ILogger<MetricsController> _logger;
        private readonly IDatabasePlugin _database;

        public MetricsController(IDatabasePlugin database, ILogger<MetricsController> logger)
        {
            _database = database;
            _logger = logger;
        }

        private (CounterModel counter, bool success, string message) Validate(CounterModel c)
        {
            bool success = false;
            string message;

            if (c.To <= c.From)
                message = "'To' must be later than 'From'";

            else
            {
                message = "Ok";
                success = true;
            }

            return (c, success, message);
        }

        [HttpPost]
        public ActionResult Counter(MetricsModel metrics)
        {
            if (metrics.Counters != null)
            {
                var validatedCounters = metrics.Counters
                    .Select(c => Validate(c));

                _database.Flush(validatedCounters
                    .Where(c => c.success)
                    .Select(c => new WindowedCounter
                    {
                        name = c.counter.Name,
                        from = c.counter.From.UtcDateTime,
                        to = c.counter.To.UtcDateTime,
                        count = c.counter.Count
                    }));

                return new JsonResult(validatedCounters.Select(c => new { c.success, c.message }));
            }
            return new OkResult();
        }

        [HttpGet]
        public IEnumerable<WindowedCounter> Counter(string name, DateTimeOffset from, DateTimeOffset to)
        {
            return new WindowedCounter[] { };
        }
    }
}
