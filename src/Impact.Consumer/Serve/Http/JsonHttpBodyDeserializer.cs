using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Impact.Consumer.Serve.Http
{
    public class JsonHttpBodyDeserializer : IHttpBodyDeserializer
    {
        private readonly Encoding encoding;
        private readonly JsonSerializerSettings settings;

        public JsonHttpBodyDeserializer() : this(Encoding.UTF8, new JsonSerializerSettings())
        {
        }

        public JsonHttpBodyDeserializer(Encoding encoding, JsonSerializerSettings settings)
        {
            this.encoding = encoding;
            this.settings = settings;
        }

        public async Task<object> DeserializeFrom(Microsoft.AspNetCore.Http.HttpRequest request)
        {
            if (request.Body == null)
            {
                return null;
            }

            var memoryStream = new MemoryStream();
            await request.Body.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();

            if (bytes.Length == 0)
            {
                return null;
            }

            var json = encoding.GetString(bytes);

            return JsonConvert.DeserializeObject(json, settings);
        }
    }
}