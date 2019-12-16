using Impact.Core.Matchers;

namespace Impact.Core
{
    public interface ITransportMatchers
    {
        IMatcher[] RequestMatchers { get; }
        IMatcher[] ResponseMatchers { get; }
    }
}