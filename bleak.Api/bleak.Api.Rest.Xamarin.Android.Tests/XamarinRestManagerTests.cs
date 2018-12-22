using System;
using bleak.Api.Rest;
using NUnit.Framework;

namespace bleak.Api.Rest.Xamarin.Android.Tests
{
    [TestFixture]
    public class XamarinRestManagerTests
    {

        [SetUp]
        public void Setup() { }


        [TearDown]
        public void Tear() { }

        [Test]
        public void UserAgentTest()
        {
            var s = "https://google.com";
            var results = RestManager.ExecuteRestMethod<string, string>(uri: new Uri(s), verb: HttpVerbs.GET, userAgent: "blah blah blah");
            Assert.True(results.Results.Contains("<body"));
        }
    }
}
