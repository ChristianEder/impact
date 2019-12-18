using System;
using System.Net.Http;
using Impact.Consumer.Define;
using Impact.Core;
using Impact.Core.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Impact.Consumer.Serve.Http
{
    public class HttpMockServer : IDisposable
    {
        private readonly Pact pact;
        private readonly IPayloadFormat payloadFormat;
        private readonly ITransportMatchers transportMatchers;
        private TestServer testServer;

        public HttpMockServer(Pact pact, IPayloadFormat payloadFormat)
            : this(pact, payloadFormat, new PactV2CompliantHttpTransportMatchers())
        {
        }

        public HttpMockServer(Pact pact, IPayloadFormat payloadFormat, ITransportMatchers transportMatchers)
        {
            this.pact = pact;
            this.payloadFormat = payloadFormat;
            this.transportMatchers = transportMatchers;
        }

        public void Start()
        {
            testServer = new TestServer(new WebHostBuilder().ConfigureServices(ConfigureServices).UseStartup<HttpMockServerStartup>());
        }

        private void ConfigureServices(IServiceCollection s)
        {
            s.AddSingleton(pact);
            s.AddSingleton(payloadFormat);
            s.AddSingleton(transportMatchers);
            s.AddSingleton<ITransportFormat, HttpTransportFormat>();
        }

        public void Stop()
        {
            testServer?.Dispose();
        }

        public HttpClient CreateClient()
        {
            return testServer.CreateClient();
        }

        public void Dispose()
        {
            testServer?.Dispose();
        }
    }
}