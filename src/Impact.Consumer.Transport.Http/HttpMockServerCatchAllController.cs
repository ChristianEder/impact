using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Impact.Core;
using Impact.Core.Serialization;
using Impact.Core.Transport.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Impact.Consumer.Transport.Http
{
    [Route("")]
    public class HttpMockServerCatchAllController : Controller
    {
        private readonly Pact pact;
        private readonly IPayloadFormat payloadFormat;
        private readonly ITransportMatchers transportMatchers;

        public HttpMockServerCatchAllController(Pact pact, IPayloadFormat payloadFormat, ITransportMatchers transportMatchers)
        {
            this.pact = pact;
            this.payloadFormat = payloadFormat;
            this.transportMatchers = transportMatchers;
        }

        [HttpGet("{**path}")]
        [HttpPost("{**path}")]
        [HttpPut("{**path}")]
        [HttpPatch("{**path}")]
        [HttpDelete("{**path}")]
        [HttpHead("{**path}")]
        [HttpOptions("{**path}")]
        public async Task OnRequest(string path)
        {
            var request = new Core.Transport.Http.HttpRequest
            {
                Method = new HttpMethod(Request.Method),
                Path = Request.Path.Value,
                Query = Request.QueryString.Value,
                Body = payloadFormat.Deserialize(Request.Body)
            };

            if (Request.Headers.Any())
            {
                request.Headers = request.Headers ?? new Dictionary<string, string>();
                foreach (var header in Request.Headers)
                {
                    request.Headers[header.Key] = header.Value;
                }
            }

            Core.Transport.Http.HttpResponse response;
            try
            {
                var interaction = pact.GetMatchingInteraction(request, transportMatchers);
                if (request.Body is JObject jsonBody && interaction.RequestType != typeof(JObject))
                {
                    request.Body = payloadFormat.Deserialize(jsonBody, interaction.RequestType);
                }

                response = (Core.Transport.Http.HttpResponse)interaction.Respond(request, transportMatchers);
            }
            catch
            {
                Response.StatusCode = 404;
                var bytes = Encoding.UTF8.GetBytes("Getting a response from the pact failed for this request");
                await Response.Body.WriteAsync(bytes, 0, bytes.Length);
                return;
            }

            Response.StatusCode = (int)response.Status;
            if (response.Headers != null)
            {
                foreach (var header in response.Headers)
                {
                    Response.Headers.Add(header.Key, header.Value);
                }
            }

            if (response.Body != null)
            {
                if (!string.IsNullOrEmpty(payloadFormat.MimeType))
                {
                    Response.ContentType = payloadFormat.MimeType;
                }

                await Response.WriteAsync(payloadFormat.Serialize(response.Body).ToString(), payloadFormat.Encoding);
            }
        }
    }
}