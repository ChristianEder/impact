using System;
using System.Collections.Generic;
using Impact.Core.Matchers;

namespace Impact.Core
{
    public class MatchCheckResult
    {
        public bool Matches { get; private set; } = true;

        public string FailureReasons { get; private set; } = string.Empty;

        public void AddFailure(IEnumerable<IPropertyPathPart> propertyPath, string reason)
        {
            Matches = false;
            reason = $"$.{string.Join(".", propertyPath)}: {reason}";
            FailureReasons = string.IsNullOrEmpty(FailureReasons)
                ? reason
                : (FailureReasons + Environment.NewLine + reason);
        }
    }
}