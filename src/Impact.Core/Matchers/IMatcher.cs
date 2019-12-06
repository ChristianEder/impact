using Newtonsoft.Json.Linq;

namespace Impact.Core.Matchers
{
    public interface IMatcher
    {
        bool Matches(object expected, object actual);
        string PropertyPath { get; }

        JObject ToPactMatcher();

        IMatcher Clone(string propertyPath);

        string FailureMessage(object expected, object actual);
    }
}