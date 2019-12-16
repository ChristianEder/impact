using System;
using Newtonsoft.Json.Linq;

namespace Impact.Core.Matchers
{
    public class TypePropertyMatcher : Matcher
    {
        public TypePropertyMatcher(string path) : base(path)
        {
        }

        public override bool Matches(object expected, object actual, MatchingContext context, Action<object, object, MatchingContext> deepMatch)
        {
            return expected.GetType() == actual.GetType();
        }

        public override JObject ToPactMatcher()
        {
            return new JObject
            {
                ["match"] = "type"
            };
        }

        public override string FailureMessage(object expected, object actual)
        {
            if (expected.GetType() != actual.GetType())
            {
                return $"Expected type {expected.GetType().Name}, but got {actual.GetType().Name}";
            }

            return string.Empty;
        }
    }
}