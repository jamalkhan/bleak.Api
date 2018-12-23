using bleak.Api.Json;
using bleak.Api.Rest.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace bleak.Api.Rest.Core.Tests
{
    [TestClass]
    public class CoreRestManagerTests
    {
        [TestMethod]
        public void TestLoadWebpage()
        {
            IRestManager restManager = new CoreRestManager(
                serializer: new JsonSerializer(), 
                deserializer: new JsonDeserializer());
            var results = restManager.ExecuteRestMethod<string, string>(new Uri("http://google.com"));
            Assert.IsTrue(results.Results.Contains("<body"));
        }
    }
}