using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Impact.Core.Matchers;

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
            Matches(expected, actual, "", result, matchers);
            return result;
        }

        private void Matches(object expected, object actual, string propertyPath, MatchCheckResult result, IMatcher[] matchers)
        {
            var matchersForProperty = matchers.Where(m => m.PropertyPath == propertyPath).ToArray();
           
            if (ReferenceEquals(null, expected) && ReferenceEquals(null, actual))
            {
                return;
            }

            if (ReferenceEquals(null, expected))
            {
                result.AddFailure(propertyPath, "expected null value");
                return;
            }

            if (ReferenceEquals(null, actual))
            {
                result.AddFailure(propertyPath, "expected not null value, got null");
                return;
            }

            if (IsSimpleType(actual))
            {
                if (matchersForProperty.Any())
                {
                    AddFailures(expected, actual, matchersForProperty, result);
                }
                else
                {
                    if (!Equals(expected, actual))
                    {
                        var expectedIsEmpty = expected is string s && string.IsNullOrEmpty(s);
                        var postelsLawAllowsAdditionalProperties = !isRequest && expectedIsEmpty;
                        if (!postelsLawAllowsAdditionalProperties)
                        {
                            result.AddFailure(propertyPath, $"Expected {expected}, but got {actual}");
                        }
                    }
                }
                return;
            }

            AddFailures(expected, actual, matchersForProperty, result);

           
            if (actual is IEnumerable enumerable)
            {
                MatchesCollection(expected, propertyPath, result, matchers, matchersForProperty, enumerable);
                return;
            }

            var propertyInfos = actual.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var propertyInfo in propertyInfos)
            {
                Matches(propertyInfo.GetValue(expected), propertyInfo.GetValue(actual), propertyPath.Length > 0 ? (propertyPath + "." + propertyInfo.Name) : propertyInfo.Name, result, matchers);
            }
        }

        private void MatchesCollection(object expected, string propertyPath, MatchCheckResult result, IMatcher[] matchers, IMatcher[] matchersForProperty, IEnumerable enumerable)
        {
            var actualItems = enumerable.Cast<object>().ToArray();
            var expectedItems = (expected as IEnumerable).Cast<object>().ToArray();

            var hasLengthMatchers = matchersForProperty.Any(m => m is TypeMaxPropertyMatcher || m is TypeMinPropertyMatcher);

            if (!hasLengthMatchers && actualItems.Length != expectedItems.Length)
            {
                if (isRequest)
                {
                    // Postel's law: Accept more actuals than expecteds for responses, but fail on requests
                    result.AddFailure(propertyPath, $"Expected {expectedItems.Length} elements, but got {actualItems.Length}");
                }
            }

            for (int i = 0; i < Math.Min(actualItems.Length, expectedItems.Length); i++)
            {
                var actualItem = actualItems[i];
                var expectedItem = expectedItems[i];
                var arrayPropertyPath = propertyPath + "[*]";
                var arrayIndexPropertyPath = $"{propertyPath}[{i}]";

                var matchersToExpand = matchers.Where(m => m.PropertyPath.StartsWith(arrayPropertyPath)).ToArray();
                var expandedMatchers = matchers.Except(matchersToExpand)
                    .Concat(matchersToExpand.Select(m => m.Clone(m.PropertyPath.Replace(arrayPropertyPath, arrayIndexPropertyPath))))
                    .ToArray();

                Matches(expectedItem, actualItem, arrayIndexPropertyPath, result, expandedMatchers);
            }
        }

        private static bool IsSimpleType(object actual)
        {
            return actual is string ||
            actual is bool ||
            actual is double ||
            actual is int ||
            actual is long;
        }

        private static void AddFailures(object expected, object actual, IMatcher[] matchers, MatchCheckResult result)
        {
            var failingMatchers = matchers.Where(m => !m.Matches(expected, actual)).ToArray();
            foreach (var failingMatcher in failingMatchers)
            {
                result.AddFailure(failingMatcher.PropertyPath, failingMatcher.FailureMessage(expected, actual));
            }
        }
    }
}