bleak.Api is a tool for managing API access using either REST or SOAP.

RestClient is the main star. SOAP will return at a later date.

Usage:

```
    var s = "https://reqres.in/api/users/2";
    var restClient = new RestClient();
    var results = await restClient.ExecuteRestMethodAsync<GetUserTestPoco, string>(uri: new Uri(s), verb: HttpVerbs.GET);
    Console.WriteLine($"Serialized Request: {results.SerializedRequest}");
    Console.WriteLine($"Serialized Response: {results.SerializedResponse}");
    Assert.IsTrue(results.Results.data.id > 0);
    Assert.IsTrue(results.Results.data.first_name == "Janet");
    Assert.IsTrue(results.Results.data.last_name == "Weaver");
    Assert.IsTrue(results.Results.data.avatar == "https://reqres.in/img/faces/2-image.jpg");
    Assert.IsTrue(results.Results.support.text == "Tired of writing endless social media content? Let Content Caddy generate it for you.");
```

NOTE: RestManager has been Deprecated. Please use RestClient instead. Synchronous support remains in RestManager, but RestClient ONLY has Async support.

TSuccess is the type of the object returned on success
TError is the type of object returned on error.

Perfectly valid to use strings. Can also use POCOs. If you want more sophisticated [de]serialization options, you can always implement your own ISerializer

## Current Features
- Basic invocation of REST endpoints.
- invocations of really any HTTP/HTTPS endpoint, if you know how to serialize/deserialize the data.
- header management
- app identication.
- async and sync methods
- automagic serialization/deserialization for payloads/responses.
- 1000000% more friendly to folks who want to build apps quickly and efficiently than RESTSharp. No hate. RESTSharp saved my ass many times. But this is better. Designed to be easier to get started, easier to get data out of the HTTP Request, and easier to work with C# POCOs.
- The Unlicense. I don't really care how you use it. You're not paying me. And youy're free to fork this as you see fit. Also, welcome to drop a PR to the Github Repo.

## Currently possible with some effort:
- Invoking SOAP calls. See bleak.Martech.Salesforce for more details on how to use SOAP. Expect that work to be brought in at a later date.
- working with XML based APIs

## Currently _technically_ possible but unsupported:
- cookies. If you read the code, I've stubbed out some cookie management. TBD at some point.
- Form Data. Also stubbed out.
- I'm sure a ton of other stuff.

## Tested against some of the following APIs:
- Salesforce Marketing Cloud (both REST and SOAP)
- Several Other ESPs and/or CDPs
- Azure REST APIs.
- Internal REST Apps.

## Coming in the not too distant future.
- Proper SOAP support
- More Unit Tests
- Integration Tests
- Automated deployment of NUGET packages (Azure DevOps deployment is currently broken)
- Retry logic for use with things like OAUTH based authentication. You can totally do OAuth manually right now... just make an AuthRequest, and then attach the auth variable. See this implemeneted in bleak.Martech.Salesforce

### If you like what I do, you're welcome to buy me a coffee.

* Bitcoin Address: bc1qrwysp9a5mqthnz6aygn527xjkg58lcd7u5jdz5
* Solana Address: 9Y8ZUL1MqXQkE5D2Qj9gx5mgkXkXct22jkQ3Ucj5z5N3
* Ethereum Address: 0xA38305025bF8D7c9d92F73193CD5A70E9B030208