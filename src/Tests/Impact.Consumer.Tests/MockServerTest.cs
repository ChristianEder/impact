using System;
using System.Collections.Generic;
using System.IO;
using Impact.Consumer.Serve;
using Impact.Core;
using Impact.Core.Matchers;
using Impact.Tests.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Impact.Consumer.Tests
{
    public class MockServerTest
    {
        [Fact]
        public void Xlkjsdlf()
        {
            var testCase = JObject.Parse(File.ReadAllText(@"C:\prj\private\impact\src\Tests\Impact.Core.Tests\testcases\v2\request\body\array with at least one element not matching example type.json"));


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

            var m = new MatchChecker(rules, false);

            var result = m.Matches(expected, testCase["actual"]);
        }

        [Fact]
        public void ReturnsExpectedResponses()
        {
            var mockServer = new MockServer(PublishedPact.DefinePact());

            var response = mockServer.SendRequest<Request, Response>(new Request { Type = "Foo", Ids = { "3", "4" } });

            Assert.Empty(response.Bars);
            Assert.Equal(2, response.Foos.Count);
            Assert.Contains(response.Foos, f => f.Id == "3");
            Assert.Contains(response.Foos, f => f.Id == "4");
        }

        [Fact]
        public void FailsOnUnexpectedRequests()
        {
            var mockServer = new MockServer(PublishedPact.DefinePact());

            Assert.ThrowsAny<Exception>(() => mockServer.SendRequest<Request, Response>(new Request { Type = "Bar" }));
            Assert.ThrowsAny<Exception>(() => mockServer.SendRequest<Request, Response>(new Request { Type = "Baz", Ids = { "3", "4" } }));
        }

        [Fact]
        public void PassesVerificationIfAllInteractionsWhereCalled()
        {
            var pact = PublishedPact.DefinePact();
            var mockServer = new MockServer(pact);

            mockServer.SendRequest<Request, Response>(new Request { Type = "Foo", Ids = { "3", "4" } });
            mockServer.SendRequest<Request, Response>(new Request { Type = "Bar", Ids = { "3", "4" } });

            pact.VerifyAllInteractionsWhereCalled();
        }


        [Fact]
        public void FailsVerificationIfNotAllInteractionsWhereCalled()
        {
            var pact = PublishedPact.DefinePact();
            var mockServer = new MockServer(pact);

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
