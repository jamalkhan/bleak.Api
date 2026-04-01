# bleak.Api HTTP And SOAP Usage

This guide shows how to use `bleak.Api.Rest` for:

- REST `GET`
- REST `POST`
- REST `PUT`
- REST `DELETE`
- SOAP calls using HTTP `POST`

The examples are based on real usage patterns from `bleak.Martech.SalesforceMarketingCloud`, especially:

- REST authentication via `POST`
- REST resource retrieval via `GET`
- SOAP retrieve/create calls via raw XML payloads over `POST`

Where the Salesforce Marketing Cloud repo does not currently contain a concrete `PUT` or `DELETE` example, this guide shows the equivalent `bleak.Api` usage pattern using the same conventions.

## Core Concepts

The main entry point is `RestClient`.

```csharp
var client = new RestClient();
```

The main method is:

```csharp
Task<RestResults<TSuccess, TError>> ExecuteRestMethodAsync<TSuccess, TError>(
    Uri uri,
    HttpVerbs verb = HttpVerbs.GET,
    object payload = null,
    string serializedPayload = null,
    IEnumerable<FormParameter> parameters = null,
    IEnumerable<Header> headers = null,
    string username = null,
    string password = null,
    string accept = null,
    string contentType = "application/json",
    CancellationToken cancellationToken = default
)
```

Use:

- `payload` when you want `bleak.Api` to serialize an object for you
- `serializedPayload` when you already built the exact request body yourself
- `headers` for bearer tokens, content types, SOAP headers, and custom API headers
- `TSuccess` for the expected success model
- `TError` for the expected error model, or `string` when you want the raw error body

The response wrapper includes:

- `Results`
- `Error`
- `SerializedRequest`
- `SerializedResponse`
- `Status`
- `UnhandledError`

## REST Patterns

### REST POST

This is the clearest real-world pattern in `bleak.Martech.SalesforceMarketingCloud`: authenticate by posting JSON to an auth endpoint.

```csharp
var authResults = await restClient.ExecuteRestMethodAsync<SfmcAuthToken, string>(
    uri: new Uri(tokenUri),
    verb: HttpVerbs.POST,
    payload: new
    {
        grant_type = "client_credentials",
        client_id = clientId,
        client_secret = clientSecret,
        account_id = memberId,
    },
    headers: new List<Header>
    {
        new Header { Name = "Content-Type", Value = "application/json" }
    }
);
```

Use this when:

- creating a resource
- submitting credentials
- sending a JSON command or search payload

If you already have a serialized body:

```csharp
var results = await client.ExecuteRestMethodAsync<MyResponse, string>(
    uri: new Uri("https://api.example.com/items"),
    verb: HttpVerbs.POST,
    serializedPayload: "{\"name\":\"example\"}",
    headers: new[]
    {
        new Header { Name = "Content-Type", Value = "application/json" }
    }
);
```

### REST GET

This is also used throughout the Salesforce Marketing Cloud codebase for loading assets, folders, and data extension rows.

```csharp
var results = await restClient.ExecuteRestMethodAsync<SfmcRestWrapper<SfmcAsset>, string>(
    uri: new Uri(url),
    verb: HttpVerbs.GET,
    headers: headers
);
```

A typical authenticated pattern looks like this:

```csharp
var headers = new List<Header>
{
    new Header { Name = "Content-Type", Value = "application/json" },
    new Header { Name = "Authorization", Value = $"Bearer {accessToken}" }
};

var results = await client.ExecuteRestMethodAsync<MyListResponse, string>(
    uri: new Uri("https://api.example.com/items?page=1"),
    verb: HttpVerbs.GET,
    headers: headers
);
```

Use this when:

- fetching a single record
- fetching lists
- paging through API results

### REST PUT

I did not find a concrete `PUT` example in `bleak.Martech.SalesforceMarketingCloud`, but `bleak.Api` supports it the same way as `POST`. The only practical difference is the HTTP verb.

```csharp
var results = await client.ExecuteRestMethodAsync<MyUpdatedItem, string>(
    uri: new Uri("https://api.example.com/items/42"),
    verb: HttpVerbs.PUT,
    payload: new
    {
        name = "Updated Name",
        isActive = true
    },
    headers: new[]
    {
        new Header { Name = "Authorization", Value = $"Bearer {accessToken}" },
        new Header { Name = "Content-Type", Value = "application/json" }
    }
);
```

