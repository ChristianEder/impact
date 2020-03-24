using System.Collections.Generic;

namespace Impact.Samples.JsonOverHttp.Consumer.Tests
{
	public class TestConsole : IConsole
	{
		public List<string> Messages { get; } = new List<string>();

		public void Print(string message)
		{
			Messages.Add(message);
		}
	}
}
