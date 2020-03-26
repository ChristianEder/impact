using System;
using System.Linq;
using System.Threading.Tasks;
using Impact.Core;
using Impact.Core.Matchers;
using Impact.Provider.Transport;
using Newtonsoft.Json.Linq;

namespace Impact.Provider
{
	public class Interaction
	{
		private readonly ITransport transport;
		private readonly Func<string, Task> ensureProviderState;
		private readonly string description;
		private string providerState;
		private readonly object request;
		private readonly object response;
		private readonly IMatcher[] matchers;

		internal Interaction(JObject i, ITransport transport, System.Func<string, Task> ensureProviderState)
		{
			this.transport = transport;
			this.ensureProviderState = ensureProviderState;
			description = i["description"]?.ToString() ?? "";
			providerState = i["providerState"]?.ToString() ?? "";
			request = transport.Format.DeserializeRequest(i["request"]);
			response = transport.Format.DeserializeResponse(i["response"]);

			matchers = i["matchingRules"] is JObject rules ? MatcherParser.Parse(rules) : new IMatcher[0];
			matchers = transport.Matchers.ResponseMatchers.Concat(matchers).ToArray();
		}

		public async Task<InteractionVerificationResult> Honour()
		{
			if (!string.IsNullOrEmpty(providerState))
			{
				await ensureProviderState(providerState);
			}

			var actualResponse = await transport.Respond(request);
			var context = new MatchingContext(matchers, false);
			var checker = new MatchChecker();

			var roundTrippedResponse = transport.Format.DeserializeResponse(transport.Format.SerializeResponse(actualResponse));
			checker.Matches(response, roundTrippedResponse, context);

			return new InteractionVerificationResult(description, context.Result.Matches, context.Result.FailureReasons);
		}
	}

	public class InteractionVerificationResult
	{
		public InteractionVerificationResult(string description, bool success, string failureReason = null)
		{
			Description = description;
			Success = success;
			FailureReason = failureReason;
		}

		public string Description { get; }
		public bool Success { get; }
		public string FailureReason { get; }
	}
}