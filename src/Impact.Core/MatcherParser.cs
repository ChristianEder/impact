using System;
using System.Collections.Generic;
using System.Linq;
using Impact.Core.Matchers;
using Newtonsoft.Json.Linq;

namespace Impact.Core
{
    public class MatchingContext
    {
        public MatchingContext(IMatcher[] matchers, bool isRequest)
            : this(matchers, isRequest, new List<IPropertyPathPart>(), new MatchCheckResult())
        {
        }

        private MatchingContext(IMatcher[] matchers, bool isRequest, List<IPropertyPathPart> propertyPath, MatchCheckResult result)
        {
            PropertyPath = propertyPath;
            Matchers = matchers;
            MatchersForProperty = matchers.Where(m => m.AppliesTo(PropertyPath)).ToArray();
            IsRequest = isRequest;
            IgnoreExpected = false;
            Result = result;
        }

        public List<IPropertyPathPart> PropertyPath { get; }
        public IMatcher[] Matchers { get; }
        public IMatcher[] MatchersForProperty { get; }
        public bool IsRequest { get; }
        public bool IgnoreExpected { get; private set; }
        public MatchCheckResult Result { get; }
        public MatchingContext For(IPropertyPathPart property, bool? ignoreExpected = null)
        {
            return new MatchingContext(Matchers, IsRequest, new List<IPropertyPathPart>(PropertyPath) { property }, Result)
            {
                IgnoreExpected = ignoreExpected.GetValueOrDefault(IgnoreExpected)
            };
        }
    }

    public class MatcherParser
    {
        public static IMatcher[] Parse(JObject rules)
        {
            List<IMatcher> matchers = new List<IMatcher>();

            foreach (var ruleOrRuleArray in rules.Properties())
            {
                var ruleArray = ruleOrRuleArray.Value is JArray arr ? arr : new JArray(ruleOrRuleArray.Value);
                var propertyPath = ruleOrRuleArray.Name.Replace("$.", string.Empty);

                foreach (var rule in ruleArray)
                {
                    var matcher = rule["match"]?.ToString();
                    switch (matcher)
                    {
                        case "regex":
                            matchers.Add(new RegexPropertyMatcher(propertyPath, rule["regex"].ToString()));
                            break;
                        case "type":
                            AddTypeAndMinMaxMatchers(matchers, propertyPath, rule, true);
                            break;
                        case null:
                            AddTypeAndMinMaxMatchers(matchers, propertyPath, rule, false);
                            break;
                        default:
                            throw new InvalidOperationException("Cannot parse matcher");
                    }
                }
            }

            return matchers.ToArray();
        }

        private static void AddTypeAndMinMaxMatchers(List<IMatcher> matchers, string propertyPath, JToken rule, bool addTypeMatcher)
        {
            var max = rule["max"];
            var min = rule["min"];

            if (max != null)
            {
                matchers.Add(new TypeMaxPropertyMatcher(propertyPath, long.Parse(max.ToString())));
            }
            if (min != null)
            {
                matchers.Add(new TypeMinPropertyMatcher(propertyPath, long.Parse(min.ToString())));
            }

            if (addTypeMatcher && min == null && max == null)
            {
                matchers.Add(new TypePropertyMatcher(propertyPath));
            }
        }
    }
}