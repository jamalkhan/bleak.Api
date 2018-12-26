namespace bleak.Api.Rest.Common
{

    public class BaseRestManager
    {
        protected ISerializer _serializer { get; set; }
        protected IDeserializer _deserializer { get; set; }
        protected string _userAgent { get; set; }
    }
}