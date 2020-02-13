using System.Collections.Generic;
using System.Linq;
using Impact.Core.Matchers;

namespace Impact.Core
{
    public class MatchingContext
    {
        public MatchingContext(IMatcher[] matchers, bool isRequest)
            : this(matchers, isRequest, new List<IPropertyPathPart>(), new MatchCheckResult())
        {
        }

        private MatchingContext(IMatcher[] matchers, bool isRequest,
            List<IPropertyPathPart> propertyPath, MatchCheckResult result)
        {
            PropertyPath = propertyPath;
            Matchers = matchers;
            MatchersForProperty = matchers.Where(m => m.AppliesTo(PropertyPath)).ToArray();
            IsRequest = isRequest;
            IgnoreExpected = false;
            Result = result;
        }

        public List<IPropertyPathPart> PropertyPath { get; }
        public IMatcher[] Matchers { get; }
        public IMatcher[] MatchersForProperty { get; }
        public bool IsRequest { get; }
        public bool IgnoreExpected { get; set; }
        public MatchCheckResult Result { get; }
        public bool TerminationRequested { get; private set; }

        public void Terminate()
        {
            TerminationRequested = true;
        }

        public MatchingContext For(IPropertyPathPart property, bool? ignoreExpected = null, params IMatcher[] additionalMatchers)
        {
            var matchers = Matchers;
            if(additionalMatchers != null && additionalMatchers.Any())
            {
                matchers = Matchers.Concat(additionalMatchers).ToArray();
            }

            return new MatchingContext(matchers, IsRequest, new List<IPropertyPathPart>(PropertyPath) { property }, Result)
            {
                IgnoreExpected = ignoreExpected.GetValueOrDefault(IgnoreExpected),
                TerminationRequested = TerminationRequested
            };
        }
    }
}