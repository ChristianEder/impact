using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Impact.Core.Matchers
{
    public class TypePropertyMatcher : Matcher
    {
        private readonly bool applyToAll;

        public TypePropertyMatcher(string path) : base(path)
        {
        }

        private TypePropertyMatcher(string path, bool applyToAll) : base(path)
        {
            this.applyToAll = applyToAll;
        }

        public override bool AppliesTo(object expected, object actual, MatchingContext context)
        {
            return applyToAll || base.AppliesTo(expected, actual, context);
        }

        public override bool AppliesTo(List<IPropertyPathPart> propertyPath)
        {
            return applyToAll || base.AppliesTo(propertyPath);
        }

        public override bool Matches(object expected, object actual, MatchingContext context, Action<object, object, MatchingContext> deepMatch)
        {
            if (expected is JObject expectedObject && actual is JObject actualObject)
            {
                foreach (var property in expectedObject.Properties().Concat(actualObject.Properties()).Select(p => p.Name).Distinct().ToArray())
                {
                    var expectedProperty = expectedObject.Properties().FirstOrDefault(p => p.Name.Equals(property));
                    var actualProperty = actualObject.Properties().FirstOrDefault(p => p.Name.Equals(property));

                    var propertyName = (expectedProperty ?? actualProperty)?.Name ?? property;
                    deepMatch(expectedProperty?.Value, actualProperty?.Value, context.For(new PropertyPathPart(propertyName), null, new TypePropertyMatcher(PropertyPath + "." + propertyName, applyToAll)));
                }
                context.Terminate();
                return context.Result.Matches;

            }
            return expected.GetType() == actual.GetType();
        }

        public override JObject ToPactMatcher()
        {
            return new JObject
            {
                ["match"] = "type"
            };
        }

        public override string FailureMessage(object expected, object actual)
        {
            if (expected.GetType() != actual.GetType())
            {
                return $"Expected type {expected.GetType().Name}, but got {actual.GetType().Name}";
            }

            if (expected is JObject expectedObject && actual is JObject actualObject)
            {
                return "Expected and actual object structures do not match";
            }

            return string.Empty;
        }
    }
}