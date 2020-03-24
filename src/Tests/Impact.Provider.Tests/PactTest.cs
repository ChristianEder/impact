using System;
using System.Threading.Tasks;
using Impact.Consumer.Tests;
using Impact.Core;
using Impact.Core.Payload.Json;
using Impact.Provider.Transport;
using Impact.Provider.Transport.Callbacks;
using Impact.Provider.Transport.Http;
using Impact.Tests.Shared;
using Newtonsoft.Json;
using Xunit;

namespace Impact.Provider.Tests
{
	public class PactTest
	{
		private Pact pact;

		private void GivenAPact(Func<Request, Response> responder)
		{
			var transport = new CallbackTransport(new NoTransportFormat(new JsonPayloadFormat()), async request =>
			{
				return responder(JsonConvert.DeserializeObject<Request>(request.ToString()));
			});
			pact = new Pact(PublishedPact.Get(), transport, new NoTransportMatchers());
		}

		[Fact]
		public async Task PassesWhenPactIsHonoured()
		{
			GivenAPact(PublishedPact.ValidRequestHandler);

			var verificationResult = await pact.Honour();

			Assert.True(verificationResult.Success);
		}

		[Fact]
		public async Task FailsWhenPactIsNotHonoured()
		{
			GivenAPact(request => new Response());
			var verificationResult = await pact.Honour();
			
			Assert.False(verificationResult.Success);			
		}

		[Fact]
		public async Task FailsWhenPactIsNotHonoured2()
		{
			GivenAPact(request =>
			{
				var response = PublishedPact.ValidRequestHandler((Request)request);
				response.Foos.ForEach(f => f.Id = Guid.NewGuid().ToString());
				response.Bars.ForEach(b => b.Id = Guid.NewGuid().ToString());
				return response;
			});
			var verificationResult = await pact.Honour();
			Assert.False(verificationResult.Success);
		}

		[Fact]
		public void CreatesAValidVerificationFileWhenPassing()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void CreatesAValidVerificationFileWhenFailing()
		{
			throw new NotImplementedException();
		}
	}
}
