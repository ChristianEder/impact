using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Impact.Consumer.Serve.Http;
using Impact.Core.Matchers;
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

            var transportMatchers = new PactV2CompliantHttpTransportMatchers();

            if (isRequest)
            {
                rules = transportMatchers.RequestMatchers.Concat(rules).ToArray();
            }
            else
            {
                rules = transportMatchers.ResponseMatchers.Concat(rules).ToArray();
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
                var testCases = Directory.GetFiles(testCaseDir, "*.json", SearchOption.AllDirectories).Where(f => new FileInfo(f).Name.ToLower().Contains("xml"));
                return testCases.Select(f =>
                {
                    var name = string.Join(" ",
                        f.Replace(testCaseDir, string.Empty).Trim().Replace("\\", "/").Replace(".json", string.Empty)
                            .Split("/", StringSplitOptions.RemoveEmptyEntries));



                    var testCase = JObject.Parse(File.ReadAllText(f));

                    var isXmlTestCase = new FileInfo(f).Name.ToLower().Contains("xml");
                    if (isXmlTestCase)
                    {
                        // C:\prj\private\impact\src\Tests\Impact.Core.Tests\testcases\v2\response\body\.json
                        if (name.Contains("response body array at top level with matchers xml"))
                        {

                        }
                        try
                        {
                            BodyXmlToJson(testCase, "expected");
                            BodyXmlToJson(testCase, "actual");
                        }
                        catch (Exception e)
                        {
                        }
                    }


                    name += (bool)testCase["match"] ? " matches" : " does not match";
                    return new object[]
                    {
                        name,
                        name.StartsWith("request"),
                        testCase
                    };
                })
                //.Where(t => (bool)t[1] == false && (t[0].ToString().StartsWith("response body objects in array type matching") ||
                //t[0].ToString().StartsWith("response body objects in array with type mismatching")))
                .ToArray();

            }
        }

        private static void BodyXmlToJson(JObject testCase, string which)
        {
            var xmlString = testCase[which]?["body"]?.ToString();
            if (string.IsNullOrEmpty(xmlString))
            {
                return;
            }
            var xmlBody = XDocument.Parse(xmlString);
            var jsonBody = XmlToJson(xmlBody.Root, true);
            testCase[which]["body"] = jsonBody;
        }

        private static JObject XmlToJson(XElement element, bool isRoot)
        {
            var o = new JObject();
            if (isRoot)
            {
                o["__xmlElementName"] = element.Name.LocalName;
            }

            foreach (var attribute in element.Attributes())
            {
                o["__xmlAttribute_" + attribute.Name.LocalName] = attribute.Value;
            }

            if (!element.Elements().Any())
            {
                o["__xmlValue"] = element.Value;
            }
            else
            {
                var childrenArrays = element.Elements().GroupBy(e => e.Name.LocalName);
                foreach (var childArray in childrenArrays)
                {
                    o[childArray.Key] = new JArray(childArray.Select(x => XmlToJson(x, false)).ToArray());
                }
            }

            return o;
        }
    }
}
