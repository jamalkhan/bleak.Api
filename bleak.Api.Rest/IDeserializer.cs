namespace bleak.Api.Rest
{
    /// <summary>
    /// Deserializes a string of data
    /// </summary>
    /// <typeparam name="T">The Type to cast the data string to.</typeparam>
    /// <param name="data">The data string to Deserialize</param>
    /// <returns>An Instantiated Object from the data string</returns>
    public interface IDeserializer
    {
        T Deserialize<T>(string data);
    }
}