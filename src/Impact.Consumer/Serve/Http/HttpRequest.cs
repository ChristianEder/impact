using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace Impact.Consumer.Serve.Http
{
    public class HttpRequest
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "method")]
        public HttpMethod Method { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "path")]
        public string Path { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "query")]
        public string Query { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "headers")]
        public IDictionary<string, string> Headers { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "body")]
        public object Body { get; set; }
    }
}