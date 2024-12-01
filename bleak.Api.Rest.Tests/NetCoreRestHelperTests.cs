using Microsoft.VisualStudio.TestTools.UnitTesting;
using bleak.Api.Rest;
using System;

namespace bleak.Api.Rest.Tests
{
    [TestClass]
    public class NetCoreRestHelperTests
    {
        [TestMethod]
        public void UserAgentTest()
        {
            var s = "https://google.com";
            var serializer = new JsonSerializer();
            var restManager = new RestManager(serializer, serializer);
            var results = restManager.ExecuteRestMethod<string, string>(uri: new Uri(s), verb: HttpVerbs.GET);
            Assert.IsTrue(results.Results.Contains("<body"));
        }
    }
}
