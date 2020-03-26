using System;
using System.Threading.Tasks;
using Impact.Consumer.Tests;
using Impact.Core.Payload.Json;
using Impact.Provider.Transport;
using Impact.Provider.Transport.Callbacks;
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
			pact = new Pact(PublishedPact.Get(), transport, s => Task.CompletedTask);
		}

		[Fact]
		public async Task<VerificationResult> PassesWhenPactIsHonoured()
		{
			GivenAPact(PublishedPact.ValidRequestHandler);

			var verificationResult = await pact.Honour();

			Assert.True(verificationResult.Success);

			return verificationResult;
		}

		[Fact]
		public async Task<VerificationResult> FailsWhenPactIsNotHonoured()
		{
			GivenAPact(request => new Response());
			var verificationResult = await pact.Honour();
			
			Assert.False(verificationResult.Success);

			return verificationResult;
		}

		[Fact]
		public async Task FailsWhenPactIsNotHonoured2()
		{
			GivenAPact(request =>
			{
				var response = PublishedPact.ValidRequestHandler(request);
				response.Foos.ForEach(f => f.Id = Guid.NewGuid().ToString());
				response.Bars.ForEach(b => b.Id = Guid.NewGuid().ToString());
				return response;
			});
			var verificationResult = await pact.Honour();
			Assert.False(verificationResult.Success);
		}

		[Fact]
		public async Task CreatesAValidVerificationFileWhenPassing()
		{
			var result = await PassesWhenPactIsHonoured();
			// TBD
		}

		[Fact]
		public async Task CreatesAValidVerificationFileWhenFailing()
		{
			var result = await FailsWhenPactIsNotHonoured();
			// TBD
		}
	}
}
