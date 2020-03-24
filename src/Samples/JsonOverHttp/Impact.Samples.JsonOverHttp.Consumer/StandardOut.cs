using System;

namespace Impact.Samples.JsonOverHttp.Consumer
{
	public class StandardOut : IConsole
	{
		public void Print(string message)
		{
			Console.WriteLine(message);
		}
	}
}
