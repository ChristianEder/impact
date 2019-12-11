using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace Impact.Consumer.Serve.Http
{
    public class HttpResponse
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "status")]
        public HttpStatusCode Status { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "headers")]
        public IDictionary<string, string> Headers { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "body")]
        public object Body { get; set; }
    }
}