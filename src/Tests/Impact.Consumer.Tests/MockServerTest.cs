using System;
using Impact.Tests.Shared;
using Xunit;

namespace Impact.Consumer.Tests
{
    public class MockServerTest
    {
        [Fact]
        public void ReturnsExpectedResponses()
        {
            var mockServer = PublishedPact.CreateMockServer();

            var response = mockServer.SendRequest<Request, Response>(new Request { Type = "Foo", Ids = { "3", "4" } });

            Assert.Empty(response.Bars);
            Assert.Equal(2, response.Foos.Count);
            Assert.Contains(response.Foos, f => f.Id == "3");
            Assert.Contains(response.Foos, f => f.Id == "4");
        }

        [Fact]
        public void FailsOnUnexpectedRequests()
        {
            var mockServer = PublishedPact.CreateMockServer();

            Assert.ThrowsAny<Exception>(() => mockServer.SendRequest<Request, Response>(new Request { Type = "Foo", Ids = { "3", "4" } }));
            Assert.ThrowsAny<Exception>(() => mockServer.SendRequest<Request, Response>(new Request { Type = "Bar" }));
            Assert.ThrowsAny<Exception>(() => mockServer.SendRequest<Request, Response>(new Request { Type = "Baz", Ids = { "3", "4" } }));
        }

        [Fact]
        public void FailsOnUnexpectedResponses()
        {
            var mockServer = PublishedPact.CreateMockServer(r => new Response());

            Assert.ThrowsAny<Exception>(() => mockServer.SendRequest<Request, Response>(new Request { Type = "Foo", Ids = { "3", "4" } }));
            Assert.ThrowsAny<Exception>(() => mockServer.SendRequest<Request, Response>(new Request { Type = "Bar", Ids = { "3", "4" } }));
        }


        [Fact]
        public void PassesVerificationIfAllInteractionsWhereCalled()
        {
            var mockServer = PublishedPact.CreateMockServer();

            mockServer.SendRequest<Request, Response>(new Request { Type = "Foo", Ids = { "3", "4" } });
            mockServer.SendRequest<Request, Response>(new Request { Type = "Bar", Ids = { "3", "4" } });

            mockServer.VerifyAllInteractionsWhereCalled();
        }


        [Fact]
        public void FailsVerificationIfNotAllInteractionsWhereCalled()
        {
            var mockServer = PublishedPact.CreateMockServer();

            mockServer.SendRequest<Request, Response>(new Request { Type = "Foo", Ids = { "3", "4" } });

            Assert.ThrowsAny<Exception>(() => mockServer.VerifyAllInteractionsWhereCalled());
        }

        [Fact]
        public void GeneratesValidAndCompletePactFile()
        {
            throw new NotImplementedException();
        }
    }
}
