using Newtonsoft.Json.Linq;

namespace Impact.Core.Matchers
{


    public class TypePropertyMatcher : IMatcher
    {
        public TypePropertyMatcher(string path)
        {
            PropertyPath = path;
        }

        public bool Matches(object expected, object actual)
        {
            return expected.GetType() == actual.GetType();
        }

        public string PropertyPath { get; }
        public JObject ToPactMatcher()
        {
            return new JObject
            {
                ["match"] = "type"
            };
        }

        public IMatcher Clone(string propertyPath)
        {
            return new TypePropertyMatcher(propertyPath);
        }

        public string FailureMessage(object expected, object actual)
        {
            if (expected.GetType() != actual.GetType())
            {
                return $"Expected type {expected.GetType().Name}, but got {actual.GetType().Name}";
            }

            return string.Empty;
        }
    }
}