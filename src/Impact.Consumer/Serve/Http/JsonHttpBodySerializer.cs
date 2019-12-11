using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Impact.Consumer.Serve.Http
{
    public class JsonHttpBodySerializer : IHttpBodySerializer
    {
        private readonly Formatting formatting;
        private readonly JsonSerializerSettings settings;

        public JsonHttpBodySerializer() : this(Formatting.Indented, new JsonSerializerSettings())
        {
        }

        public JsonHttpBodySerializer(Formatting formatting, JsonSerializerSettings settings)
        {
            this.formatting = formatting;
            this.settings = settings;
        }

        public async Task SerializeTo(HttpResponse pactResponse, Microsoft.AspNetCore.Http.HttpResponse response)
        {
            if (pactResponse.Body != null)
            {
                response.ContentType = MediaTypeNames.Application.Json;
                await response.WriteAsync(JsonConvert.SerializeObject(pactResponse.Body, formatting, settings));
            }
        }
    }
}