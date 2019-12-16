using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Impact.Core.Matchers
{
    public class RegexPropertyMatcher : Matcher
    {
        private readonly Regex regex;

        public RegexPropertyMatcher(string path, string regex) : base(path)
        {
            this.regex = new Regex(regex, RegexOptions.Compiled);
        }

        public override bool Matches(object expected, object actual, MatchingContext context, Action<object, object, MatchingContext> deepMatch)
        {
            if (ReferenceEquals(actual, null))
            {
                return false;
            }

            return regex.IsMatch(actual.ToString());
        }

        public override JObject ToPactMatcher()
        {
            return new JObject
            {
                ["match"] = "regex",
                ["regex"] = regex.ToString()
            };
        }

        public override string FailureMessage(object expected, object actual)
        {
            return $"Expected regex format {regex}, but got {actual}";
        }
    }
}