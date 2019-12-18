using System.Collections.Generic;
using Impact.Consumer.Serve.Http.Matchers;
using Impact.Core;
using Impact.Core.Matchers;

namespace Impact.Consumer.Serve.Http
{
    public class PactV2CompliantHttpTransportMatchers : ITransportMatchers
    {
        public IMatcher[] RequestMatchers { get; }
        public IMatcher[] ResponseMatchers { get; }

        public PactV2CompliantHttpTransportMatchers()
        {
            var httpMatchers = new HttpTransportMatchers();
            RequestMatchers = new List<IMatcher>(httpMatchers.RequestMatchers)
            {
                new V2CompliantRequestBody()
            }.ToArray();

            ResponseMatchers = httpMatchers.ResponseMatchers;
        }
    }
}