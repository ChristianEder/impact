using Newtonsoft.Json.Linq;

namespace Impact.Provider
{
    public interface IRequestResponseDeserializer
    {
        object DeserializeRequest(JToken request);
        object DeserializeResponse(JToken response);
    }
}