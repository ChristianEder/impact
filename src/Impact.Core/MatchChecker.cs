using System;
using System.Collections.Generic;
using System.Linq;
using Impact.Core.Matchers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Impact.Core
{
    public class MatchChecker
    {
        private readonly bool isRequest;
        private readonly IMatcher[] matchers;

        public MatchChecker(IEnumerable<IMatcher> matchers, bool isRequest)
        {
            this.isRequest = isRequest;
            this.matchers = matchers.ToArray();
        }

        public MatchCheckResult Matches(object expected, object actual)
        {
            var result = new MatchCheckResult();
            AddFailures(ToJToken(expected), ToJToken(actual), new List<IPropertyPathPart>(), isRequest, result, matchers);
            return result;
        }

        private static void AddFailures(JToken expected, JToken actual, List<IPropertyPathPart> propertyPath, bool isRequest, MatchCheckResult result, IMatcher[] allMatchers, bool onlyMatchers = false)
        {
            if (!onlyMatchers && AddFailuresForNull(expected, actual, propertyPath, result, isRequest))
            {
                return;
            }

            if (!onlyMatchers && expected.GetType() != actual.GetType())
            {
                result.AddFailure(propertyPath, "types do not match");
                return;
            }

            var matchersForProperty = allMatchers.Where(m => m.AppliesTo(propertyPath)).ToArray();

            switch (actual)
            {
                case JValue actualValue:
                    AddFailuresForValue((JValue)expected, actualValue, propertyPath, result, matchersForProperty, onlyMatchers);
                    break;
                case JObject actualObject:
                    AddFailuresForObject((JObject)expected, actualObject, propertyPath, isRequest, result, allMatchers, onlyMatchers);
                    break;
                case JArray actualArray:
                    AddFailuresForArray((JArray)expected, actualArray, propertyPath, isRequest, result, allMatchers, matchersForProperty, onlyMatchers);
                    break;
                default:
                    result.AddFailure(propertyPath, "unknown type found: " + actual.GetType().Name);
                    break;
            }
        }

        private static void AddFailuresForValue(JValue expected, JValue actual, List<IPropertyPathPart> propertyPath,
            MatchCheckResult result, IMatcher[] matchersForProperty, bool onlyMatchers)
        {
            if (actual.Equals(expected))
            {
                return;
            }

            if (matchersForProperty.Any())
            {
                AddFailures(expected.Value, actual.Value, matchersForProperty, result);
            }
            else if (!onlyMatchers)
            {
                result.AddFailure(propertyPath, $"Expected {expected.Value}, but got {actual.Value}");
            }
        }

        private static void AddFailuresForObject(JObject expected, JObject actual, List<IPropertyPathPart> propertyPath,
            bool isRequest, MatchCheckResult result, IMatcher[] allMatchers, bool onlyMatchers)
        {
            var allProperties = expected.Properties().Union(actual.Properties()).Select(p => p.Name).Distinct().ToArray();

            foreach (var property in allProperties)
            {
                var expectedProperty = expected.Property(property);
                var actualProperty = actual.Property(property);

                var childPropertyPath = new List<IPropertyPathPart>(propertyPath) { new PropertyPathPart(property) };

                if (!onlyMatchers)
                {
                    if (expectedProperty != null && actualProperty == null)
                    {
                        result.AddFailure(childPropertyPath, "expected not null value, got null");
                        continue;
                    }

                    if (actualProperty != null && expectedProperty == null && isRequest)
                    {
                        result.AddFailure(propertyPath, "expected null value");
                        continue;
                    }
                }

                AddFailures(expectedProperty.Value, actualProperty.Value, childPropertyPath, isRequest, result, allMatchers, onlyMatchers);
            }
        }

        private static void AddFailuresForArray(JArray expected, JArray actual, List<IPropertyPathPart> propertyPath,
            bool isRequest, MatchCheckResult result, IMatcher[] allMatchers, IMatcher[] matchersForProperty,
            bool onlyMatchers)
        {
            var expectedItems = expected.ToArray();
            var actualItems = actual.ToArray();

            var lengthMatchers = matchersForProperty.Where(m => m is TypeMaxPropertyMatcher || m is TypeMinPropertyMatcher).ToArray();

            if (!lengthMatchers.Any() && !onlyMatchers && (actualItems.Length < expectedItems.Length || actualItems.Length > expectedItems.Length && isRequest))
            {
                // Postel's law: Accept more actuals than expecteds for responses, but fail on requests. Never accept less than expected.
                // The presence of length matchers indicates that the user wants to override this behaviour.
                result.AddFailure(propertyPath, $"Expected {expectedItems.Length} elements, but got {actualItems.Length}");
            }

            if (lengthMatchers.Any())
            {
                AddFailures(expectedItems, actualItems, lengthMatchers, result);
            }

            for (int i = 0; i < Math.Min(actualItems.Length, expectedItems.Length); i++)
            {
                var actualItem = actualItems[i];
                var expectedItem = expectedItems[i];

                var itemPath = new List<IPropertyPathPart>(propertyPath) { new ArrayIndexPathPart(i.ToString()) };

                AddFailures(expectedItem, actualItem, itemPath, isRequest, result, allMatchers, onlyMatchers);
            }

            if (actual.Count > expected.Count)
            {
                for (var i = expected.Count; i < actual.Count; i++)
                {
                    var actualItem = actualItems[i];
                    var itemPath = new List<IPropertyPathPart>(propertyPath) { new ArrayIndexPathPart(i.ToString()) };

                    AddFailures(expectedItems.Last(), actualItem, itemPath, isRequest, result, allMatchers, onlyMatchers: true);
                }
            }
        }

        private static bool AddFailuresForNull(JToken expected, JToken actual, List<IPropertyPathPart> propertyPath, MatchCheckResult result, bool isRequest)
        {
            var nullValue = JValue.CreateNull();

            var expectedIsNull = nullValue.Equals(expected);
            var actualIsNull = nullValue.Equals(actual);

            if (expectedIsNull && actualIsNull)
            {
                // Both are null, this is fine
                return true;
            }

            if (expectedIsNull && isRequest)
            {
                // expected is null, but actual ha a value. According to Postels Law, this is only allowed for responses.
                result.AddFailure(propertyPath, "expected null value");
                return true;
            }

            if (actualIsNull)
            {
                result.AddFailure(propertyPath, "expected not null value, got null");
                return true;
            }

            return false;
        }

        private static void AddFailures(object expected, object actual, IMatcher[] matchers, MatchCheckResult result)
        {
            var failingMatchers = matchers.Where(m => !m.Matches(expected, actual)).ToArray();
            foreach (var failingMatcher in failingMatchers)
            {
                result.AddFailure(failingMatcher.PropertyPathParts, failingMatcher.FailureMessage(expected, actual));
            }
        }

        private JToken ToJToken(object o)
        {
            return JToken.Parse(JsonConvert.SerializeObject(o));
        }
    }
}