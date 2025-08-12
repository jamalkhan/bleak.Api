using Microsoft.VisualStudio.TestTools.UnitTesting;
using bleak.Api.Rest;
using System;
using System.Threading.Tasks;

namespace bleak.Api.Rest.Tests;



[TestClass]
public class RestClientTests
{

    [TestMethod]
    public async Task RestClientGetSuccessStringTest()
    {
        var s = "https://www.google.com";
        Console.WriteLine($"Testing {s}");
        var restManager = new RestClient();
        var results = await restManager.ExecuteRestMethodAsync<string, string>(
            uri: new Uri(s),
            verb: HttpVerbs.GET);
        Assert.IsTrue(results?.Results?.Contains("<body"));
    }


    // Using https://reqres.in/ as tests

    [TestMethod]
    public async Task RestClientGetSuccessUserTest()
    {
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
    }
    

    [TestMethod]
    public async Task RestClientPostUserTest()
    {
        var s = "https://reqres.in/api/users";
        var payload = new PostUserTestPoco{ name="jamal", job="engineer" };

        var restClient = new RestClient();
        var results = await restClient.ExecuteRestMethodAsync<PostResultUserTestPoco, string>
            (uri: new Uri(s),
            verb: HttpVerbs.POST,
            payload: payload,
            headers: new Header[] { new Header() { Name = "x-api-key", Value = "reqres-free-v1" }}
            );

        Console.WriteLine($"Serialized Request: {results.SerializedRequest}");
        Console.WriteLine($"Serialized Response: {results.SerializedResponse}");
        Assert.IsTrue(int.Parse(results.Results.id) > 0);
        Assert.IsTrue(results.Results.name == "jamal");
        Assert.IsTrue(results.Results.job == "engineer");

        // Ensure the date is within -24 hours and +24 hours from now.
        // * I could test more precisely,
        // * but I'm not testing reqres.in's ability to generate a date;
        TimeSpan difference = DateTime.Now - results.Results.createdAt;
        Assert.IsTrue(difference.TotalHours <= 24 && difference.TotalHours >= -24);
    }

    [TestMethod]
    public async Task RestClientPutUserTest()
    {
        var s = "https://reqres.in/api/users/2";
        var payload = new PostUserTestPoco{ name="jamal", job="test engineer" };

        var restClient = new RestClient();
        var results = await restClient.ExecuteRestMethodAsync<PostResultUserTestPoco, string>
            (uri: new Uri(s),
            verb: HttpVerbs.POST,
            payload: payload,
            headers: new Header[] { new Header() { Name = "x-api-key", Value = "reqres-free-v1" }}
            );

        Console.WriteLine($"Serialized Request: {results.SerializedRequest}");
        Console.WriteLine($"Serialized Response: {results.SerializedResponse}");
        Assert.IsTrue(results.Results.name == "jamal");
        Assert.IsTrue(results.Results.job == "test engineer");

        // Ensure the date is within -24 hours and +24 hours from now.
        // * I could test more precisely,
        // * but I'm not testing reqres.in's ability to generate a date;
        TimeSpan difference = DateTime.Now - results.Results.createdAt;
        Assert.IsTrue(difference.TotalHours <= 24 && difference.TotalHours >= -24);
    }

    [TestMethod]
    public async Task RestClientLoginSuccessTest()
    {
        var s = "https://reqres.in/api/login";
        var payload = new LoginTestPoco { email="eve.holt@reqres.in", password="abc123" };

        var restManager = new RestClient();
        var results = await restManager.ExecuteRestMethodAsync<LoginSuccessPoco, LoginFailPoco>
            (uri: new Uri(s),
            verb: HttpVerbs.POST,
            payload: payload,headers: new Header[] { new Header() { Name = "x-api-key", Value = "reqres-free-v1" }}
            );

        Console.WriteLine($"Serialized Request: {results.SerializedRequest}");
        Console.WriteLine($"Serialized Response: {results.SerializedResponse}");
        //Assert.IsTrue(results.HttpCode == 200);
        Assert.IsTrue(!string.IsNullOrEmpty(results.Results.token));
    }

    [TestMethod]
    public async Task RestClientLoginFailureTest()
    {
        var s = "https://reqres.in/api/login";
        var payload = new LoginTestPoco { email = "eve.holt@reqres.in" };

        var restManager = new RestClient();
        try
        {
            RestResults<LoginSuccessPoco, LoginFailPoco> results;
            results = await restManager.ExecuteRestMethodAsync<LoginSuccessPoco, LoginFailPoco>
                (uri: new Uri(s),
                verb: HttpVerbs.POST,
                payload: payload,
                headers: new Header[] { new Header() { Name = "x-api-key", Value = "reqres-free-v1" } }
            );

            Assert.IsTrue(!string.IsNullOrEmpty(results.Error.error));
            Console.WriteLine($"Serialized Request: {results.SerializedRequest}");
            Console.WriteLine($"Serialized Response: {results.SerializedResponse}");
            Console.WriteLine($"Error: {results.Error.error}");
            Assert.IsTrue(results.Error.error.Contains("Missing Password", StringComparison.InvariantCultureIgnoreCase));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.ToString()}");
            Assert.IsTrue(false);
        }
    }
}