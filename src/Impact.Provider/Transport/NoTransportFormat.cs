using Impact.Core.Serialization;
using Newtonsoft.Json.Linq;

namespace Impact.Provider.Transport
{
	public class NoTransportFormat : ITransportFormat
    {
        private readonly IPayloadFormat payloadFormat;

        public NoTransportFormat(IPayloadFormat payloadFormat)
        {
            this.payloadFormat = payloadFormat;
        }
        public object DeserializeRequest(JToken request)
        {
            return payloadFormat.Deserialize(request, null);
        }

        public object DeserializeResponse(JToken response)
        {
            return payloadFormat.Deserialize(response, null);
        }

        public JToken SerializeResponse(object response)
        {
            return payloadFormat.Serialize(response);
        }
    }
}