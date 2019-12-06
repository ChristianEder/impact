using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Impact.Provider
{
    public class JsonRequestResponseDeserializer<TRequest, TResponse> : IRequestResponseDeserializer
    {
        public object DeserializeRequest(JToken request)
        {
            return JsonConvert.DeserializeObject<TRequest>(request.ToString());
        }

        public object DeserializeResponse(JToken response)
        {
            return JsonConvert.DeserializeObject<TResponse>(response.ToString());
        }
    }
}