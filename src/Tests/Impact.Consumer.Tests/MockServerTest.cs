using System;
using System.IO;
using System.Linq;
using Impact.Consumer.Serve.Callbacks;
using Impact.Consumer.Serve.Http.Matchers;
using Impact.Core;
using Impact.Core.Matchers;
using Impact.Tests.Shared;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Impact.Consumer.Tests
{
    public class MockServerTest
    {
        [Fact]
        public void Xlkjsdlf()
        {
            var testCase = JObject.Parse(File.ReadAllText(@"C:\prj\private\impact\src\Tests\Impact.Core.Tests\testcases\v2\request\body\.json"));
            var expected = (JObject)testCase["expected"];
            var rulesJsonProperty = expected["matchingRules"];

            if (testCase["actual"]["matchingRules"] != null)
            {
                throw new NotImplementedException();
            }

            var rules = new IMatcher[0];

            if (rulesJsonProperty != null)
            {
                rules = MatcherParser.Parse(rulesJsonProperty as JObject);
                expected.Remove("matchingRules");
            }

            rules = new[] { new RequestHeadersDoNotFailPostelsLaw() }.Concat(rules).ToArray();

            var c = new MatchingContext(rules, true);
            var m = new MatchChecker();
            m.Matches(expected, testCase["actual"], c);

            var result = c.Result;
            var matches = result.Matches;
            var shouldMatch = (testCase["match"] as JValue).Value<bool>();


        }

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
