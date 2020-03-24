using Impact.Core;
using Impact.Core.Serialization;
using Impact.Core.Transport.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Impact.Provider.Transport.Http
{
	public class HttpTransport : ITransport
	{
		private readonly HttpClient httpClient;
		private readonly IPayloadFormat payloadFormat;

		public HttpTransport(HttpClient httpClient, IPayloadFormat payloadFormat, ITransportMatchers matchers = null)
		{
			Format = new HttpTransportFormat(payloadFormat);
			Matchers = matchers ?? new HttpTransportMatchers();
			this.httpClient = httpClient;
			this.payloadFormat = payloadFormat;
		}

		public ITransportFormat Format { get; }

		public ITransportMatchers Matchers { get; }

		public async Task<object> Respond(object requestObject)
		{
			var request = requestObject as HttpRequest;

			var requestMessage = new HttpRequestMessage();
			requestMessage.Method = request.Method;
			requestMessage.RequestUri = new Uri(request.Path + (string.IsNullOrEmpty(request.Query) ? "" : ("?" + request.Query)), UriKind.Relative);
			if(request.Headers != null)
			{
				foreach (var header in request.Headers)
				{
					requestMessage.Headers.Add(header.Key, header.Value);
				}
			}
			if (!ReferenceEquals(null, request.Body))
			{
				var memoryStream = new MemoryStream();
				payloadFormat.Serialize(request.Body, memoryStream);
				memoryStream.Position = 0;
				requestMessage.Content = new StreamContent(memoryStream);
				requestMessage.Content.Headers.Add("content-type", payloadFormat.MimeType);
			}

			var responseMessage = await httpClient.SendAsync(requestMessage);
			
			var response = new HttpResponse();
			response.Status = responseMessage.StatusCode;
			
			foreach (var header in responseMessage.Headers)
			{
				response.Headers = response.Headers ?? new Dictionary<string, string>();
				if(header.Value.Count() != 1)
				{
					// TODO: we need to support multi-valued headers in the future
					throw new NotSupportedException("Currently, Impact does not yet support multi-valued http headers. The provider API responded with multiple values for the header \"" + header.Key + "\"");
				}
				response.Headers.Add(header.Key, header.Value.Single());
			}

			if(responseMessage.Content != null)
			{
				var bodyStream = await responseMessage.Content.ReadAsStreamAsync();
				response.Body = payloadFormat.Deserialize(bodyStream);
			}

			return response;
		}
	}
}
