# Impact

![Build, Test And Deploy Impact](https://github.com/ChristianEder/impact/workflows/Build,%20Test%20And%20Deploy%20Impact/badge.svg)

Impact is a .NET libary that implements the "Consumer Driven Contract Testing" pattern as promoted by [PACT](https://docs.pact.io/). 

Consumer driven contract testing is all about providing means to test the communication between the consumer of an API and the provider of an API in a decoupled way. As a rule of thumb, consumer driven contract testing might be the right tool for you, if you
- Have different teams, or even organizations, working on the consumer and provider of an API
- Feel the need to create a mock implementation of the API to support the development and testing process of the consuming application

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

