using System.Threading.Tasks;

namespace Impact.Consumer.Serve.Http
{
    public interface IHttpBodySerializer
    {
        Task SerializeTo(HttpResponse pactResponse, Microsoft.AspNetCore.Http.HttpResponse response);
    }
}