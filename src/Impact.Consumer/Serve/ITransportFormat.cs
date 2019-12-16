using Newtonsoft.Json.Linq;

namespace Impact.Consumer.Serve
{
    public interface ITransportFormat
    {
        JToken SerializeRequest(object request);
        JToken SerializeResponse(object response);
    }
}