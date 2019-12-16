using System;
using System.Linq;
using Impact.Core.Matchers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Impact.Core
{
    public class MatchChecker
    {
        public void Matches(object expected, object actual, MatchingContext context)
        {
            AddFailures(JToken.FromObject(expected), JToken.FromObject(actual), context);
        }

        private static void AddFailures(JToken expected, JToken actual, MatchingContext context)
        {
            if (context.TerminationRequested)
            {
                return;
            }

            if (!context.IgnoreExpected && AddFailuresForNull(expected, actual, context))
            {
                return;
            }

            if (!context.IgnoreExpected && expected.GetType() != actual.GetType())
            {
                context.Result.AddFailure(context.PropertyPath, "types do not match");
                return;
            }

            switch (actual)
            {
                case JValue actualValue:
                    AddFailuresForValue((JValue)expected, actualValue, context);
                    break;
                case JObject actualObject:
                    AddFailuresForObject((JObject)expected, actualObject, context);
                    break;
                case JArray actualArray:
                    AddFailuresForArray((JArray)expected, actualArray, context);
                    break;
                default:
                    context.Result.AddFailure(context.PropertyPath, "unknown type found: " + actual.GetType().Name);
                    break;
            }
        }

        private static void AddFailuresForValue(JValue expected, JValue actual, MatchingContext context)
        {
            if (actual.Equals(expected))
            {
                return;
            }

            if (context.MatchersForProperty.Any())
            {
                AddFailures(expected.Value, actual.Value, context.MatchersForProperty, context);
            }
            else if (!context.IgnoreExpected)
            {
                context.Result.AddFailure(context.PropertyPath, $"Expected {expected.Value}, but got {actual.Value}");
            }
        }

        private static void AddFailuresForObject(JObject expected, JObject actual, MatchingContext context)
        {
            if (context.MatchersForProperty.Any())
            {
                AddFailures(expected, actual, context.MatchersForProperty, context);
            }

            var allProperties = expected.Properties().Union(actual.Properties()).Select(p => p.Name).Distinct().ToArray();

            foreach (var property in allProperties)
            {
                var childContext = context.For(new PropertyPathPart(property));

                var expectedProperty = expected.Property(property);
                var actualProperty = actual.Property(property);

                if (!context.IgnoreExpected)
                {
                    if (childContext.MatchersForProperty.Any())
                    {
                        AddFailures(expectedProperty?.Value, actualProperty?.Value, childContext.MatchersForProperty, childContext);
                    }
                    else
                    {
                        var nullValue = JValue.CreateNull();

                        if (actualProperty == null && expectedProperty != null && !expectedProperty.Value.Equals(nullValue))
                        {
                            childContext.Result.AddFailure(childContext.PropertyPath, "expected not null value, got null");
                            continue;
                        }

                        if (expectedProperty == null && actualProperty != null && !actualProperty.Value.Equals(nullValue) && childContext.IsRequest)
                        {
                            childContext.Result.AddFailure(childContext.PropertyPath, "expected null value");
                            continue;
                        }
                    }
                }

                var expectedValue = expectedProperty?.Value;
                var actualValue = actualProperty?.Value;

                if (expectedValue == null && actualValue == null)
                {
                    continue;
                }

                if (expectedValue == null)
                {
                    expectedValue = CreateEmpty(actualValue);
                }
                else if (actualValue == null)
                {
                    actualValue = CreateEmpty(expectedValue);
                }

                AddFailures(expectedValue, actualValue, childContext);
            }
        }

        private static JToken CreateEmpty(JToken other)
        {
            switch (other)
            {
                case JValue v:
                    return JValue.CreateNull();
                case JObject o:
                    return new JObject();
                case JArray a:
                    return new JArray();
            }

            return null;
        }

        private static void AddFailuresForArray(JArray expected, JArray actual, MatchingContext context)
        {
            var expectedItems = expected.ToArray();
            var actualItems = actual.ToArray();

            var lengthMatchers = context.MatchersForProperty.Where(m => m is TypeMaxPropertyMatcher || m is TypeMinPropertyMatcher).ToArray();

            if (!lengthMatchers.Any() && !context.IgnoreExpected && (actualItems.Length < expectedItems.Length || actualItems.Length > expectedItems.Length && context.IsRequest))
            {
                // Postel's law: Accept more actuals than expecteds for responses, but fail on requests. Never accept less than expected.
                // The presence of length matchers indicates that the user wants to override this behaviour.
                context.Result.AddFailure(context.PropertyPath, $"Expected {expectedItems.Length} elements, but got {actualItems.Length}");
            }

            if (lengthMatchers.Any())
            {
                AddFailures(expectedItems, actualItems, lengthMatchers, context);
            }

            for (int i = 0; i < Math.Min(actualItems.Length, expectedItems.Length); i++)
            {
                var actualItem = actualItems[i];
                var expectedItem = expectedItems[i];

                AddFailures(expectedItem, actualItem, context.For(new ArrayIndexPathPart(i.ToString())));
            }

            if (actual.Count > expected.Count)
            {
                for (var i = expected.Count; i < actual.Count; i++)
                {
                    var actualItem = actualItems[i];

                    AddFailures(expectedItems.Last(), actualItem, context.For(new ArrayIndexPathPart(i.ToString()), ignoreExpected: true));
                }
            }
        }

        private static bool AddFailuresForNull(JToken expected, JToken actual, MatchingContext context)
        {
            var nullValue = JValue.CreateNull();

            var expectedIsNull = nullValue.Equals(expected);
            var actualIsNull = nullValue.Equals(actual);

            if (expectedIsNull && actualIsNull)
            {
                // Both are null, this is fine
                return true;
            }

            if (expectedIsNull && context.IsRequest)
            {
                // expected is null, but actual ha a value. According to Postels Law, this is only allowed for responses.
                context.Result.AddFailure(context.PropertyPath, "expected null value");
                return true;
            }

            if (actualIsNull)
            {
                context.Result.AddFailure(context.PropertyPath, "expected not null value, got null");
                return true;
            }

            return false;
        }

        private static void AddFailures(object expected, object actual, IMatcher[] matchers, MatchingContext context)
        {
            var failingMatchers = matchers.Where(m => !m.Matches(expected, actual, context, (e, a, c) => AddFailures(JToken.FromObject(e), JToken.FromObject(a), c))).ToArray();
            foreach (var failingMatcher in failingMatchers)
            {
                context.Result.AddFailure(failingMatcher.PropertyPathParts, failingMatcher.FailureMessage(expected, actual));
            }
        }
    }
}