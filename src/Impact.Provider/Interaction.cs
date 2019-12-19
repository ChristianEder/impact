using System;
using System.Linq;
using Impact.Core;
using Impact.Core.Matchers;
using Newtonsoft.Json.Linq;

namespace Impact.Provider
{
    public class Interaction
    {
        private readonly ITransportMatchers transportMatchers;
        private readonly string description;
        private string providerState;
        private readonly object request;
        private readonly object response;
        private readonly IMatcher[] matchers;

        internal Interaction(JObject i, IRequestResponseDeserializer deserializer, ITransportMatchers transportMatchers)
        {
            this.transportMatchers = transportMatchers;
            description = i["description"].ToString();
            providerState = i["providerState"].ToString();
            request = deserializer.DeserializeRequest(i["request"]);
            response = deserializer.DeserializeResponse(i["response"]);

            matchers = i["matchingRules"] is JObject rules ? MatcherParser.Parse(rules): new IMatcher[0];
            matchers = transportMatchers.ResponseMatchers.Concat(matchers).ToArray();
        }

        public InteractionVerificationResult Honour(Func<object, object> sendRequest)
        {
            var actualResponse = sendRequest(request);
            var context = new MatchingContext(matchers, false);
            var checker = new MatchChecker();

            checker.Matches(response, actualResponse, context);

            return new InteractionVerificationResult(description, context.Result.Matches, context.Result.FailureReasons);

        }
    }

    public class InteractionVerificationResult
    {
        public InteractionVerificationResult(string description, bool success, string failureReason = null)
        {
            Description = description;
            Success = success;
            FailureReason = failureReason;
        }

        public string Description { get; }
        public bool Success { get; }
        public string FailureReason { get; }
    }
}