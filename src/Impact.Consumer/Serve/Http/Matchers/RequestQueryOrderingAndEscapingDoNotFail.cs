using System;
using System.Linq;
using System.Web;
using Impact.Core;
using Impact.Core.Matchers;
using Newtonsoft.Json.Linq;

namespace Impact.Consumer.Serve.Http.Matchers
{
    public class RequestQueryOrderingAndEscapingDoNotFail : Matcher
    {
        public RequestQueryOrderingAndEscapingDoNotFail() : base(nameof(HttpRequest.Query))
        {
        }

        public override bool Matches(object expected, object actual, MatchingContext context, Action<object, object, MatchingContext> deepMatch)
        {
            var expectedQuery = expected?.ToString()?.Trim()?.TrimStart('?')?.TrimEnd('&');
            var actualQuery = actual?.ToString()?.Trim()?.TrimStart('?')?.TrimEnd('&');

            if (string.IsNullOrEmpty(expectedQuery) && string.IsNullOrEmpty(actualQuery))
            {
                return true;
            }

            if (string.IsNullOrEmpty(expectedQuery) || string.IsNullOrEmpty(actualQuery))
            {
                return false;
            }

            var expectedValues = HttpUtility.ParseQueryString(expectedQuery);
            var actualValues = HttpUtility.ParseQueryString(actualQuery);

            if (expectedValues.AllKeys.Except(actualValues.AllKeys).Any() ||
                actualValues.AllKeys.Except(expectedValues.AllKeys).Any())
            {
                return false;
            }

            return expectedValues.AllKeys.All(k => expectedValues.Get(k).Equals(actualValues.Get(k)));
        }

        public override JObject ToPactMatcher()
        {
            throw new InvalidOperationException("This matcher should not be part of the pact file");
        }

        public override string FailureMessage(object expected, object actual)
        {
            return $"Expected query \"{expected}\", but got \"{actual}\"";
        }
    }
}