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

            if (expected.IsNullish() && actual.IsNullish())
            {
                if (context.IsRequest && actual != null && expected == null)
                {
                    context.Result.AddFailure(context.PropertyPath, "Expected no value, but got one");
                }
                return;
            }

            if (actual == null)
            {
                context.Result.AddFailure(context.PropertyPath, "Expected a value, but got none");
                return;
            }

            if (!context.IgnoreExpected)
            {
                if (expected == null)
                {
                    if (context.IsRequest)
                    {
                        if (context.MatchersForProperty.Any())
                        {
                            AddFailures(null, actual, context.MatchersForProperty, context);
                        }
                        else
                        {
                            context.Result.AddFailure(context.PropertyPath, "Expected no value, but got one");
                        }

                        return;
                    }

                    expected = actual.CreateEmpty();
                    context.IgnoreExpected = true;
                }
            }
            else
            {
                expected = expected ?? actual.CreateEmpty();
            }

            if (expected.GetType() != actual.GetType())
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

            var nullValue = JValue.CreateNull();

            if (expected.Equals(nullValue) && !actual.Equals(nullValue) && !context.IgnoreExpected)
            {
                context.Result.AddFailure(context.PropertyPath, "Expected no value, but got " + actual.Value);
                return;
            }

            if (actual.Equals(nullValue) && !expected.Equals(nullValue))
            {
                context.Result.AddFailure(context.PropertyPath, "Expected a value, but got none");
                return;
            }

            var applyingMatchers = context.MatchersForProperty.Where(m => m.AppliesTo(expected, actual, context))
                .ToArray();

            if (applyingMatchers.Any())
            {
                var expectedValue = expected.Value;
                var actualValue = actual.Value;

                if (expected.Type == JTokenType.Date)
                {
                    expectedValue = JsonConvert.SerializeObject(expectedValue);
                }
                if (actual.Type == JTokenType.Date)
                {
                    actualValue = JsonConvert.SerializeObject(actualValue);
                }


                AddFailures(expectedValue, actualValue, applyingMatchers, context);

                if (applyingMatchers.Any(m => m.IsTerminal))
                {
                    return;
                }
            }

            if (!context.IgnoreExpected)
            {
                context.Result.AddFailure(context.PropertyPath, $"Expected {expected.Value}, but got {actual.Value}");
            }
        }

        private static void AddFailuresForObject(JObject expected, JObject actual, MatchingContext context)
        {
            foreach (var property in expected.Properties().Concat(actual.Properties()).Select(p => p.Name.ToLowerInvariant()).Distinct().ToArray())
            {
                var expectedProperty = expected.Properties().FirstOrDefault(p => p.Name.Equals(property, StringComparison.InvariantCultureIgnoreCase));
                var actualProperty = actual.Properties().FirstOrDefault(p => p.Name.Equals(property, StringComparison.InvariantCultureIgnoreCase));

                var propertyName = (expectedProperty ?? actualProperty)?.Name ?? property;
                AddFailures(expectedProperty?.Value, actualProperty?.Value, context.For(new PropertyPathPart(propertyName)));
            }
        }

        private static void AddFailuresForArray(JArray expected, JArray actual, MatchingContext context)
        {
            var expectedItems = expected.ToArray();
            var actualItems = actual.ToArray();

            var lengthMatchers = context.MatchersForProperty.OfType<IArrayLengthMatcher>().Where(m => m.AppliesTo(expectedItems, actualItems, context)).ToArray();

            if (lengthMatchers.Any())
            {
                AddFailures(expectedItems, actualItems, lengthMatchers, context);
            }

            if(!lengthMatchers.Any(m => m.IsTerminal))
            {
                if (!context.IgnoreExpected)
                {
                    if (context.IsRequest)
                    {
                        if (actualItems.Length != expectedItems.Length)
                        {
                            context.Result.AddFailure(context.PropertyPath,
                                $"Expected {expectedItems.Length} items, but got {actualItems.Length} items");
                        }
                    }
                    else
                    {
                        if (actualItems.Length < expectedItems.Length)
                        {
                            context.Result.AddFailure(context.PropertyPath,
                                $"Expected at least {expectedItems.Length} items, but got only {actualItems.Length} items");
                        }
                    }
                }
            }

            for (var i = 0; i < actualItems.Length; i++)
            {
                AddFailures(i < expectedItems.Length ? expectedItems[i] : expectedItems.LastOrDefault(), actualItems[i], 
                    context.For(new ArrayIndexPathPart(i.ToString()), ignoreExpected: i >= expectedItems.Length));
            }
        }

        private static void AddFailures(object expected, object actual, IMatcher[] matchers, MatchingContext context)
        {
            if (IsNullish(expected) || IsNullish(actual))
            {
                return;
            }

            var matchersThatApply = matchers.Where(m => m.AppliesTo(expected, actual, context));

            var failingMatchers = matchersThatApply.Where(m => !m.Matches(expected, actual, context, (e, a, c) => AddFailures(JToken.FromObject(e), JToken.FromObject(a), c))).ToArray();
            foreach (var failingMatcher in failingMatchers)
            {
                context.Result.AddFailure(failingMatcher.PropertyPathParts, failingMatcher.FailureMessage(expected, actual));
            }
        }

        private static bool IsNullish(object value)
        {
            return ReferenceEquals(value, null) || (value is JValue v && v.Equals(JValue.CreateNull()));
        }
    }
}