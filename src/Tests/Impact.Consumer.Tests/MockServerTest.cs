using System;
using System.Linq;
using Impact.Consumer.Transport;
using Impact.Consumer.Transport.Callbacks;
using Impact.Core.Payload.Json;
using Impact.Tests.Shared;
using Newtonsoft.Json.Linq;
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
            var pact = PublishedPact.DefinePact();
            var pactFile = JObject.Parse(pact.ToPactFile("consumer", "provider", new NoTransportFormat(new JsonPayloadFormat())));

            Assert.Equal("consumer", pactFile["consumer"]["name"].ToString());
            Assert.Equal("provider", pactFile["provider"]["name"].ToString());

            var interactions = pactFile["interactions"] as JArray;

            Assert.NotNull(interactions);
            Assert.Equal(2, interactions.Count);

            var rules = interactions[0]["matchingRules"] as JObject;

            Assert.NotNull(rules);
            Assert.Equal(3, rules.Properties().Count());

            foreach (var ruleProperty in rules.Properties())
            {
                var rule = ruleProperty.Value as JArray;
                Assert.NotNull(rule);
                Assert.Single(rule);

                var expectedType = ruleProperty.Name.ToLowerInvariant().EndsWith(".id")
                    ? "regex"
                    : "type";
                Assert.Equal(expectedType, rule[0]["match"].ToString());
            }
        }
    }
}
