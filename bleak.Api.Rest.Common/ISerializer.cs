namespace bleak.Api.Rest.Common
{
    /// <summary>
    /// Serializes an Object
    /// </summary>
    /// <param name="obj">The Object to be serialized</param>
    /// <returns>A data string of the Object</returns>
    public interface ISerializer
    {
        string Serialize(object obj);
    }
}