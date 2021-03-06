﻿using bleak.Api.Rest.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace bleak.Api.Rest.Portable
{
    public class PortableRestManager
        : BaseRestManager
        , IRestManager
    {
        public PortableRestManager(ISerializer serializer, IDeserializer deserializer, string userAgent) : base()
        {
            _serializer = serializer;
            _deserializer = deserializer;
            _userAgent = userAgent;
        }

        /*
        public PortableRestManager(ISerializer serializer, IDeserializer deserializer)
            : this(serializer, deserializer, $"{new StackTrace().GetFrame(1).GetMethod().DeclaringType.Assembly.GetName().Name} API Connector/{GetType().Assembly.GetName().Version} ({Environment.OSVersion.ToString()})")
        {
        }
        */

        public RequestResponseSummary<TSuccess, TError> ExecuteRestMethod<TSuccess, TError>
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
            string contentType = null
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

            var summary = new RequestResponseSummary<TSuccess, TError>();

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
                   );

                BuildPayload(payload, serializedPayload, parameters, httpWebRequest, ref summary);

                SubmitResponseUsing<TSuccess, TError>(ref summary, httpWebRequest);

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
        /// TODO: Could probably consolidate with ExecuteRestMethod if formParameters can be &= or multiparted.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="summary"></param>
        /// <param name="httpWebRequest"></param>
        /// <returns></returns>
        private void SubmitResponseUsing<TSuccess, TError>(
            ref RequestResponseSummary<TSuccess, TError> summary,
            HttpWebRequest httpWebRequest
            )
        {
            var asyncresponse = httpWebRequest.GetResponseAsync();
            // TODO: Implement a true Async experience

            using (var response = asyncresponse.Result)
            {
                ProcessResponse<TSuccess, TError>(
                    summary: ref summary,
                    response: response);
            }

        }


        private void ManageHeaders(
            HttpWebRequest httpWebRequest,
            string method,
            IEnumerable<Header> headers,
            string username,
            string password,
            string accept,
            string contentType
            )
        {
            if (!string.IsNullOrEmpty(username))
            {
                var authHeader = new Header()
                {
                    Name = "Authorization",
                    // TODO: Encoding.Unicode was Encoding.Default. Does this matter?
                    Value = "Basic " + Convert.ToBase64String(Encoding.Unicode.GetBytes(username + ":" + password))
                };
                if (headers == null)
                {
                    headers = new List<Header>();
                }
                headers = headers.Concat(new Header[] { authHeader });
            }

            httpWebRequest.ContentType = contentType;
            httpWebRequest.Method = method;
            //if (cookieContainer != null)
            //{
            //    httpWebRequest.CookieContainer = cookieContainer;
            //}

            try
            {
                httpWebRequest.Headers["User-Agent"] = _userAgent;
            }
            catch
            {
            }

            if (!string.IsNullOrEmpty(accept))
            {
                httpWebRequest.Accept = accept;
            }
            if (headers != null && headers.Count() > 0)
            {
                foreach (var header in headers)
                {
                    try
                    {
                        httpWebRequest.Headers[header.Name] = header.Value;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        protected void HandleWebException<TSuccess, TError>(
                ref RequestResponseSummary<TSuccess, TError> summary,
                string url,
                WebException ex)
        {

            if (ex.Response != null)
            {
                var httpResponse = (HttpWebResponse)ex.Response;
                summary.Status = httpResponse.StatusCode;
                using (var stream = httpResponse.GetResponseStream())
                {
                    string errorText = null;
                    using (var reader = new StreamReader(stream))
                    {
                        errorText = reader.ReadToEnd();
                        summary.SerializedResponse = errorText;
                    }

                    try
                    {
                        if (typeof(TError).Name == "String")
                        {
                            summary.Error = (TError)Convert.ChangeType(errorText, typeof(TError));
                        }
                        else
                        {
                            var errorObject = _deserializer.Deserialize<TError>(errorText);
                            summary.Error = errorObject;
                            return;
                        }
                    }
                    catch (Exception ex2)
                    {
                        summary.UnhandledError = ex2.Message;
                        return;
                    }
                }
            }
            //summary.Status = 0;
            summary.UnhandledError = ex.Message;
        }


        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <returns>The response.</returns>
        /// <param name="summary">Summary.</param>
        /// <param name="response">Response.</param>
        /// <typeparam name="TSuccess">The 1st type parameter.</typeparam>
        /// <typeparam name="TError">The 2nd type parameter.</typeparam>
        private RequestResponseSummary<TSuccess, TError> ProcessResponse<TSuccess, TError>(
            ref RequestResponseSummary<TSuccess, TError> summary,
            WebResponse response)
        {
            var httpResponse = (HttpWebResponse)response;
            summary.Status = httpResponse.StatusCode;

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                summary.SerializedResponse = responseText;
                if (string.IsNullOrWhiteSpace(responseText))
                {

                    summary.Results = default(TSuccess);
                    return summary;
                }
                try
                {
                    if (typeof(TSuccess).Name == "String")
                    {
                        summary.Results = (TSuccess)Convert.ChangeType(responseText.Trim(), typeof(TSuccess));
                        return summary;
                    }
                    else
                    {
                        summary.Results = _deserializer.Deserialize<TSuccess>(responseText);
                        return summary;
                    }
                }
                catch (Exception ex2)
                {
                    summary.UnhandledError = ex2.Message;
                    return null;
                }
            }
        }

        private void BuildPayload<TSuccess, TError>(
            object payload,
            string serializedPayload,
            IEnumerable<FormParameter> formPameters,
            HttpWebRequest httpWebRequest,
            ref RequestResponseSummary<TSuccess, TError> summary
            )
        {
            if (payload != null)
            {
                serializedPayload = _serializer.Serialize(payload);
            }
            if (!string.IsNullOrEmpty(serializedPayload))
            {
                if (summary != null)
                {
                    summary.SerializedRequest = serializedPayload;
                }

                // TODO: Really make this an Async behavior
                var asyncResult = httpWebRequest.GetRequestStreamAsync();
                using (var stream = new StreamWriter(asyncResult.Result))
                {
                    stream.Write(serializedPayload);
                }
            }
            if (formPameters != null && formPameters.Count() > 0)
            {
                var formData = GetFormData(ref summary, formPameters.ToArray());

                // TODO: Really make this an Async behavior
                var asyncResult = httpWebRequest.GetRequestStreamAsync();
                using (var stream = asyncResult.Result)
                {
                    stream.Write(formData, 0, formData.Length);
                }
            }
        }

        // TODO: Remove this
        private byte[] GetFormData<TSuccess, TError>(
            ref RequestResponseSummary<TSuccess, TError> summary,
            params FormParameter[] parms)
        {
            var sb = new StringBuilder();
            bool firstTime = true;
            foreach (var parm in parms)
            {
                if (firstTime)
                {
                    firstTime = false;
                }
                else
                {
                    sb.Append("&");
                }
                sb.Append(parm.Name);
                sb.Append("=");
                sb.Append(parm.Value);
            }
            summary.SerializedRequest = sb.ToString();
            var formData = Encoding.Unicode.GetBytes(sb.ToString());
            return formData;
        }
    }
}