using Impact.Consumer;
using Impact.Consumer.Transport.Http;
using Impact.Core.Payload.Json;
using Impact.Core.Transport.Http;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Impact.Samples.JsonOverHttp.Consumer.Tests
{

	public class PrintWeatherForecastUseCaseTest : IDisposable
	{
		private readonly Pact pact;
		private readonly Regex pathFormat = new Regex("weatherforecast\\/([a-zA-Z]+)\\/(\\d+)");
		private readonly HttpMockServer server;

		public PrintWeatherForecastUseCaseTest()
		{
			pact = new Pact();
			pact.Given("")
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

			server = new HttpMockServer(pact, new JsonPayloadFormat());
			server.Start();
		}

		[Fact]
		public async Task ShouldPrintTheForecast()
		{
			var console = new TestConsole();
			var useCase = new PrintWeatherForecastUseCase(new WeatherForecastApiClient(server.CreateClient()), console);

			await useCase.Print("Berlin");

			Assert.Single(console.Messages);
			Assert.Equal("The weather in Berlin on 24.03.2020 will be Sunny at 24°C", console.Messages.Single());
		}

		[Fact]
		public async Task ShouldFailForInvalidRequest()
		{
			var console = new TestConsole();
			var useCase = new PrintWeatherForecastUseCase(new WeatherForecastApiClient(server.CreateClient()), console);

			// City names with umlauts are not supported by the provider
			await Assert.ThrowsAnyAsync<Exception>(() => useCase.Print("München"));
		}

		public void Dispose()
		{
			// In a real world scenario, you'd now publish the pact file to a pact broker or other central repository
			var pactFile = pact.ToPactFile("Weather Forecast consumer", "Weather Forecast API", server.TransportFormat);
			server.Dispose();
		}
	}
}
