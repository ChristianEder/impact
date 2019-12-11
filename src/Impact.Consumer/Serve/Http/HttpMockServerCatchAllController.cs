using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Impact.Consumer.Define;
using Microsoft.AspNetCore.Mvc;

namespace Impact.Consumer.Serve.Http
{
    [Route("")]
    public class HttpMockServerCatchAllController : Controller
    {
        private readonly Pact pact;
        private readonly IHttpBodyDeserializer deserializer;
        private readonly IHttpBodySerializer serializer;

        public HttpMockServerCatchAllController(Pact pact, IHttpBodyDeserializer deserializer, IHttpBodySerializer serializer)
        {
            this.pact = pact;
            this.deserializer = deserializer;
            this.serializer = serializer;
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
                Body = await deserializer.DeserializeFrom(Request)
            };

            HttpResponse response;
            try
            {
                response = pact.SendRequest<HttpRequest, HttpResponse>(request);
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
            await serializer.SerializeTo(response, Response);
        }
    }
}