Use this when the API expects full replacement or update semantics over `PUT`.

### REST DELETE

I also did not find a concrete `DELETE` example in the Salesforce Marketing Cloud repo, but the usage is straightforward:

```csharp
var results = await client.ExecuteRestMethodAsync<string, string>(
    uri: new Uri("https://api.example.com/items/42"),
    verb: HttpVerbs.DELETE,
    headers: new[]
    {
        new Header { Name = "Authorization", Value = $"Bearer {accessToken}" }
    }
);
```

If the API returns an empty body on success, `Results` may be `null` and `Status` becomes the more important success signal.

### REST With Form Data

If an API expects `application/x-www-form-urlencoded`, use `parameters`:

```csharp
var results = await client.ExecuteRestMethodAsync<string, string>(
    uri: new Uri("https://api.example.com/token"),
    verb: HttpVerbs.POST,
    parameters: new[]
    {
        new FormParameter { Name = "client_id", Value = clientId },
        new FormParameter { Name = "client_secret", Value = clientSecret },
        new FormParameter { Name = "grant_type", Value = "client_credentials" }
    }
);
```

## SOAP Patterns

## Important SOAP Note

With `bleak.Api`, SOAP is still just an HTTP request. In practice:

- the HTTP verb is almost always `POST`
- the body is XML
- the SOAP operation is expressed in the XML envelope, not by using HTTP `GET`, `PUT`, or `DELETE`

That means SOAP `retrieve`, `create`, `update`, and `delete` are all typically sent as HTTP `POST`.

This is exactly how `bleak.Martech.SalesforceMarketingCloud` uses the library.

### SOAP POST With Raw XML

The Salesforce Marketing Cloud repo uses this pattern for SOAP `Describe` and `Create` calls:

```csharp
var results = await client.ExecuteRestMethodAsync<string, string>(
    uri: new Uri(url),
    verb: HttpVerbs.POST,
    serializedPayload: requestPayload,
    headers: BuildHeaders()
);
```

That is the simplest SOAP usage pattern in `bleak.Api`:

- manually build the XML envelope
- pass it as `serializedPayload`
- set `Content-Type` to `text/xml`
- usually ask for `string` back if you want raw XML

Example:

```csharp
var soapEnvelope = """
<?xml version="1.0" encoding="UTF-8"?>
<s:Envelope xmlns:s="http://www.w3.org/2003/05/soap-envelope">
  <s:Header>
    <Action xmlns="http://schemas.xmlsoap.org/ws/2004/08/addressing">Describe</Action>
  </s:Header>
  <s:Body>
    <!-- SOAP body here -->
  </s:Body>
</s:Envelope>
""";

var results = await client.ExecuteRestMethodAsync<string, string>(
    uri: new Uri("https://example.soap.endpoint/Service.asmx"),
    verb: HttpVerbs.POST,
    serializedPayload: soapEnvelope,
    headers: new[]
    {
        new Header { Name = "Content-Type", Value = "text/xml" },
        new Header { Name = "Accept", Value = "/" },
        new Header { Name = "Cache-Control", Value = "no-cache" }
    }
);
```

### SOAP POST With Typed XML Deserialization

The Salesforce Marketing Cloud repo also shows a typed SOAP pattern by supplying an XML serializer/deserializer and a typed envelope model:

```csharp
var soapClient = new RestClient(
    serializer: new SoapSerializer(),
    deserializer: new SoapSerializer()
);

var results = await soapClient.ExecuteRestMethodAsync<SoapEnvelope<MySoapObject>, string>(
    uri: new Uri(url),
    verb: HttpVerbs.POST,
    serializedPayload: requestPayload,
    headers: BuildHeaders()
);
```

This pattern is useful when:

- the SOAP response shape is stable
- you want strongly typed access to response fields
- you have DTOs annotated for XML serialization

In the SFMC repo, `SoapSerializer` uses `XmlSerializer`, and `SoapEnvelope<T>` maps the XML envelope/body into DTOs.

### SOAP Retrieve

In Salesforce Marketing Cloud, SOAP retrieve is still an HTTP `POST`. The operation is expressed in the XML header/body:

