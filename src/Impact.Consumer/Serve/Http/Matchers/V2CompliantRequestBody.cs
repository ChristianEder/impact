using System;
using Impact.Core;
using Impact.Core.Matchers;
using Newtonsoft.Json.Linq;

namespace Impact.Consumer.Serve.Http.Matchers
{
    public class V2CompliantRequestBody : Matcher
    {
        public V2CompliantRequestBody() : base(nameof(HttpRequest.Body))
        {
        }

        public override bool Matches(object expected, object actual, MatchingContext context, Action<object, object, MatchingContext> deepMatch)
        {
            if (expected != null || !(actual is JObject actualBody))
            {
                return false;
            }

            foreach (var property in actualBody.Properties())
            {
                deepMatch(null, property.Value, context.For(new PropertyPathPart(property.Name)));
            }

            return true;
        }

        public override JObject ToPactMatcher()
        {
            throw new InvalidOperationException("This matcher should not be part of the pact file");
        }

        public override string FailureMessage(object expected, object actual)
        {
            return "Request bodies do not match";
        }
    }
}