using System;
using Impact.Consumer;
using System.Text.RegularExpressions;
using Impact.Core.Serialization;
using Impact.Core.Payload.Json;
using Impact.Core;
using Impact.Core.Transport.Http;

namespace Impact.Samples.ProviderMockAsAzureFunction
{
    /// <summary>
    /// In a real world scenario, you could use the same Pact defintions, payload formats and transport matchers that you also use
    /// When running unit tests against a local <see cref="Impact.Consumer.Transport.Http.HttpMockServer"/>
    /// </summary>
    public class PactDefinition
    {
        public static readonly Pact Pact;
        public static IPayloadFormat PayloadFormat = new JsonPayloadFormat();
        public static ITransportMatchers TransportMatchers = new PactV2CompliantHttpTransportMatchers();

        static PactDefinition()
        {
            Pact = new Pact();
            var pathFormat = new Regex("weatherforecast\\/([a-zA-Z]+)\\/(\\d+)");
            Pact.Given("")
                        .UponReceiving("A weather forecast request")
                        .With(new HttpRequest
                        {
                            Method = System.Net.Http.HttpMethod.Get,
                            Path = "weatherforecast/Munich/3"
                        })
                        .WithRequestMatchingRule(r => r.Path, r => r.Regex(pathFormat.ToString()))
                        .WillRespondWith(request =>
                        {
                            var match = pathFormat.Match(request.Path);
                            return new HttpResponse
                            {
                                Status = System.Net.HttpStatusCode.OK,
                                Body = new WeatherForecast
                                {
                                    City = match.Groups[1].Value,
                                    Date = new DateTime(2020, 3, 24),
                                    Summary = "Sunny",
                                    TemperatureC = 24,
                                    TemperatureF = 75
                                }
                            };
                        })
                        .WithResponseMatchingRule(r => ((WeatherForecast)r.Body), r => r.Type());
        }
    }
}
