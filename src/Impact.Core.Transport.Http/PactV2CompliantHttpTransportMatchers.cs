using System.Collections.Generic;
using Impact.Core;
using Impact.Core.Matchers;
using Impact.Core.Transport.Http.Matchers;

namespace Impact.Core.Transport.Http
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

            ResponseMatchers = new List<IMatcher>(httpMatchers.ResponseMatchers)
            {
                new V2CompliantResponseBodyArrays()
            }.ToArray();
        }
    }
}