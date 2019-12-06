using System;
using System.Linq;
using Impact.Tests.Shared;

namespace Impact.Consumer.Tests
{
    public class PublishedPact
    {
        public static string Get()
        {
            return CreateMockServer().ToPactFile("Consumer", "Provider", new JsonRequestResponseSerializer());
        }

        public static readonly Func<Request, Response> ValidRequestHandler = (Request request) =>
        {
            var response = new Response();

            if (request.Type == "Foo")
            {
                response.Foos.AddRange(request.Ids.Select(i => new Foo {Id = i, Size = i.Length}));
            }

            if (request.Type == "Bar")
            {
                response.Bars.AddRange(request.Ids.Select(i => new Bar {Id = i, Name = "Bar: " + i}));
            }

            return response;
        };

        internal static MockServer CreateMockServer(Func<Request, Response> requestHandler = null)
        {
            var mockServer = new MockServer();

            mockServer
                .Given("A foo with id 1 exists")
                .UponReceiving("A get foo request")
                .With(new Request
                {
                    Type = "Foo",
                    Ids = { "1" }
                })
                .WithRequestArrayMatchingRule(r => r.Ids, i => i.LengthMin(1).All().Regex("[1-9][0-9]*"))
                .WillRespondWith(requestHandler ?? ValidRequestHandler)
                .WithResponseArrayMatchingRule(r => r.Bars, b => b.LengthMax(0))
                .WithResponseArrayMatchingRule(r => r.Foos, r => r.Length(1).At(0).With(f => f.Size, s => s.TypeMin(0)));

            mockServer
                .Given("A bar with id 12 exists")
                .UponReceiving("A get bar request")
                .With(new Request
                {
                    Type = "Bar",
                    Ids = { "12" }
                })
                .WithRequestArrayMatchingRule(r => r.Ids, i => i.LengthMin(1).All().Regex("[1-9][0-9]*"))
                .WillRespondWith(requestHandler ?? ValidRequestHandler)
                .WithResponseArrayMatchingRule(r => r.Foos, b => b.LengthMax(0))
                .WithResponseArrayMatchingRule(r => r.Bars, r => r.Length(1).At(0).With(f => f.Name, s => s.Type()));

            return mockServer;
        }
    }
}