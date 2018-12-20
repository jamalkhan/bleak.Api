using System;
using bleak.Api.Rest;
using NUnit.Framework;

namespace bleak.Api.Rest.Xamarin.Android.Tests
{
    [TestFixture]
    public class TestsSample
    {

        [SetUp]
        public void Setup() { }


        [TearDown]
        public void Tear() { }

        [Test]
        public void LoadGoogleTest()
        {
            var s = "https://google.com";
            var results = RestManager.ExecuteRestMethod<string, string>(uri: new Uri(s), verb: HttpVerbs.GET, userAgent: "blah blah blah");
            Assert.True(results.Results.Contains("<body"));
        }
    }
}
