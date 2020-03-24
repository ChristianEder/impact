using Impact.Core;

namespace Impact.Consumer.Transport.Callbacks
{
    public class CallbacksMockServer
    {
        private readonly Pact pact;
        private readonly ITransportMatchers matchers;

        public CallbacksMockServer(Pact pact, ITransportMatchers matchers = null)
        {
            this.pact = pact;
            this.matchers = matchers ?? new NoTransportMatchers();
        }

        public TResponse SendRequest<TRequest, TResponse>(TRequest request)
        {
            var interaction = pact.GetMatchingInteraction(request, matchers);
            return (TResponse)interaction.Respond(request, matchers);
        }
    }
}