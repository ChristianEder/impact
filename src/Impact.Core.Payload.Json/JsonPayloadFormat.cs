using Impact.Core.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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
		private readonly JsonSerializer serializer;

		public JsonPayloadFormat() : this(Encoding.UTF8, CreateDefaultSettings())
		{
		}

		public JsonPayloadFormat(Encoding encoding, JsonSerializerSettings settings)
		{
			Encoding = encoding;
			this.settings = settings;
			serializer = JsonSerializer.CreateDefault(settings);
		}

		public Encoding Encoding { get; }

		public JToken Serialize(object payload)
		{

			return ReferenceEquals(payload, null) ? JValue.CreateNull() : JToken.FromObject(payload, serializer);
		}

		public void Serialize(object payload, Stream stream)
		{
			var json = JToken.FromObject(payload, serializer).ToString(settings.Formatting, (settings.Converters ?? new List<JsonConverter>()).ToArray());
			var bytes = Encoding.GetBytes(json);
			stream.Write(bytes, 0, bytes.Length);
		}

		public object Deserialize(JToken payload, Type targetType)
		{
			if (payload is JValue value)
			{
				return value.Value;
			}

			return JsonConvert.DeserializeObject(payload.ToString(), targetType, settings);
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

		public static JsonSerializerSettings CreateDefaultSettings()
		{
			var settings = new JsonSerializerSettings();
			settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			return settings;
		}
	}
}
