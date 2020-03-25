using Impact.Samples.ProtobufOverHttp.Consumer;
using System;

namespace Impact.Samples.ProtobufOverHttp.Consumer
{
	public class StandardOut : IConsole
	{
		public void Print(string message)
		{
			Console.WriteLine(message);
		}
	}
}
