using System;
using System.Linq;
using Impact.Core.Matchers;
using Newtonsoft.Json.Linq;

namespace Impact.Core
{
    public class MatchChecker2
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

            if (expected == null && actual == null)
            {
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
                        //  failure & terminate # !!! ausnahme: http headers: set e to empty(a), go down with matchersOnly
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

                    expected = CreateEmpty(actual);
                    context.IgnoreExpected = true;
                }
            }
            else
            {
                expected = expected ?? CreateEmpty(actual);
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

            if (context.MatchersForProperty.Any())
            {
                AddFailures(expected.Value, actual.Value, context.MatchersForProperty, context);
                return;
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

            var lengthMatchers = context.MatchersForProperty.Where(m => m is TypeMaxPropertyMatcher || m is TypeMinPropertyMatcher).ToArray();

            if (lengthMatchers.Any())
            {
                AddFailures(expectedItems, actualItems, lengthMatchers, context);
            }
            else
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

        private static JToken CreateEmpty(JToken other)
        {
            switch (other)
            {
                case JValue _:
                    return JValue.CreateNull();
                case JObject _:
                    return new JObject();
                case JArray _:
                    return new JArray();
            }

            return null;
        }

        private static void AddFailures(object expected, object actual, IMatcher[] matchers, MatchingContext context)
        {
            if (IsNullish(expected) || IsNullish(actual))
            {
                return;
            }

            var failingMatchers = matchers.Where(m => !m.Matches(expected, actual, context, (e, a, c) => AddFailures(JToken.FromObject(e), JToken.FromObject(a), c))).ToArray();
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