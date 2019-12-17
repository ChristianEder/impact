﻿using System;
using System.Linq;
using Impact.Core;
using Impact.Core.Matchers;
using Newtonsoft.Json.Linq;

namespace Impact.Consumer.Serve.Http.Matchers
{
    public class RequestHeadersAllowWhitespaceAfterComma : Matcher
    {
        public RequestHeadersAllowWhitespaceAfterComma() : base(nameof(HttpRequest.Headers) + ".*")
        {
        }

        public override bool Matches(object expected, object actual, MatchingContext context, Action<object, object, MatchingContext> deepMatch)
        {
            if (context.MatchersForProperty.Any(m => m != this))
            {
                return true;
            }

            var expectedValue = expected?.ToString()?.Trim();
            var actualValue = actual?.ToString()?.Trim();

            if (!string.IsNullOrEmpty(expectedValue) && !string.IsNullOrEmpty(actualValue) && expectedValue.Contains(",") && actualValue.Contains(","))
            {
                context.Terminate();

                var expectedValues = expectedValue.Split(",").Select(v => v.Trim()).ToArray();
                var actualValues = actualValue.Split(",").Select(v => v.Trim()).ToArray();

                if (expectedValues.Length != actualValues.Length)
                {
                    return false;
                }

                for (int i = 0; i < expectedValues.Length; i++)
                {
                    if (expectedValues[i] != actualValues[i])
                    {
                        return false;
                    }
                }
            }
            else if(expected?.ToString() != actual?.ToString())
            {
                return false;
            }


            return true;
        }

        public override JObject ToPactMatcher()
        {
            throw new InvalidOperationException("This matcher should not be part of the pact file");
        }

        public override string FailureMessage(object expected, object actual)
        {
            return $"Header values differ. Expected: {expected}, actual: {actual}";
        }
    }
}