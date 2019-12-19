﻿using Impact.Consumer.Serve.Http.Matchers;
using Impact.Core;
using Impact.Core.Matchers;

namespace Impact.Consumer.Serve.Http
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