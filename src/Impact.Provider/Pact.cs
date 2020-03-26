using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Impact.Provider.Transport;
using Newtonsoft.Json.Linq;

namespace Impact.Provider
{
    public class Pact
    {
        private string provider;
        private string consumer;
        private Interaction[] interactions;

        public Pact(string pactJson, ITransport transport, Func<string, Task> ensureProviderState)
        {
            var pact = JObject.Parse(pactJson);
            provider = pact["provider"].ToString();
            consumer = pact["consumer"].ToString();

            interactions = pact["interactions"].Cast<JObject>().Select(i => new Interaction(i, transport, ensureProviderState)).ToArray();
        }

        public async Task<VerificationResult> Honour()
        {
            var result = new VerificationResult();

            foreach (var interaction in interactions)
            {
                result.Add(await interaction.Honour());
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
