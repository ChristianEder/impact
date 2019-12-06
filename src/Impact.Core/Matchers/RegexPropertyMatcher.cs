using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Impact.Core.Matchers
{
    public class RegexPropertyMatcher : IMatcher
    {
        private readonly Regex regex;

        public RegexPropertyMatcher(string regex, string path)
        {
            PropertyPath = path;
            this.regex = new Regex(regex, RegexOptions.Compiled);
        }

        public bool Matches(object expected, object actual)
        {
            if (ReferenceEquals(actual, null))
            {
                return false;
            }

            return regex.IsMatch(actual.ToString());
        }

        public string PropertyPath { get; }
        public JObject ToPactMatcher()
        {
            return new JObject
            {
                ["match"] = "regex",
                ["regex"] = regex.ToString()
            };
        }

        public IMatcher Clone(string propertyPath)
        {
            return new RegexPropertyMatcher(regex.ToString(), propertyPath);
        }

        public string FailureMessage(object expected, object actual)
        {
            return $"Expected regex format {regex}, but got {actual}";
        }
    }
}