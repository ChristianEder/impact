using Impact.Core.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            return JToken.FromObject(payload);
        }

        public void Serialize(object payload, Stream stream)
        {
            var json = JToken.FromObject(payload).ToString(settings.Formatting, (settings.Converters ?? new List<JsonConverter>()).ToArray());
            var bytes = Encoding.GetBytes(json);
            stream.Write(bytes, 0, bytes.Length);
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

            return Deserialize(JToken.Parse(json), null);
        }

        public string MimeType => "application/json";
    }
}
