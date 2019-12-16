using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Impact.Consumer.Define;
using Impact.Core;
using Impact.Core.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Impact.Consumer.Serve.Http
{
    [Route("")]
    public class HttpMockServerCatchAllController : Controller
    {
        private readonly Pact pact;
        private readonly IPayloadFormat payloadFormat;
        private readonly ITransportMatchers matchers = new HttpTransportMatchers();

        public HttpMockServerCatchAllController(Pact pact, IPayloadFormat payloadFormat)
        {
            this.pact = pact;
            this.payloadFormat = payloadFormat;
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
            var request = new HttpRequest
            {
                Method = new HttpMethod(Request.Method),
                Path = Request.Path.Value,
                Query = Request.QueryString.Value,
                Headers = Request.Headers.Any()
                    ? new Dictionary<string, string>(Request.Headers.Select(h =>
                        new KeyValuePair<string, string>(h.Key, h.Value.ToString())))
                    : null,
                Body = payloadFormat.Deserialize(Request.Body)
            };

            HttpResponse response;
            try
            {
                var interaction = pact.GetMatchingInteraction(request, matchers);
                if (request.Body is JObject jsonBody && interaction.RequestType != typeof(JObject))
                {
                    request.Body = payloadFormat.Deserialize(jsonBody, interaction.RequestType);
                }

                response = (HttpResponse) interaction.Respond(request, matchers);
            }
            catch
            {
                Response.StatusCode = 404;
                await Response.Body.WriteAsync(Encoding.UTF8.GetBytes("Getting a response from the pact failed for this request"));
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