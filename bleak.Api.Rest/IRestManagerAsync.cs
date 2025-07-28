using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bleak.Api.Rest
{
    public interface IRestManagerAsync
    {
        /// <summary>
        /// Executes a REST method asynchronously using the specified parameters and returns the results.
        /// </summary>
        /// <typeparam name="TSuccess">The type representing a successful response.</typeparam>
        /// <typeparam name="TError">The type representing an error response.</typeparam>
        /// <param name="uri">The URI of the REST endpoint.</param>
        /// <param name="verb">The HTTP verb to use for the request (default is GET).</param>
        /// <param name="payload">The object to be serialized and sent as the request body (optional).</param>
        /// <param name="serializedPayload">A pre-serialized payload to send as the request body (optional).</param>
        /// <param name="parameters">A collection of form parameters to include in the request (optional).</param>
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
        Task<RestResults<TSuccess, TError>> ExecuteRestMethodAsync<TSuccess, TError>
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
            where TError : class;
    }
}