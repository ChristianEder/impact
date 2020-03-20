using Impact.Core.Serialization;
using Impact.Core.Transport.Http;
using Newtonsoft.Json.Linq;

namespace Impact.Consumer.Transport.Http
{
    public class HttpTransportFormat : ITransportFormat
    {
        private readonly IPayloadFormat payloadFormat;

        public HttpTransportFormat(IPayloadFormat payloadFormat)
        {
            this.payloadFormat = payloadFormat;
        }

        public JToken SerializeRequest(object request)
        {
            var httpRequest = (HttpRequest)request;
            var body = httpRequest.Body;
            httpRequest.Body = null;

            var json = JObject.FromObject(httpRequest);
            json["body"] = payloadFormat.Serialize(body);
            return json;
        }

        public JToken SerializeResponse(object response)
        {
            var httpResponse = (HttpResponse)response;
            var body = httpResponse.Body;
            httpResponse.Body = null;

            var json = JObject.FromObject(httpResponse);
            json["body"] = payloadFormat.Serialize(body);
            return json;
        }
    }
}