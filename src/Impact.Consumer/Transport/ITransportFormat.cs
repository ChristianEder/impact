using Newtonsoft.Json.Linq;

namespace Impact.Consumer.Transport
{
    public interface ITransportFormat
    {
        JToken SerializeRequest(object request);
        JToken SerializeResponse(object response);
    }
}