using System;
using System.Net.Http;
using Impact.Consumer.Define;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Impact.Consumer.Serve.Http
{
    public class HttpMockServer : IDisposable
    {
        private readonly Pact pact;
        private TestServer testServer;

        public HttpMockServer(Pact pact)
        {
            this.pact = pact;
        }

        public void Start()
        {
            testServer = new TestServer(new WebHostBuilder().ConfigureServices(s => s.AddSingleton(pact)).UseStartup<HttpMockServerStartup>());
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