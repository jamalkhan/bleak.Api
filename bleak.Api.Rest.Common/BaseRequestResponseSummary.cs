﻿using System.Collections.Generic;
using System.Net;

namespace bleak.Api.Rest.Common
{
    public abstract class BaseRequestResponseSummary
    {
        public string SerializedRequest { get; set; }
        public string SerializedResponse { get; set; }
        public IEnumerable<Header> RequestHeaders { get; set; }
        public IEnumerable<Header> ResponseHeaders { get; set; }
        public HttpStatusCode Status { get; set; }
    }
}