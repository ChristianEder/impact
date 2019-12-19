using System;
using System.Collections.Generic;
using System.Linq;
using Impact.Core;
using Impact.Core.Matchers;
using Newtonsoft.Json.Linq;

namespace Impact.Consumer.Serve.Http.Matchers
{
    public class V2CompliantResponseBodyArrays : Matcher, IArrayLengthMatcher
    {
        public V2CompliantResponseBodyArrays() : base(nameof(HttpResponse.Body))
        {
        }

        public override bool AppliesTo(List<IPropertyPathPart> propertyPath)
        {
            return base.AppliesTo(propertyPath.Take(1).ToList());
        }

        public override bool AppliesTo(object expected, object actual, MatchingContext context)
        {
            if (!base.AppliesTo(expected, actual, context))
            {
                return false;
            }

            return !context.IgnoreExpected && expected is JToken[] && actual is JToken[];
        }

        public override bool Matches(object expected, object actual, MatchingContext context, Action<object, object, MatchingContext> deepMatch)
        {
            if (HasCatchAllItemsMatcherForAnythingBelow(context))
            {
                return true;
            }

            return ((JToken[])expected).Length >= ((JToken[])actual).Length;
        }

        private bool HasCatchAllItemsMatcherForAnythingBelow(MatchingContext context)
        {
            var catchAllItems = new List<IPropertyPathPart>(context.PropertyPath) { new ArrayIndexPathPart("*", exact: true) };

            return context.Matchers.Any(m => StartsWith(m.PropertyPathParts, catchAllItems));
        }

        private bool StartsWith(IPropertyPathPart[] p1, List<IPropertyPathPart> p2)
        {
            if (p1.Length < p2.Count)
            {
                return false;
            }

            for (int i = 0; i < p2.Count; i++)
            {
                if (!p2[i].Equals(p1[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override JObject ToPactMatcher()
        {
            throw new InvalidOperationException("This matcher should not be part of the pact file");
        }

        public override string FailureMessage(object expected, object actual)
        {
            return @"Expected {((JToken[])expected).Length} items, but got {((JToken[])actual).Length} items";
        }

        public override bool IsTerminal => false;
    }
}