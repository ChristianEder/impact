namespace Impact.Consumer
{
    public class ProviderState
    {
        private string providerState;
        private readonly MockServer mockServer;

        public ProviderState(string providerState, MockServer mockServer)
        {
            this.providerState = providerState;
            this.mockServer = mockServer;
        }

        public ProviderState Given(string providerState)
        {
            this.providerState += "&&" + providerState;
            return this;
        }

        public Interaction UponReceiving(string interactionDescription)
        {
            return new Interaction(interactionDescription, providerState, mockServer);
        }
    }
}