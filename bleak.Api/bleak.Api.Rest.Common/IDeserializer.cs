namespace bleak.Api.Rest.Common
{
    public interface IDeserializer
    {
        T Deserialize<T>(string data);
    }
}