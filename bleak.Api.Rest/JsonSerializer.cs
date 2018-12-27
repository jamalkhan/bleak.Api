using bleak.Api.Rest.Common;
using Newtonsoft.Json;

namespace bleak.Api.Rest
{
    public class JsonSerializer : ISerializer, IDeserializer
    {
        JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>
        /// Serializes an Object
        /// </summary>
        /// <param name="obj">The Object to be serialized</param>
        /// <returns>A JSON string of the Object</returns>
        public string Serialize(object obj)
        {
            string serializedObj = JsonConvert.SerializeObject(obj,
                Formatting.None,
                _settings);
            return serializedObj;
        }

        /// <summary>
        /// Deserializes a JSON string
        /// </summary>
        /// <typeparam name="T">The Type to cast the JSON String to.</typeparam>
        /// <param name="json">The JSON string to Deserialize</param>
        /// <returns>An Instantiated Object from the JSON</returns>
        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}