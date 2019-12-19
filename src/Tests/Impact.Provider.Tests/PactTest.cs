using System;
using Impact.Consumer.Tests;
using Impact.Core;
using Impact.Tests.Shared;
using Xunit;

namespace Impact.Provider.Tests
{
    public class PactTest
    {
        private readonly Pact pact;

        public PactTest()
        {
            pact = new Pact(PublishedPact.Get(), new JsonRequestResponseDeserializer<Request, Response>(), new NoTransportMatchers());
        }

        [Fact]
        public void PassesWhenPactIsHonoured()
        {
            var verificationResult = pact.Honour(request => PublishedPact.ValidRequestHandler((Request)request));

            Assert.True(verificationResult.Success);
        }

        [Fact]
        public void FailsWhenPactIsNotHonoured()
        {
            var verificationResult = pact.Honour(request => new Response());
            Assert.False(verificationResult.Success);

            verificationResult = pact.Honour(request =>
            {
                var response = PublishedPact.ValidRequestHandler((Request) request);
                response.Foos.ForEach(f => f.Id = Guid.NewGuid().ToString());
                response.Bars.ForEach(b => b.Id = Guid.NewGuid().ToString());
                return response;
            });
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
