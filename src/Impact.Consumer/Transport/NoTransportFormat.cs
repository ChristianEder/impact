using Impact.Core.Serialization;
using Newtonsoft.Json.Linq;

namespace Impact.Consumer.Transport
{
    public class NoTransportFormat : ITransportFormat
    {
        private readonly IPayloadFormat payloadFormat;

        public NoTransportFormat(IPayloadFormat payloadFormat)
        {
            this.payloadFormat = payloadFormat;
        }

        public JToken SerializeRequest(object request)
        {
            return payloadFormat.Serialize(request);
        }

        public JToken SerializeResponse(object response)
        {
            return payloadFormat.Serialize(response);
        }
    }
}