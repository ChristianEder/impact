using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Impact.Consumer.Serve.Callbacks;
using Impact.Consumer.Serve.Http;
using Impact.Core;
using Impact.Core.Matchers;
using Impact.Tests.Shared;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Impact.Consumer.Tests
{
    public class MockServerTest
    {
        [Theory]
        [MemberData(nameof(V2SpecData))]
        public void Matcher(string name, bool isRequest, JObject testCase, string fileName)
        {
            var matchChecker = new MatchChecker();
            DoTest(name, isRequest, testCase, fileName, matchChecker.Matches);
        }

        [Theory]
        [MemberData(nameof(V2SpecData))]
        public void Matcher2(string name, bool isRequest, JObject testCase, string fileName)
        {
            var matchChecker = new MatchChecker2();
            DoTest(name, isRequest, testCase, fileName, matchChecker.Matches);
        }

        private static void DoTest(string name, bool isRequest, JObject testCase, string fileName, Action<object, object, MatchingContext> match)
        {
            if (fileName ==
                @"C:\prj\private\impact\src\Tests\Impact.Core.Tests\testcases\v2\request\method\different method.json"
            )
            {

            }

            var shouldMatch = (bool)testCase["match"];
            var actual = testCase["actual"];
            var expected = testCase["expected"];

            Assert.Null(actual["matchingRules"]);
            var rulesJsonProperty = expected["matchingRules"];
            var rules = new IMatcher[0];
            if (rulesJsonProperty != null)
            {
                rules = MatcherParser.Parse(rulesJsonProperty as JObject);
                ((JObject)expected).Remove("matchingRules");
            }

            rules = (isRequest ? new HttpTransportMatchers().RequestMatchers : new HttpTransportMatchers().ResponseMatchers).Concat(rules).ToArray();
            var context = new MatchingContext(rules, isRequest);

            match(expected, actual, context);

            if (shouldMatch)
            {
                if (!context.Result.Matches)
                {
                }

                Assert.True(context.Result.Matches, name + ": " + context.Result.FailureReasons);
            }
            else
            {
                if (context.Result.Matches)
                {
                }

                Assert.False(context.Result.Matches, name + ": did match although it shouldn't");
            }
        }

        public static IEnumerable<object[]> V2SpecData
        {
            get
            {
                var testCaseDir = @"C:\prj\private\impact\src\Tests\Impact.Core.Tests\testcases\v2";
                var files = Directory.GetFiles(testCaseDir, "*.json", SearchOption.AllDirectories);

                var testCases = files.Where(f => !new FileInfo(f).Name.ToLower().Contains("xml"));
                return testCases.Select(f =>
                {
                    var name = string.Join(" ",
                        f.Replace(testCaseDir, string.Empty).Trim().Replace("\\", "/").Replace(".json", string.Empty)
                            .Split("/", StringSplitOptions.RemoveEmptyEntries));

                    var testCase = JObject.Parse(File.ReadAllText(f));

                    return new object[]
                    {
                        name,
                        name.StartsWith("request"),
                        testCase,
                        f
                    };
                }).ToArray();
            }
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
