using Microsoft.VisualStudio.TestTools.UnitTesting;
using bleak.Api.Rest;
using System;

namespace bleak.Api.Rest.Tests
{
    [TestClass]
    public class NetCoreRestHelperTest
    {
        [TestMethod]
        public void LoadGoogleTest()
        {
            var s = "https://google.com";
            var results = RestManager.ExecuteRestMethod<string, string>(uri: new Uri(s), verb: HttpVerbs.GET, userAgent: "blah blah blah");
            Assert.IsTrue(results.Results.Contains("<body"));
        }
    }
}
