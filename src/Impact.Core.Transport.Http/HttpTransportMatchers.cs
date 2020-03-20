using Impact.Core;
using Impact.Core.Matchers;
using Impact.Core.Transport.Http.Matchers;

namespace Impact.Core.Transport.Http
{
    public class HttpTransportMatchers : ITransportMatchers
    {
        public IMatcher[] RequestMatchers { get; } =
        {
            new HeadersDoNotFailPostelsLaw(),
            new HeadersAllowWhitespaceAfterComma(),
            new RequestMethodIgnoresCasing(),
            new RequestQueryOrderingAndEscapingDoNotFail()
        };

        public IMatcher[] ResponseMatchers { get; } =
        {
            new HeadersDoNotFailPostelsLaw(),
            new HeadersAllowWhitespaceAfterComma()
        };
    }
}