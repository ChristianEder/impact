using Newtonsoft.Json.Linq;

namespace Impact.Core.Matchers
{
    public class TypeMinPropertyMatcher : TypeCountMatcher
    {
        private readonly long min;

        public TypeMinPropertyMatcher(string path, long min) : base(path)
        {
            this.min = min;
        }

        public override bool Matches(object expected, object actual)
        {
            if (expected.GetType() != actual.GetType())
            {
                return false;
            }

            var count = Count(actual, true);

            return !count.HasValue || count.Value >= min;
        }
        
        public override JObject ToPactMatcher()
        {
            return new JObject
            {
                ["match"] = "type",
                ["min"] = min
            };
        }

        public override IMatcher Clone(string propertyPath)
        {
            return new TypeMaxPropertyMatcher(propertyPath, min);
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


            return $"Expected >= {min} elements, but got {count.Value}";
        }
    }
}