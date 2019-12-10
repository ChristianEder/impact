namespace Impact.Consumer.Define
{
    public class ProviderState
    {
        private string providerState;
        private readonly Pact pact;

        public ProviderState(string providerState, Pact pact)
        {
            this.providerState = providerState;
            this.pact = pact;
        }

        public ProviderState And(string providerState)
        {
            this.providerState += "&&" + providerState;
            return this;
        }

        public Interaction UponReceiving(string interactionDescription)
        {
            return new Interaction(interactionDescription, providerState, pact);
        }
    }
}