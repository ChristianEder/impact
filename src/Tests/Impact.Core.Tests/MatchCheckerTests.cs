using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var actual = JsonConvert.DeserializeObject(testCase["actual"].ToString());
            var expected = JsonConvert.DeserializeObject(testCase["expected"].ToString());

            var matchChecker = new MatchChecker(new List<IMatcher>(), isRequest);
            var result = matchChecker.Matches(expected, actual);

            if (shouldMatch)
            {
                Assert.True(result.Matches, name + ": " + result.FailureReasons);
            }
            else
            {
                Assert.False(result.Matches, name + ": did match although it shouldn't");
            }
        }

        public static IEnumerable<object[]> V2SpecData
        {
            get
            {
                var testCaseDir = Path.Combine(Directory.GetCurrentDirectory(), "testcases", "v2");
                var testCases = Directory.GetFiles(testCaseDir, "*.json", SearchOption.AllDirectories);
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
