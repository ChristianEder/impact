namespace Impact.Samples.ProtobufOverHttp.Provider.Tests
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
        ""path"": ""weatherforecast/Munich/3""
      },
      ""response"": {
        ""status"": 200,
        ""body"": ""CgZNdW5pY2gQGBhLIgVTdW5ueSoGCICb5fMF""
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
