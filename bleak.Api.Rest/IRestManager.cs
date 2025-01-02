using System;
using System.Collections.Generic;

namespace bleak.Api.Rest
{
    public interface IRestManager
    {
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
        RestResults<TSuccess, TError> ExecuteRestMethod<TSuccess, TError>
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
            //CookieContainer cookieContainer,
            string contentType = null
        )
            where TSuccess : class
            where TError : class
            ;
    }
}