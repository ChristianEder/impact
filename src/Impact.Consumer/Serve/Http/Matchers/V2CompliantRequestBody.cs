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

        public override bool AppliesTo(object expected, object actual, MatchingContext context)
        {
            return base.AppliesTo(expected, actual, context) && expected == null && actual is JObject;
        }

        public override bool Matches(object expected, object actual, MatchingContext context, Action<object, object, MatchingContext> deepMatch)
        {
            var actualBody = (JObject) actual;

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

        public override bool IsTerminal => false;
    }
}