```xml
<a:Action s:mustUnderstand="1">Retrieve</a:Action>
```

Then the request is sent like this:

```csharp
var results = await soapClient.ExecuteRestMethodAsync<SoapEnvelope<MySoapObject>, string>(
    uri: new Uri(url),
    verb: HttpVerbs.POST,
    serializedPayload: requestPayload,
    headers: soapHeaders
);
```

### SOAP Create

The SFMC repo uses the same HTTP call shape for SOAP create:

```xml
<a:Action s:mustUnderstand="1">Create</a:Action>
```

Sent as:

```csharp
var results = await client.ExecuteRestMethodAsync<string, string>(
    uri: new Uri(url),
    verb: HttpVerbs.POST,
    serializedPayload: requestPayload,
    headers: soapHeaders
);
```

### SOAP Update

I did not find an implemented SOAP update call in the SFMC repo, but based on the same WSDL/action pattern, the bleak.Api usage would still be HTTP `POST` with an `Update` SOAP action:

```xml
<a:Action s:mustUnderstand="1">Update</a:Action>
```

```csharp
var results = await client.ExecuteRestMethodAsync<string, string>(
    uri: new Uri(url),
    verb: HttpVerbs.POST,
    serializedPayload: updateSoapEnvelope,
    headers: soapHeaders
);
```

### SOAP Delete

Likewise, SOAP delete is generally still HTTP `POST` with a `Delete` action in the envelope:

```xml
<a:Action s:mustUnderstand="1">Delete</a:Action>
```

```csharp
var results = await client.ExecuteRestMethodAsync<string, string>(
    uri: new Uri(url),
    verb: HttpVerbs.POST,
    serializedPayload: deleteSoapEnvelope,
    headers: soapHeaders
);
```

## Common Header Patterns

### Bearer Token REST Headers

```csharp
var headers = new List<Header>
{
    new Header { Name = "Content-Type", Value = "application/json" },
    new Header { Name = "Authorization", Value = $"Bearer {accessToken}" }
};
```

This matches the pattern used in the SFMC base REST APIs.

### SOAP Headers

The SFMC SOAP base class builds headers like this:

```csharp
var headers = new List<Header>
{
    new Header { Name = "Content-Type", Value = "text/xml" },
    new Header { Name = "Accept", Value = "/" },
    new Header { Name = "Cache-Control", Value = "no-cache" },
    new Header { Name = "Host", Value = hostName }
};
```

The OAuth token itself is typically embedded in the SOAP envelope body or header as XML, not sent as a bearer token header.

## Choosing Between `payload` And `serializedPayload`

Use `payload` when:

- you have a normal C# object
- JSON serialization is fine
- you want `bleak.Api` to serialize it

Use `serializedPayload` when:

- you already built the exact body yourself
- you are sending SOAP XML
- you need precise control over the request body

## Error Handling

For both REST and SOAP calls, inspect:

- `Status` for the HTTP status code
- `Error` for a deserialized error payload
- `UnhandledError` for transport or deserialization failures
- `SerializedResponse` when you need to inspect the raw body

Typical pattern:

```csharp
if (!string.IsNullOrWhiteSpace(results.UnhandledError))
{
    throw new Exception(results.UnhandledError);
}

if (results.Error is not null)
{
    throw new Exception(results.SerializedResponse);
}
```

## What Was Actually Observed In `bleak.Martech.SalesforceMarketingCloud`

Observed directly in that repo:

- REST `POST` for authentication
- REST `GET` for assets, folders, and data extension data
- SOAP `POST` with raw XML payloads
- SOAP `POST` with typed XML deserialization using a custom `SoapSerializer`
- SOAP action-based operations such as `Retrieve`, `Create`, and `Describe`

Not currently implemented there, but supported by `bleak.Api` and documented here:

- REST `PUT`
- REST `DELETE`
- SOAP `Update`
- SOAP `Delete`

## Practical Guidance

- For modern REST APIs, use `RestClient` with JSON payloads and bearer-token headers.
- For SOAP APIs, use `serializedPayload` and send the exact XML you want.
- If you want typed SOAP responses, inject an XML serializer/deserializer instead of using the default JSON serializer.
- Treat SOAP operations as action-based XML over HTTP `POST`, not as HTTP verb variations.
