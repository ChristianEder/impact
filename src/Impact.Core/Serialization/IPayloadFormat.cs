using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Impact.Core.Serialization
{
    public interface IPayloadFormat
    {
        Encoding Encoding { get; }
        JToken Serialize(object payload);
        void Serialize(object payload, Stream stream);
        object Deserialize(JToken payload, Type targetType);
        object Deserialize(Stream payload);
        string MimeType { get; }
    }
}