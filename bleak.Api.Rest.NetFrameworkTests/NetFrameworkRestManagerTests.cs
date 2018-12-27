using System;
using bleak.Api.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace bleak.Api.Rest.NetFrameworkTests
{
    [TestClass]
    public class NetFrameworkRestManagerTests
    {
        [TestMethod]
        public void UserAgentTest()
        {
            var s = "https://google.com";
            var results = RestManager.ExecuteRestMethod<string, string>(uri: new Uri(s), verb: HttpVerbs.GET, userAgent: "blah blah blah");
            Assert.IsTrue(results.Results.Contains("<body"));
        }
    }
}
