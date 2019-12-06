using System;

namespace Impact.Core
{
    public class MatchCheckResult
    {
        public bool Matches { get; private set; } = true;

        public string FailureReasons { get; private set; } = String.Empty;

        public void AddFailure(string propertyPath, string reason)
        {
            Matches = false;
            reason = $"$.{propertyPath}: {reason}";
            FailureReasons = string.IsNullOrEmpty(FailureReasons)
                ? reason
                : (FailureReasons + Environment.NewLine + reason);
        }
    }
}