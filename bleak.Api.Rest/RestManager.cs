using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace bleak.Api.Rest
{
    public partial class RestManager : BaseRestManager, IRestManager, IRestManagerAsync
    {
        public RestManager(ISerializer serializer, IDeserializer deserializer)
        {
            _serializer = serializer;
            _deserializer = deserializer;
            _userAgent = $"{new StackTrace().GetFrame(1).GetMethod().DeclaringType.Assembly.GetName().Name} API Connector/{GetType().Assembly.GetName().Version} ({Environment.OSVersion})";
        }

        /// <summary>
        /// Executes a REST method asynchronously using the specified parameters and returns the results.
        /// </summary>
        /// <typeparam name="TSuccess">The type representing a successful response.</typeparam>
        /// <typeparam name="TError">The type representing an error response.</typeparam>
        /// <param name="uri">The URI of the REST endpoint.</param>
        /// <param name="verb">The HTTP verb to use for the request (default is GET).</param>
        /// <param name="payload">The object to be serialized and sent as the request body (optional).</param>
        /// <param name="serializedPayload">A pre-serialized payload to send as the request body (optional).</param>
        /// <param name="parameters">A collection of form parameters to include i           n the request (optional).</param>
        /// <param name="headers">A collection of headers to include in the request (optional).</param>
        /// <param name="username">The username for basic authentication (optional).</param>
        /// <param name="password">The password for basic authentication (optional).</param>
        /// <param name="accept">The value for the Accept header (optional).</param>
        /// <param name="contentType">The value for the Content-Type header (default is "application/json").</param>
        /// <returns>
        /// A <see cref="RestResults{TSuccess, TError}"/> object containing the results of the REST request.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="payload"/>, <paramref name="serializedPayload"/>, and <paramref name="parameters"/> are all provided, as they are mutually exclusive.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="uri"/> is null or empty.
        /// </exception>
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
            where TSuccess : class
            where TError : class
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

        /// <summary>
        /// Executes the rest method.
        /// </summary>
        /// <returns>The rest method.</returns>
        /// <param name="uri">URI.</param>
        /// <param name="verb">Verb.</param>
        /// <param name="userAgent">User agent.</param>
        /// <param name="payload">Payload.</param>
        /// <param name="serializedPayload">Serialized payload.</param>
        /// <param name="parameters">Parameters.</param>
        /// <param name="headers">Headers.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="accept">Accept.</param>
        /// <param name="cookieContainer">Cookie container.</param>
        /// <param name="contentType">Content type.</param>
        /// <typeparam name="TSuccess">The 1st type parameter.</typeparam>
        /// <typeparam name="TError">The 2nd type parameter.</typeparam>
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
            where TSuccess : class
            where TError : class
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
            return result.ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}