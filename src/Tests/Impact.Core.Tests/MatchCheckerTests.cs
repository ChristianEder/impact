using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Impact.Consumer.Serve.Http;
using Impact.Consumer.Serve.Http.Matchers;
using Impact.Core.Matchers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Impact.Core.Tests
{
    public class MatchCheckerTests
    {
        [Theory]
        [MemberData(nameof(V2SpecData))]
        public void V2Specs(string name, bool isRequest, JObject testCase)
        {
            var shouldMatch = (bool) testCase["match"];
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

            if (isRequest)
            {
                rules = new HttpTransportMatchers().RequestMatchers.Concat(rules).ToArray();
            }
            else
            {
                rules = new HttpTransportMatchers().ResponseMatchers.Concat(rules).ToArray();
            }

            var context = new MatchingContext(rules, isRequest);
            var matchChecker = new MatchChecker();
            matchChecker.Matches(expected, actual, context);

            if (shouldMatch)
            {
                Assert.True(context.Result.Matches, name + ": " + context.Result.FailureReasons);
            }
            else
            {
                Assert.False(context.Result.Matches, name + ": did match although it shouldn't");
            }
        }

        public static IEnumerable<object[]> V2SpecData
        {
            get
            {
                var testCaseDir = Path.Combine(Directory.GetCurrentDirectory(), "testcases", "v2");
                var testCases = Directory.GetFiles(testCaseDir, "*.json", SearchOption.AllDirectories).Where(f => !new FileInfo(f).Name.ToLower().Contains("xml"));
                return testCases.Select(f =>
                {
                    var name = string.Join(" ",
                        f.Replace(testCaseDir, string.Empty).Trim().Replace("\\", "/").Replace(".json", string.Empty)
                            .Split("/", StringSplitOptions.RemoveEmptyEntries));

                    var testCase = JObject.Parse(File.ReadAllText(f));

                    name += (bool) testCase["match"] ? " matches" : " does not match";
                    return new object[]
                    {
                        name,
                        name.StartsWith("request"),
                        testCase
                    };
                }).ToArray();
            }
        }
    }
}
