using Impact.Consumer.Serve.Http.Matchers;
using Impact.Core;
using Impact.Core.Matchers;

namespace Impact.Consumer.Serve.Http
{
    public class HttpTransportMatchers : ITransportMatchers
    {
        public IMatcher[] RequestMatchers { get; } = { new RequestHeadersDoNotFailPostelsLaw()};
        public IMatcher[] ResponseMatchers { get; } = new IMatcher[0];
    }
}