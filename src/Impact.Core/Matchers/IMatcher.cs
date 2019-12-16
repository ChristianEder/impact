using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Impact.Core.Matchers
{
    public interface IMatcher
    {
        bool Matches(object expected, object actual, MatchingContext context, Action<object, object, MatchingContext> deepMatch);

        string PropertyPath { get; }

        IPropertyPathPart[] PropertyPathParts { get; }

        JObject ToPactMatcher();

        string FailureMessage(object expected, object actual);

        bool AppliesTo(List<IPropertyPathPart> propertyPath);
    }
}