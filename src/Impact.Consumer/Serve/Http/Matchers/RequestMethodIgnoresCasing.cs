using System;
using Impact.Core;
using Impact.Core.Matchers;
using Newtonsoft.Json.Linq;

namespace Impact.Consumer.Serve.Http.Matchers
{
    public class RequestMethodIgnoresCasing : Matcher
    {
        public RequestMethodIgnoresCasing() : base(nameof(HttpRequest.Method))
        {
        }

        public override bool Matches(object expected, object actual, MatchingContext context, Action<object, object, MatchingContext> deepMatch)
        {
            var expectedMethod = expected?.ToString() ?? "";
            var actualMethod = actual?.ToString() ?? "";

            return actualMethod.Equals(expectedMethod, StringComparison.InvariantCultureIgnoreCase);
        }

        public override JObject ToPactMatcher()
        {
            throw new InvalidOperationException("This matcher should not be part of the pact file");
        }

        public override string FailureMessage(object expected, object actual)
        {
            return "Expteced HTTP method " + expected + ", but got " + actual;
        }
    }
}