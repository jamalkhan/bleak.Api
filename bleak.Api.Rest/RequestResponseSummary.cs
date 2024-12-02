namespace bleak.Api.Rest
{
    public class RestResults<TSuccess, TError> : BaseHttpResults
    {
        public TSuccess Results { get; set; }
        public TError Error { get; set; }
        public string UnhandledError { get; set; }
    }
}