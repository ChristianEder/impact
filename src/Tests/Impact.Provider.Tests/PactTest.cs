using System;
using Impact.Consumer.Tests;
using Impact.Tests.Shared;
using Xunit;

namespace Impact.Provider.Tests
{
    public class PactTest
    {
        private readonly Pact pact;

        public PactTest()
        {
            pact = new Pact(PublishedPact.Get(), new JsonRequestResponseDeserializer<Request, Response>());
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
        }

        [Fact]
        public void CreatesAValidVerificationFile()
        {
            throw new NotImplementedException();
        }
    }
}
