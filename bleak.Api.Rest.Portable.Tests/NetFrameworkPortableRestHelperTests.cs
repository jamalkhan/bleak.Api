using System;
using bleak.Api.Rest.Common;
using bleak.Api.Rest.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{

    [TestClass]
    public class NetFrameworkPortableRestHelperTests
    {
        [TestMethod]
        public void UserAgentTest()
        {
            var s = "https://google.com";
            var serializer = new PortableJsonSerializer();
            var restManager = new PortableRestManager(serializer, serializer, "blah blah blah");
            var results = restManager.ExecuteRestMethod<string, string>(uri: new Uri(s), verb: HttpVerbs.GET);
            Assert.IsTrue(results.Results.Contains("<body"));
        }
    }
}