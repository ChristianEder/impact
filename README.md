# Impact

![Build, Test And Deploy Impact](https://github.com/ChristianEder/impact/workflows/Build,%20Test%20And%20Deploy%20Impact/badge.svg)

Impact is a .NET libary that implements the "Consumer Driven Contract Testing" pattern as promoted by [PACT](https://docs.pact.io/). 

Consumer driven contract testing is all about providing means to test the communication between the consumer of an API and the provider of an API in a decoupled way. As a rule of thumb, consumer driven contract testing might be the right tool for you, if you
- Have different teams, or even organizations, working on the consumer and provider of an API
- Feel the need to create a mock implementation of the API to support the development and testing process of the consuming application

[![Impact.Consumer](https://img.shields.io/nuget/v/Impact.Consumer.png "Latest nuget package for Impact.Consumer") Impact.Consumer](https://www.nuget.org/packages/Impact.Consumer/)

[![Impact.Consumer.Transport.Http](https://img.shields.io/nuget/v/Impact.Consumer.Transport.Http.png "Latest nuget package for Impact.Consumer.Transport.Http") Impact.Consumer.Transport.Http](https://www.nuget.org/packages/Impact.Consumer.Transport.Http/)

[![Impact.Provider](https://img.shields.io/nuget/v/Impact.Provider.png "Latest nuget package for Impact.Provider") Impact.Provider](https://www.nuget.org/packages/Impact.Provider/)

[![Impact.Provider.Transport.Http](https://img.shields.io/nuget/v/Impact.Provider.Transport.Http.png "Latest nuget package for Impact.Provider.Transport.Http") Impact.Provider.Transport.Http](https://www.nuget.org/packages/Impact.Provider.Transport.Http/)

[![Impact.Core.Payload.Json](https://img.shields.io/nuget/v/Impact.Core.Payload.Json.png "Latest nuget package for Impact.Core.Payload.Json") Impact.Core.Payload.Json](https://www.nuget.org/packages/Impact.Core.Payload.Json/)

[![Impact.Core.Payload.Protobuf](https://img.shields.io/nuget/v/Impact.Core.Payload.Protobuf.png "Latest nuget package for Impact.Core.Payload.Protobuf") Impact.Core.Payload.Protobuf](https://www.nuget.org/packages/Impact.Core.Payload.Protobuf/)

# Usage

The concept of consumer drive contract testing involves two parties to add a specific type of test into their development workflow.

- The consumer of an API (e.g. a mobile app that uses a RESTful API to retrieve data) implements a test that
  - Defines the shape of requests the consumer will send to the API
  - Defines the shape responses the consumer expects to get from the API
  - Uses these defined expectations to setup a mocked API
  - Tests the consuming application against this mocked API
  - Publishes these expectations in the form a PACT file like [this](https://github.com/DiUS/pactjs0/blob/master/test/unit/sample-pact.json) to some shared repository (e.g. a [Pact broker](https://docs.pact.io/getting_started/sharing_pacts) instance)
  - An example for a consumer test can be found [here](src/Samples/JsonOverHttp/Impact.Samples.JsonOverHttp.Consumer.Tests/PrintWeatherForecastUseCaseTest.cs)
- The provider of an API implements a test that  
  - loads the published expectations of the consumer encoded in the PACT file
  - Replays the requests encoded in the file against its API implementation
  - Compares the actual responses to the expected responses encoded in the file
  - Optionally, publishes its test results to a [Pact broker](https://docs.pact.io/getting_started/sharing_pacts) instance. This will enable the PACT broker to build up a compatibility matrix over time, which you can use to identify breaking changes between provider and consumer.
  - An example for a provider test can be found [here](src/Samples/JsonOverHttp/Impact.Samples.JsonOverHttp.Provider.Tests/HonourConsumerPact.cs)

## Implementing a consumer test

We'll look into writing a consumer test for an application consuming an API that uses JSON serialized payload over an HTTP connection. In our scenario, the API provides weather forecasts, and the consuming application prints these forecasts to the console.

You will have to start by defining the consumers expectations in terms of which requests it will send and what responses it expects to get from the API:

```cs
var pact = new Pact();
// Define a new expected interactio for requesting the weather forecast data. 
// Our example API only has this one endpoint. In a real world
// scenario, you'll most likely have multiple definitions like the following. 
pact.Given("")
    .UponReceiving("A weather forecast request")
    // Define an example request the consumer will be sending
    .With(new HttpRequest
    {
        Method = System.Net.Http.HttpMethod.Get,
        Path = "weatherforecast/Munich/3"
    })
    // If your test cases that will be run later will send
    // Requests that do not exactly match the provided example
    // request, you can use request matching rules to 
    // further define the generalized shape of your request
    .WithRequestMatchingRule(r => r.Path, r => r.Regex(pathFormat.ToString()))
    // Define how you expect the API to respond to the given request
    // This callback will be used later when setting up
    // the mocked API server
    .WillRespondWith(request =>
    {
        var match = pathFormat.Match(request.Path);
        return new HttpResponse
        {
            Status = System.Net.HttpStatusCode.OK,
            Body = new WeatherForecast
            {
                City = match.Groups[1].Value,
                Date = new DateTime(2020, 3, 24),
                Summary = "Sunny",
                TemperatureC = 24,
                TemperatureF = 75
            }
        };
    })
    // Define your expectations regarding the response provided
    // by the API. The following matching rule means
    // that you expect every field of the body to have the same
    // type as the fields you provided in the response
    // callback above.
    .WithResponseMatchingRule(r => ((WeatherForecast)r.Body), r => r.Type());
```

Now, you'll need to start a mock API server that you can use to test the consuming application against:

```cs
// Create an new mock server, providing the Pact 
// that contains all the expected requests and responses,
// and the payload format
var server = new HttpMockServer(pact, new JsonPayloadFormat());
server.Start();
```

Now you can use the mock server to instantiate an HttpClient to be used in your tests:

```cs
[Fact]
public async Task ShouldPrintTheForecast()
{
    var console = new TestConsole();
    // This HttpClient instance will access the mock
    // server we set up above
    var httpClient = server.CreateClient();
    var useCase = new PrintWeatherForecastUseCase(new WeatherForecastApiClient(httpClient), console);

    await useCase.Print("Berlin");

    // The message printed to the console should now contain
    // - the city name we provided in this test case
    // - the date we provided when setting up the Pact response callback
    Assert.Single(console.Messages);
    Assert.Equal("The weather in Berlin on 24.03.2020 will be Sunny at 24°C", console.Messages.Single());
}
```

You can get the full example [here](src/Samples/JsonOverHttp/Impact.Samples.JsonOverHttp.Consumer.Tests).

## Implementing a provider test

Lets take a look into implementing a provider test for an API sending JSON over HTTP.

First, you need to do some configuration in order to create an instance of the Pact class:

```cs
// Create a new HTTP client configured to call the API under test
var client = new HttpClient();
client.BaseAddress = new Uri("http://localhost:60374/");

// Load the pact file published by the consumer
var pactFileContents = ...;

// Configure the transport and payload formats to be JSON over HTTP
var transport = new HttpTransport(client, new JsonPayloadFormat());

// Create the Pact instance using
// - the loaded pact file contents
// - the configured transport & payload
// - a callback used to prepare preconditions to be met for each expectation in the Pact file
pact = new Pact(pactFileContents, transport, s => Task.CompletedTask);
```
Next, you'll have to write a single test method using this Pact instance for validation:

```cs
[Fact]
public async Task HonourPact()
{
    var result = await pact.Honour();
    Assert.True(result.Success, string.Join(Environment.NewLine, result.Results.Select(r => r.FailureReason)));
}
```

You can get the full example [here](src/Samples/JsonOverHttp/Impact.Samples.JsonOverHttp.Provider.Tests).


# Supported API transport types and serialization formats

Impact comes with support for the following types of transport:
- HTTP
- Callback (in-process methods as the APIs to be tested)
- You can add your own by implememting custom transport support. See the HTTP transport implementation for reference. You'll need to implement
  - Transport specific data models and matchers similar to [these](/src/Impact.Core.Transport.Http)
  - For the consumer side, transport specific serialization logic and a way to serve a mocked provider API like [here](src/Impact.Consumer.Transport.Http)
  - For the provider side, transport specific deserialization logic and a way to send requests to an actual API like [here](src/Impact.Provider.Transport.Http) 

Impact comes with support for the following serialization formats:
- JSON
- Protobuf
- You can add your own by implementing the [IPayloadFormat](src/Impact.Core/Serialization/IPayloadFormat.cs) interface. Examples can be found in the [JSON](src/Impact.Core.Payload.Json) and [Protobuf](src/Impact.Core.Payload.Protobuf) implementations.

# Should I use Impact or pact-net?

Besides Impact, there is also the official .NET implementation provided by the PACT Foundation at [https://github.com/pact-foundation/pact-net](https://github.com/pact-foundation/pact-net). One of the major differences between Impact and the official pact-net package is that Impact offers the ability to add support for different types of transport (e.g. HTTP) and payload serialization (e.g. JSON or Protobuf), whereas the official implementation focuses on JSON over HTTP. If your scenario involves only JSON over HTTP APIs, you might be better off using the official package.

