using System;
using Impact.Consumer.Transport.Callbacks;
using Impact.Tests.Shared;
using Xunit;

namespace Impact.Consumer.Tests
{
	public class MockServerTest
    {
        [Fact]
        public void ReturnsExpectedResponses()
        {
            var mockServer = new CallbacksMockServer(PublishedPact.DefinePact());

            var response = mockServer.SendRequest<Request, Response>(new Request { Type = "Foo", Ids = { "3", "4" } });

            Assert.Empty(response.Bars);
            Assert.Equal(2, response.Foos.Count);
            Assert.Contains(response.Foos, f => f.Id == "3");
            Assert.Contains(response.Foos, f => f.Id == "4");
        }

        [Fact]
        public void FailsOnUnexpectedRequests()
        {
            var mockServer = new CallbacksMockServer(PublishedPact.DefinePact());

            Assert.ThrowsAny<Exception>(() => mockServer.SendRequest<Request, Response>(new Request { Type = "Bar" }));
            Assert.ThrowsAny<Exception>(() => mockServer.SendRequest<Request, Response>(new Request { Type = "Baz", Ids = { "3", "4" } }));
        }

        [Fact]
        public void PassesVerificationIfAllInteractionsWhereCalled()
        {
            var pact = PublishedPact.DefinePact();
            var mockServer = new CallbacksMockServer(pact);

            mockServer.SendRequest<Request, Response>(new Request { Type = "Foo", Ids = { "3", "4" } });
            mockServer.SendRequest<Request, Response>(new Request { Type = "Bar", Ids = { "3", "4" } });

            pact.VerifyAllInteractionsWhereCalled();
        }


        [Fact]
        public void FailsVerificationIfNotAllInteractionsWhereCalled()
        {
            var pact = PublishedPact.DefinePact();
            var mockServer = new CallbacksMockServer(pact);

            mockServer.SendRequest<Request, Response>(new Request { Type = "Foo", Ids = { "3", "4" } });

            Assert.ThrowsAny<Exception>(() => pact.VerifyAllInteractionsWhereCalled());
        }

        [Fact]
        public void GeneratesValidAndCompletePactFile()
        {
            throw new NotImplementedException();
        }
    }
}
