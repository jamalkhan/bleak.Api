using bleak.Api.Rest.Common;
using Newtonsoft.Json;
using System;

namespace bleak.Api.Json
{
    public class JsonSerializer : ISerializer
    {
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }

    public class JsonDeserializer : IDeserializer
    {
        public T Deserialize<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
