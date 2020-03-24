using Google.Protobuf;
using Impact.Core.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace Impact.Core.Payload.Protobuf
{
	public class ProtobufPayloadFormat<TMessage> : IPayloadFormat
		where TMessage : IMessage, new()
	{
		public ProtobufPayloadFormat(string mimeType = "application/octet-stream") : this(Encoding.UTF8, mimeType)
		{
		}

		public ProtobufPayloadFormat(Encoding encoding, string mimeType = "application/octet-stream")
		{
			Encoding = encoding;
			MimeType = mimeType;
		}

		public Encoding Encoding { get; }

		public string MimeType { get; }

		public object Deserialize(JToken payload, Type targetType)
		{
			if (!(payload is JValue value))
			{
				throw new NotSupportedException("ProtobufPayloadFormat expects the payload to be a base64 encoded binary representation");
			}
			var base64String = value.ToString();
			var bytes = Convert.FromBase64String(base64String);

			return Deserialize(bytes);
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

			return Deserialize(bytes);
		}

		public JToken Serialize(object payload)
		{
			if (!(payload is TMessage message))
			{
				throw new NotSupportedException("ProtobufPayloadFormat expects the payload to be an instance of " + typeof(TMessage).FullName);
			}

			return new JValue(Convert.ToBase64String(message.ToByteArray()));
		}

		public void Serialize(object payload, Stream stream)
		{
			var bytes = Encoding.GetBytes(Serialize(payload).ToString());
			stream.Write(bytes, 0, bytes.Length);
		}

		private static object Deserialize(byte[] bytes)
		{
			var message = new TMessage();
			return message.Descriptor.Parser.ParseFrom(bytes);
		}
	}
}
