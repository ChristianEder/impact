using System;
using System.Threading.Tasks;

namespace Impact.Provider.Transport.Callbacks
{
	public class CallbackTransport : ITransport
	{
		private readonly Func<object, Task<object>> callback;

		public CallbackTransport(ITransportFormat transportFormat, Func<object, Task<object>> callback)
		{
			Format = transportFormat;
			this.callback = callback;
		}

		public ITransportFormat Format { get; }

		public Task<object> Respond(object request)
		{
			return callback(request);
		}
	}
}
