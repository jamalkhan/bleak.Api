using System.Net;

namespace bleak.Api.Rest
{

    public abstract class BaseRequestResponseSummary
    {
        public string SerializedRequest { get; set; }
        public string SerializedResponse { get; set; }
        public HttpStatusCode Status { get; set; }
    }
}