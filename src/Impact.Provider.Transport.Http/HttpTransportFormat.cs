using Impact.Core.Serialization;
using Impact.Core.Transport.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Impact.Provider.Transport.Http
{
	internal class HttpTransportFormat : ITransportFormat
	{
		private readonly IPayloadFormat payloadFormat;

		public HttpTransportFormat(IPayloadFormat payloadFormat)
		{
			this.payloadFormat = payloadFormat;
		}
		public object DeserializeRequest(JToken requestToken)
		{
			var requestObject = (JObject)requestToken;

			var body = requestObject["body"];
			requestObject.Remove("body");

			var request = JsonConvert.DeserializeObject<HttpRequest>(requestObject.ToString());

			if (body != null)
			{
				request.Body = payloadFormat.Deserialize(body, null);
			}

			return request;
		}

		public object DeserializeResponse(JToken responseToken)
		{
			var responseObject = (JObject)responseToken;

			var body = responseObject["body"];
			responseObject.Remove("body");

			var response = JsonConvert.DeserializeObject<HttpResponse>(responseObject.ToString());

			if (body != null)
			{
				response.Body = payloadFormat.Deserialize(body, null);
			}

			return response;
		}
	}
}
