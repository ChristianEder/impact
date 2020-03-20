using Impact.Core.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace Impact.Core.Payload.Json
{
    public class JsonPayloadFormat : IPayloadFormat
    {
        private readonly JsonSerializerSettings settings;

        public JsonPayloadFormat() : this(Encoding.UTF8, new JsonSerializerSettings())
        {
        }

        public JsonPayloadFormat(Encoding encoding, JsonSerializerSettings settings)
        {
            Encoding = encoding;
            this.settings = settings;
        }

        public Encoding Encoding { get; }

        public JToken Serialize(object payload)
        {
            return JToken.Parse(JsonConvert.SerializeObject(payload));
        }

        public object Deserialize(JToken payload, Type targetType)
        {
            if (payload is JValue value)
            {
                return value.Value;
            }

            return JsonConvert.DeserializeObject(payload.ToString(), targetType);
        }

        public object Deserialize(Stream payload)
        {
            var memoryStream = new MemoryStream();
            payload.CopyTo(memoryStream);
            var bytes = memoryStream.ToArray();

            if (bytes.Length == 0)
            {
                return null;
            }

            var json = Encoding.GetString(bytes);

            return JsonConvert.DeserializeObject(json, settings);
        }

        public string MimeType => "application/json";
    }
}
