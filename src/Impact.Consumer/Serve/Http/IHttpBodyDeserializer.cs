using System.Threading.Tasks;

namespace Impact.Consumer.Serve.Http
{
    public interface IHttpBodyDeserializer
    {
        Task<object> DeserializeFrom(Microsoft.AspNetCore.Http.HttpRequest request);
    }
}