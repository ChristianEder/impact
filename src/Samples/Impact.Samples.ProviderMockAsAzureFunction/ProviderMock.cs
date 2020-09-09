using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System;

namespace Impact.Samples.ProviderMockAsAzureFunction
{
    public static class ProviderMock
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, Route = "{*route}")] HttpRequest req,
            string route)
        {
            var requestBody = new MemoryStream();
            await req.Body.CopyToAsync(requestBody);
            requestBody.Seek(0, SeekOrigin.Begin);
            var request = new Core.Transport.Http.HttpRequest
            {
                Method = new HttpMethod(req.Method),
                Path = route,
                Query = req.QueryString.Value,
                Body = PactDefinition.PayloadFormat.Deserialize(requestBody)
            };

            if (req.Headers.Any())
            {
                request.Headers = request.Headers ?? new Dictionary<string, string>();
                foreach (var header in req.Headers)
                {
                    request.Headers[header.Key] = header.Value;
                }
            }

            Core.Transport.Http.HttpResponse response;
            try
            {
                var interaction = PactDefinition.Pact.GetMatchingInteraction(request, PactDefinition.TransportMatchers);
                if (request.Body is JObject jsonBody && interaction.RequestType != typeof(JObject))
                {
                    request.Body = PactDefinition.PayloadFormat.Deserialize(jsonBody, interaction.RequestType);
                }

                response = (Core.Transport.Http.HttpResponse)interaction.Respond(request, PactDefinition.TransportMatchers);
            }
            catch (Exception ex)
            {
                return new NotFoundObjectResult("Getting a response from the pact failed for this request. Exception: " + Environment.NewLine + ex);
            }

            if (response.Headers != null)
            {
                foreach (var header in response.Headers)
                {
                    req.HttpContext.Response.Headers.Add(header.Key, header.Value);
                }
            }

            if (response.Body != null)
            {
                var memoryStream = new MemoryStream();
                PactDefinition.PayloadFormat.Serialize(response.Body,memoryStream);

                return new FileContentResult(memoryStream.ToArray(),
                    string.IsNullOrEmpty(PactDefinition.PayloadFormat.MimeType)
                    ? "application/octet-stream" 
                    :  PactDefinition.PayloadFormat.MimeType);
            }
            else
            {
                return new StatusCodeResult((int)response.Status);
            }
        }
    }
}
