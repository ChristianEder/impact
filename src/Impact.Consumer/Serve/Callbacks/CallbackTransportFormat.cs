using Impact.Core.Serialization;
using Newtonsoft.Json.Linq;

namespace Impact.Consumer.Serve.Callbacks
{
    public class CallbackTransportFormat : ITransportFormat
    {
        private readonly IPayloadFormat payloadFormat;

        public CallbackTransportFormat(IPayloadFormat payloadFormat)
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