using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace bleak.Api.Rest
{
    public partial class RestManager : BaseRestManager, IRestManager
    {
        public RestManager(ISerializer serializer, IDeserializer deserializer)
        {
            _serializer = serializer;
            _deserializer = deserializer;
            _userAgent = $"{new StackTrace().GetFrame(1).GetMethod().DeclaringType.Assembly.GetName().Name} API Connector/{GetType().Assembly.GetName().Version} ({Environment.OSVersion})";
        }


        public async Task<RestResults<TSuccess, TError>> ExecuteRestMethodAsync<TSuccess, TError>
        (
            Uri uri,
            HttpVerbs verb = HttpVerbs.GET,
            object payload = null,
            string serializedPayload = null,
            IEnumerable<FormParameter> parameters = null,
            IEnumerable<Header> headers = null,
            string username = null,
            string password = null,
            string accept = null,
            string contentType = "application/json"
        )
        {
            string url = uri.ToString();
            string method = verb.ToString();

            //if (!AcceptedMethods.Contains(method))
            //    throw new ArgumentException(method + " is not currently supported.", method);

            if (payload != null && !string.IsNullOrEmpty(serializedPayload) && parameters != null)
            {
                throw new ArgumentOutOfRangeException("payload, serializedPayload, and pameters are mutually exclusive.");
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            var summary = new RestResults<TSuccess, TError>();

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

                ManageHeaders(
                   httpWebRequest: httpWebRequest,
                   method: method,
                   headers: headers,
                   username: username,
                   password: password,
                   accept: accept,
                   contentType: contentType
                   //cookieContainer: cookieContainer
                   );

                RenderPayload(httpWebRequest, ref summary, payload, serializedPayload, parameters);

                await SubmitRequestAsync<TSuccess, TError>(httpWebRequest, summary);

                return summary;
            }
            catch (WebException ex)
            {
                HandleWebException<TSuccess, TError>(
                    summary: ref summary,
                    url: url,
                    ex: ex);
                return summary;
            }
            catch (Exception ex)
            {
                summary.Results = default(TSuccess);
                summary.UnhandledError = ex.Message;
                summary.Status = 0;
                return summary;
            }
        }


        public RestResults<TSuccess, TError> ExecuteRestMethod<TSuccess, TError>
        (
            Uri uri,
            HttpVerbs verb = HttpVerbs.GET,
            object payload = null,
            string serializedPayload = null,
            IEnumerable<FormParameter> parameters = null,
            IEnumerable<Header> headers = null,
            string username = null,
            string password = null,
            string accept = null,
            string contentType = "application/json"
        )
        {
            var result = ExecuteRestMethodAsync<TSuccess, TError>(
                uri: uri,
                verb: verb,
                payload: payload,
                serializedPayload: serializedPayload,
                parameters: parameters,
                headers: headers,
                username: username,
                password: password,
                accept: accept,
                contentType: contentType
            );
            var retval = result.GetAwaiter();
            return retval.GetResult();
        }
    }
}