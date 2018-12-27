using System;
using bleak.Api.Rest.Portable;
using NUnit.Framework;
using bleak.Api.Rest.Common;

namespace bleak.Api.Rest.Xamarin.Android.Tests
{
    [TestFixture]
    public class PortableRestManagerTests
    {
        [SetUp]
        public void Setup() { }


        [TearDown]
        public void Tear() { }

        [Test]
        public void UserAgentTest()
        {
            var s = "https://google.com";
            var serializer = new PortableJsonSerializer();
            var restManager = new PortableRestManager(serializer, serializer, "PortableRestManagerTest");
            var results = restManager.ExecuteRestMethod<string, string>(uri: new Uri(s), verb: HttpVerbs.GET);
            Assert.True(results.Results.Contains("<body"));
        }
    }
}
