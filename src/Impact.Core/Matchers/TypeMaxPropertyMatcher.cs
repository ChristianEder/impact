using System;
using Newtonsoft.Json.Linq;

namespace Impact.Core.Matchers
{
    public class TypeMaxPropertyMatcher : TypeCountMatcher
    {
        private readonly long max;

        public TypeMaxPropertyMatcher(string path, long max) : base(path)
        {
            this.max = max;
        }

        public override bool Matches(object expected, object actual, MatchingContext context, Action<object, object, MatchingContext> deepMatch)
        {
            if (expected.GetType() != actual.GetType())
            {
                return false;
            }

            var count = Count(actual, true);

            return !count.HasValue || count.Value <= max;
        }
        
        public override JObject ToPactMatcher()
        {
            return new JObject
            {
                ["match"] = "type",
                ["max"] = max
            };
        }

        public override string FailureMessage(object expected, object actual)
        {
            if (expected.GetType() != actual.GetType())
            {
                return $"Expected type {expected.GetType().Name}, but got {actual.GetType().Name}";
            }

            var count = Count(actual, true);

            if (!count.HasValue)
            {
                return string.Empty;
            }
            

            return $"Expected <= {max} elements, but got {count.Value}";
        }
    }
}