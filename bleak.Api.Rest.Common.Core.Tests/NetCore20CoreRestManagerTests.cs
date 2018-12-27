using System;
using bleak.Api.Json;
using bleak.Api.Rest.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace bleak.Api.Rest.Common.Core.Tests
{
    [TestClass]
    public class NetCore20CoreRestManagerTests
    {
        [TestMethod]
        public void TestLoadWebpage()
        {
            var serializer = new JsonSerializer();
            IRestManager restManager = new CoreRestManager(
                serializer: serializer,
                deserializer: serializer);
            var results = restManager.ExecuteRestMethod<string, string>(new Uri("http://google.com"));
            Assert.IsTrue(results.Results.Contains("<body"));
        }
    }
}
