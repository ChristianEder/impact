using System;
using System.Collections.Generic;
using Impact.Core.Matchers;
using Newtonsoft.Json.Linq;

namespace Impact.Core
{
    public class MatcherParser
    {
        public static IMatcher[] Parse(JObject rules)
        {
            List<IMatcher> matchers = new List<IMatcher>();

            foreach (var ruleArray in rules.Properties())
            {
                foreach (var rule in ruleArray.Value)
                {
                    var matcher = rule["match"].ToString();
                    switch (matcher)
                    {
                        case "regex":
                            matchers.Add(new RegexPropertyMatcher(ruleArray.Name.Replace("$.", string.Empty), rule["regex"].ToString()));
                            break;
                        case "type":

                            var max = rule["max"];
                            var min = rule["min"];
                            if (max != null)
                            {
                                matchers.Add(new TypeMaxPropertyMatcher(ruleArray.Name.Replace("$.", string.Empty), long.Parse(max.ToString())));
                            }
                            else if (min != null)
                            {
                                matchers.Add(new TypeMinPropertyMatcher(ruleArray.Name.Replace("$.", string.Empty), long.Parse(min.ToString())));
                            }
                            else
                            {
                                matchers.Add(new TypePropertyMatcher(ruleArray.Name.Replace("$.", string.Empty)));
                            }

                            break;
                        default:
                            throw new InvalidOperationException("Cannot parse matcher");
                    }
                }
            }

            return matchers.ToArray();
        }
    }
}