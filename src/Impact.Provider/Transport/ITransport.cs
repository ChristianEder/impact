using Impact.Core;
using Impact.Provider.Transport;
using System.Threading.Tasks;

namespace Impact.Provider.Transport
{
	public interface ITransport
    {
        ITransportFormat Format { get; }
        ITransportMatchers Matchers { get; }
        Task<object> Respond(object request);
    }
}
