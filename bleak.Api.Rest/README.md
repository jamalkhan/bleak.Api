bleak.Api.Rest is a lightweight REST client library for .NET 8.

## Supported API

Use `RestClient` for all new work.

- async-first
- built on `HttpClient`
- supports typed success and error payloads
- supports custom headers, basic auth, cancellation, and serializer injection

`RestManager` is still included for backward compatibility with older callers, but it is obsolete and should be treated as legacy-only.

## Quick Start

```csharp
using bleak.Api.Rest;

var client = new RestClient();
var result = await client.ExecuteRestMethodAsync<MySuccessDto, MyErrorDto>(
    uri: new Uri("https://example.com/api/resource"),
    verb: HttpVerbs.GET,
    accept: "application/json"
);
```

## Package Notes

- Target framework: `.NET 8`
- Package ID: `bleak.Api.Rest`
- Default serialization: Newtonsoft.Json

If you need custom serialization or deserialization, implement `ISerializer` and `IDeserializer` and pass them into `RestClient`.
