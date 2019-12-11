using Impact.Consumer.Define;

namespace Impact.Consumer.Serve
{
    public class MockServer
    {
        private readonly Pact pact;

        public MockServer(Pact pact)
        {
            this.pact = pact;
        }

        public TResponse SendRequest<TRequest, TResponse>(TRequest request)
        {
            return pact.SendRequest<TRequest, TResponse>(request);
        }
    }
}