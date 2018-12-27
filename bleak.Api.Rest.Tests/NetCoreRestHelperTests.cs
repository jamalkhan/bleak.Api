using Microsoft.VisualStudio.TestTools.UnitTesting;
using bleak.Api.Rest;
using System;
using bleak.Api.Rest.Common;

namespace bleak.Api.Rest.Tests
{
    [TestClass]
    public class NetLegacyRestHelperTests
    {
        [TestMethod]
        public void UserAgentTest()
        {
            var s = "https://google.com";
            var results = RestManager.ExecuteRestMethod<string, string>(uri: new Uri(s), verb: HttpVerbs.GET, userAgent: "blah blah blah");
            Assert.IsTrue(results.Results.Contains("<body"));
        }
    }

    [TestClass]
    public class NetCoreRestHelperTests
    {
        [TestMethod]
        public void UserAgentTest()
        {
            var s = "https://google.com";
            var serializer = new JsonSerializer();
            var restManager = new CoreRestManager(serializer, serializer);
            var results = restManager.ExecuteRestMethod<string, string>(uri: new Uri(s), verb: HttpVerbs.GET);
            Assert.IsTrue(results.Results.Contains("<body"));
        }
    }
}
