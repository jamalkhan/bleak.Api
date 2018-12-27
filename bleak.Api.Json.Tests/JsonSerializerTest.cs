using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace bleak.Api.Json.Tests
{
    [TestClass]
    public class JsonSerializerTest
    {
        public class FakeObject
        {
            public string Str1 { get; set; }
            public int Int2 { get; set; }
        }

        [TestMethod]
        public void TestSerialize()
        {
            var serializer = new JsonSerializer();
            var obj = new FakeObject() { Int2 = 2, Str1 = "String1" };
            var ser = serializer.Serialize(obj);
            Assert.AreEqual(ser, "{\"Str1\":\"String1\",\"Int2\":2}");
        }

        [TestMethod]
        public void TestDeserialize()
        {
            var serializer = new JsonSerializer();
            var ser = @"{ ""Int2"": 2, ""Str1"": ""String1"" }";
            var obj = serializer.Deserialize<FakeObject>(ser);
            Assert.AreEqual(obj.Int2, 2);
            Assert.AreEqual(obj.Str1, "String1");
        }
    }
}
