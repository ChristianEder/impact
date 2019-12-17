using System;
using System.Linq;
using Impact.Core;
using Impact.Core.Matchers;
using Newtonsoft.Json.Linq;

namespace Impact.Consumer.Serve.Http.Matchers
{
    public class RequestHeadersDoNotFailPostelsLaw : Matcher
    {
        public RequestHeadersDoNotFailPostelsLaw() : base(nameof(HttpRequest.Headers))
        {
        }

        public override bool Matches(object expected, object actual, MatchingContext context, Action<object, object, MatchingContext> deepMatch)
        {
            var e = (JObject)expected ?? new JObject();
            var a = (JObject)actual ?? new JObject();

            var expectedHeaders = e.Properties().Select(p => p.Name.ToLowerInvariant()).ToArray();
            var actualHeaders = a.Properties().Select(p => p.Name.ToLowerInvariant()).ToArray();

            foreach (var header in actualHeaders)
            {
                var expectedHeader = e.Properties().FirstOrDefault(p => p.Name.Equals(header, StringComparison.InvariantCultureIgnoreCase))?.Value as JValue;
                var actualHeader = a.Properties().FirstOrDefault(p => p.Name.Equals(header, StringComparison.InvariantCultureIgnoreCase))?.Value as JValue;

                var childContext = context.For(new PropertyPathPart(header), ignoreExpected: expectedHeader == null);
                
                deepMatch(expectedHeader ?? JValue.CreateString(""), actualHeader, childContext);
            }

            context.Terminate();
            return !expectedHeaders.Except(actualHeaders).Any();
        }

        public override JObject ToPactMatcher()
        {
            throw new InvalidOperationException("This matcher should not be part of the pact file");
        }

        public override string FailureMessage(object expected, object actual)
        {
            var e = (JObject)expected ?? new JObject();
            var a = (JObject)actual ?? new JObject();

            var expectedHeaders = e.Properties().Select(p => p.Name).ToArray();
            var actualHeaders = a.Properties().Select(p => p.Name).ToArray();

            var missingHeaders = expectedHeaders.Except(actualHeaders).ToArray();

            return "Missing expected headers: " + string.Join(", ", missingHeaders);
        }
    }
}