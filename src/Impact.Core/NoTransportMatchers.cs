using Impact.Core.Matchers;

namespace Impact.Core
{
    public class NoTransportMatchers : ITransportMatchers
    {
        public IMatcher[] RequestMatchers { get; } = new IMatcher[0];
        public IMatcher[] ResponseMatchers { get; } = new IMatcher[0];
    }
}