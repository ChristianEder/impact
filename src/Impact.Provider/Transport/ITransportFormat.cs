using Impact.Core.Serialization;
using Newtonsoft.Json.Linq;

namespace Impact.Provider.Transport
{
	public interface ITransportFormat
	{
		object DeserializeRequest(JToken request);
		object DeserializeResponse(JToken response);
	}
}