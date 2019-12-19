using System;
using System.Collections.Generic;
using System.Linq;
using Impact.Core;
using Newtonsoft.Json.Linq;

namespace Impact.Provider
{
    public class Pact
    {
        private string provider;
        private string consumer;
        private Interaction[] interactions;

        public Pact(string pactJson, IRequestResponseDeserializer deserializer, ITransportMatchers transportMatchers)
        {
            var pact = JObject.Parse(pactJson);
            provider = pact["provider"].ToString();
            consumer = pact["consumer"].ToString();

            interactions = pact["interactions"].Cast<JObject>().Select(i => new Interaction(i, deserializer, transportMatchers)).ToArray();
        }

        public VerificationResult Honour(Func<object, object> sendRequest)
        {
            var result = new VerificationResult();

            foreach (var interaction in interactions)
            {
                result.Add(interaction.Honour(sendRequest));
            }

            return result;
        }
    }

    public class VerificationResult
    {
        public List<InteractionVerificationResult> Results { get; } = new List<InteractionVerificationResult>();

        public bool Success => Results.All(r => r.Success);

        public void Add(InteractionVerificationResult interactionVerificationResult)
        {
            Results.Add(interactionVerificationResult);
        }
    }
}
