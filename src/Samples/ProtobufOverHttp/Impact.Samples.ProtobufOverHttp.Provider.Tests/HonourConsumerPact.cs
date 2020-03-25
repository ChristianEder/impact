using Impact.Core.Payload.Protobuf;
using Impact.Provider;
using Impact.Provider.Transport.Http;
using Impact.Samples.ProtobufOverHttp.Model;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Impact.Samples.ProtobufOverHttp.Provider.Tests
{
	public class HonourConsumerPact : IDisposable
	{
		private readonly Pact pact;
		private HttpClient client;

		public HonourConsumerPact()
		{
			client = new HttpClient();
			client.BaseAddress = new Uri("http://localhost:60374/");
			pact = new Pact(ConsumerPact.GetPactFile(), new HttpTransport(client, new ProtobufPayloadFormat<WeatherForecast>()));
		}

		[Fact]
		public async Task HonourPact()
		{
			var result = await pact.Honour();
			// In a real world scenario, you'd now publish the verification results to a pact broker or other central repository
			Assert.True(result.Success, string.Join(Environment.NewLine, result.Results.Select(r => r.FailureReason)));
		}

		public void Dispose()
		{
			client.Dispose();
		}
	}
}
