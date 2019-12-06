using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Impact.Consumer
{
    public class MockServer
    {
        private readonly List<Interaction> interactions = new List<Interaction>();

        public ProviderState Given(string providerState)
        {
            return new ProviderState(providerState, this);
        }

        internal void Register(Interaction interaction)
        {
            interactions.Add(interaction);
        }

        public TResponse SendRequest<TRequest, TResponse>(TRequest request)
        {
            var matchingInteractions = interactions.Where(i => i.Matches(request)).ToArray();
            if (matchingInteractions.Length == 0)
            {
                throw new Exception("No matching interaction was found");
            }

            if (matchingInteractions.Length > 1)
            {
                throw new Exception("More than 1 matching interactions where found");
            }

            return (TResponse) matchingInteractions.Single().Respond(request);
        }

        public void VerifyAllInteractionsWhereCalled()
        {
            if (interactions.Any(i => i.CallCount < 1))
            {
                throw new Exception("Some interactions have not been called");
            }
        }

        public string ToPactFile(string consumer, string provider, IRequestResponseSerializer serializer)
        {
            return new JObject
            {
                ["consumer"] = new JObject { ["name"] = consumer },
                ["provider"] = new JObject { ["name"] = provider },
                ["interactions"] = new JArray(interactions.Select(i => i.ToPactInteraction(serializer)).ToArray())
            }.ToString();
        }
    }
}