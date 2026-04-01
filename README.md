bleak.Api is a lightweight .NET library for calling HTTP APIs and working with request/response payloads as either raw strings or typed POCOs.

## Status

- `RestClient` is the supported API surface.
- `RestManager` is still shipped for legacy synchronous callers, but it is obsolete and should not be used for new work.
- The package currently targets `.NET 8` and publishes as `bleak.Api.Rest`.

## Recommended Usage

```csharp
using bleak.Api.Rest;

var uri = new Uri("https://example.com/api/users/2");
var client = new RestClient();

var result = await client.ExecuteRestMethodAsync<MySuccessDto, MyErrorDto>(
    uri: uri,
    verb: HttpVerbs.GET,
    accept: "application/json",
    cancellationToken: cancellationToken
);

if (result.Results is not null)
{
    Console.WriteLine(result.SerializedResponse);
}
else
{
    Console.WriteLine(result.UnhandledError ?? result.SerializedResponse);
}
```

## API Overview

`RestClient` supports:

- `GET`, `POST`, `PUT`, `PATCH`, `DELETE`, and other verb-based HTTP calls
- typed success and error payloads
- custom headers
- basic authentication
- serializer/deserializer injection via `ISerializer` and `IDeserializer`
- JSON payloads and form-url-encoded payloads
- cancellation tokens

The default serializer is Newtonsoft.Json via `JsonSerializer`.

## Legacy API

`RestManager` remains in the package for backward compatibility, especially for synchronous usage, but it is marked obsolete in code and still uses `HttpWebRequest`.

For new development:

- prefer `RestClient`
- prefer async flows
- inject your own `HttpClient` when you need custom lifetime or test control

## CI And Publishing

This repository currently contains both GitHub Actions and Azure Pipelines configuration:

- GitHub Actions is the primary CI/CD path in the repo today: `.github/workflows/dotnet.yml`
- Azure Pipelines remains as a compatibility/legacy pipeline definition: `azure-pipelines.yaml`

NuGet publishing is configured from CI and expects a secret/API key in the respective platform.

## Notes

- The current automated tests are mostly integration-style tests against external services.
- SOAP support is not currently part of this package.
- If you need more advanced serialization behavior, provide custom `ISerializer` and `IDeserializer` implementations.
