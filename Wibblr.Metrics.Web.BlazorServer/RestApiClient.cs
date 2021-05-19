using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

using Wibblr.Metrics.RestApiModels;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.Linq;

namespace Wibblr.Metrics.Web.BlazorServer
{
    public class QueryBuilder
    {
        private List<(string k, string v)> nameValuePairs = new List<(string, string)>();

        public QueryBuilder AddParameter(string key, IEnumerable<string> values)
        {
            nameValuePairs.AddRange(values.Select(value => (key, value)));
            return this;
        }

        public QueryBuilder AddParameter(string key, DateTimeOffset value)
        {
            nameValuePairs.Add((key, value.ToString("yyyy-MM-ddTHH:mm:ssK")));
            return this;
        }

        public QueryBuilder AddParameter(string key, object value)
        {
            nameValuePairs.Add((key, value.ToString()));
            return this;
        }

        public override string ToString()
        {
            if (!nameValuePairs.Any())
                return string.Empty;

            return "?" + string.Join("&", nameValuePairs.Select(x => $"{WebUtility.UrlEncode(x.k)}={WebUtility.UrlEncode(x.v)}"));
        }
    }

    public class RestApiClient
    {
        public HttpClient _httpClient;
        
        private static JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public RestApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CounterResponseModel>> GetCountersAsync(IEnumerable<string> names, DateTimeOffset from, DateTimeOffset to, TimeSpan groupBy)
        {
            var uri = "api/Query/counter";

            var query = new QueryBuilder()
                .AddParameter("name", names)
                .AddParameter("from", from)
                .AddParameter("to", to)
                .AddParameter("groupBySeconds", (int)groupBy.TotalSeconds)
                .ToString();

            var response = await _httpClient.GetAsync($"{uri}{query}");

            response.EnsureSuccessStatusCode();

            using var responseContent = await response.Content.ReadAsStreamAsync();
            {
                return await JsonSerializer.DeserializeAsync<List<CounterResponseModel>>(responseContent, options);
            }
        }
    }
}
