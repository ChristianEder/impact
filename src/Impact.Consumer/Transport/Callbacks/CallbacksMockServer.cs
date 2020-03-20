using Impact.Core;

namespace Impact.Consumer.Transport.Callbacks
{
    public class CallbacksMockServer
    {
        private readonly Pact pact;
        private readonly ITransportMatchers matchers = new NoTransportMatchers();

        public CallbacksMockServer(Pact pact)
        {
            this.pact = pact;
        }

        public TResponse SendRequest<TRequest, TResponse>(TRequest request)
        {
            var interaction = pact.GetMatchingInteraction(request, matchers);
            return (TResponse)interaction.Respond(request, matchers);
        }
    }
}