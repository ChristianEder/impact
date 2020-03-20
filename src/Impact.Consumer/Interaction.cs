using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Impact.Consumer.Transport;
using Impact.Core;
using Impact.Core.Matchers;
using Newtonsoft.Json.Linq;

namespace Impact.Consumer
{
    public class Interaction
    {
        private readonly string interactionDescription;
        private readonly string providerState;
        private readonly Pact pact;
        private object request;
        private Func<object, object> responseFactory;
        private readonly List<IMatcher> responseMatchers = new List<IMatcher>();
        private readonly List<IMatcher> requestMatchers = new List<IMatcher>();
        public int CallCount { get; private set; }

        public Interaction(string interactionDescription, string providerState, Pact pact)
        {
            this.interactionDescription = interactionDescription;
            this.providerState = providerState;
            this.pact = pact;
        }

        public SpecifyRequestMachersOrResponse<T> With<T>(T request)
        {
            this.request = request;
            return new SpecifyRequestMachersOrResponse<T>(this);
        }

        public Type RequestType => request.GetType();

        internal bool Matches(object request, ITransportMatchers transportMatchers)
        {
            var context = new MatchingContext(transportMatchers.RequestMatchers.Concat(requestMatchers).ToArray(), true);
            var checker = new MatchChecker();
            checker.Matches(this.request, request, context);
            return context.Result.Matches;
        }

        public object Respond(object request, ITransportMatchers transportMatchers)
        {
            var response = responseFactory(request);

            var expectedResponse = responseFactory(this.request);

            var context = new MatchingContext(transportMatchers.ResponseMatchers.Concat(responseMatchers).ToArray(), false);
            var checker = new MatchChecker();
            checker.Matches(expectedResponse, response, context);
            if (!context.Result.Matches)
            {
                throw new Exception("Invalid interaction setup - the generated response does not match the expected format: " + context.Result.FailureReasons);
            }

            CallCount++;

            return response;
        }

        internal JObject ToPactInteraction(ITransportFormat format)
        {
            var pactInteraction = new JObject
            {
                ["description"] = interactionDescription
            };

            if (!string.IsNullOrEmpty(providerState))
            {
                pactInteraction["providerState"] = providerState;
            }

            pactInteraction["request"] = format.SerializeRequest(request);
            pactInteraction["response"] = format.SerializeResponse(responseFactory(request));

            if (responseMatchers.Any())
            {
                var rules = new JObject();
                foreach (var responseMatcher in responseMatchers.GroupBy(m => m.PropertyPath))
                {
                    rules["$." + responseMatcher.Key] = new JArray(responseMatcher.Select(r => r.ToPactMatcher()).ToArray());
                }
                pactInteraction["matchingRules"] = rules;
            }

            return pactInteraction;
        }

        public class SpecifyRequestMachersOrResponse<TRequest>
        {
            private readonly Interaction interaction;

            public SpecifyRequestMachersOrResponse(Interaction interaction)
            {
                this.interaction = interaction;
            }

            public SpecifyRequestMachersOrResponse<TRequest> WithRequestMatchingRule<TProperty>(Expression<Func<TRequest, TProperty>> property, Action<PropertyMatcher<TProperty>> rule)
            {
                var matcher = new PropertyMatcher<TProperty>(ExpressionToPropertyPath.Convert(property), interaction.requestMatchers.Add);
                rule(matcher);
                return this;
            }

            public SpecifyRequestMachersOrResponse<TRequest> WithRequestArrayMatchingRule<TProperty>(Expression<Func<TRequest, IEnumerable<TProperty>>> property, Action<ArrayMatcher<TProperty>> rules)
            {
                var matcher = new ArrayMatcher<TProperty>(ExpressionToPropertyPath.Convert(property), interaction.requestMatchers.Add);
                rules(matcher);
                return this;
            }

            public SpecifyResponseMachers<TResponse> WillRespondWith<TResponse>(Func<TRequest, TResponse> responseFactory)
            {
                interaction.responseFactory = o => responseFactory((TRequest)o);
                interaction.pact.Register(interaction);
                return new SpecifyResponseMachers<TResponse>(interaction);
            }
        }

        public class SpecifyResponseMachers<TResponse>
        {
            private readonly Interaction interaction;

            public SpecifyResponseMachers(Interaction interaction)
            {
                this.interaction = interaction;
            }

            public SpecifyResponseMachers<TResponse> WithResponseMatchingRule<TProperty>(Expression<Func<TResponse, TProperty>> property, Action<PropertyMatcher<TProperty>> rule)
            {
                var matcher = new PropertyMatcher<TProperty>(ExpressionToPropertyPath.Convert(property), interaction.responseMatchers.Add);
                rule(matcher);
                return this;
            }

            public SpecifyResponseMachers<TResponse> WithResponseArrayMatchingRule<TProperty>(Expression<Func<TResponse, IEnumerable<TProperty>>> property, Action<ArrayMatcher<TProperty>> rules)
            {
                var matcher = new ArrayMatcher<TProperty>(ExpressionToPropertyPath.Convert(property), interaction.responseMatchers.Add);
                rules(matcher);
                return this;
            }
        }
    }
}