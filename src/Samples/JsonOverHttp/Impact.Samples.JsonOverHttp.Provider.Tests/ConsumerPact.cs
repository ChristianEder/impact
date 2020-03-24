using System;
using System.Collections.Generic;
using System.Text;

namespace Impact.Samples.JsonOverHttp.Provider.Tests
{
	internal static class ConsumerPact
	{
		public static string GetPactFile()
		{
			// In a real world scenario, you'd get the pact file from a pact broker or other central repository
			return @"
{
  ""consumer"": {
    ""name"": ""Weather Forecast consumer""
  },
  ""provider"": {
    ""name"": ""Weather Forecast API""
  },
  ""interactions"": [
    {
      ""description"": ""A weather forecast request"",
      ""request"": {
        ""method"": {
          ""Method"": ""GET""
        },
        ""path"": ""weatherforecast/Munich/3"",
        ""body"": null
      },
      ""response"": {
        ""status"": 200,
        ""body"": {
          ""city"": ""Munich"",
          ""date"": ""2020-03-24T00:00:00"",
          ""temperatureC"": 24,
          ""temperatureF"": 75,
          ""summary"": ""Sunny""
        }
      },
      ""matchingRules"": {
        ""$.Body"": [
          {
            ""match"": ""type""
          }
        ]
      }
    }
  ]
}
".Trim();
		}
	}
}
