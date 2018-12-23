namespace bleak.Api.Rest.Common
{
    public class RequestResponseSummary<TSuccess, TError> : BaseRequestResponseSummary
    {
        public TSuccess Results { get; set; }
        public TError Error { get; set; }
        public string UnhandledError { get; set; }
    }
}