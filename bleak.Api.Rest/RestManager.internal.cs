using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace bleak.Api.Rest
{
    public partial class RestManager
    {
        protected async Task SubmitRequestAsync<TSuccess, TError>(
            HttpWebRequest httpWebRequest,
            RestResults<TSuccess, TError> summary
        )
            where TSuccess : class
            where TError : class
        {
            try
            {
                using (var response = await httpWebRequest.GetResponseAsync())
                {
                    ProcessResponse(
                        summary: ref summary,
                        response: response);
                }
            }
            catch (WebException ex) 
            {
                    ProcessWebException(summary: ref summary, ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"This request failed as a exception {ex.Message}");
            }
        }


        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <returns>The response.</returns>
        /// <param name="summary">Summary.</param>
        /// <param name="response">Response.</param>
        /// <typeparam name="TSuccess">The 1st type parameter.</typeparam>
        /// <typeparam name="TError">The 2nd type parameter.</typeparam>
        private RestResults<TSuccess, TError> ProcessWebException<TSuccess, TError>(
            ref RestResults<TSuccess, TError> summary,
            WebException ex)
            where TSuccess : class
            where TError : class
        {
            if (ex.Response == null)
            {
                Console.WriteLine("Web Exception Response is null");
            }

            var httpResponse = (HttpWebResponse)ex.Response;
            summary.Status = httpResponse.StatusCode;

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                summary.SerializedResponse = responseText;
                if (string.IsNullOrWhiteSpace(responseText))
                {
                    summary.Error = default(TError);
                    return summary;
                }
                try
                {
                    if (typeof(TError).Name == "String")
                    {
                        summary.Error = (TError)Convert.ChangeType(responseText.Trim(), typeof(TError));
                        return summary;
                    }
                    else
                    {
                        summary.Error = _deserializer.Deserialize<TError>(responseText);
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




        private void ManageHeaders(
            HttpWebRequest httpWebRequest,
            string method,
            IEnumerable<Header> headers,
            string username,
            string password,
            string accept,
            string contentType
            //CookieContainer cookieContainer
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
                httpWebRequest.UserAgent = _userAgent;
            }
            catch
            {
                try
                {
                    httpWebRequest.Headers["User-Agent"] = _userAgent;
                }
                catch
                {
                }
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
                ref RestResults<TSuccess, TError> summary,
                string url,
                WebException ex)
            where TSuccess : class
            where TError : class
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
        private RestResults<TSuccess, TError> ProcessResponse<TSuccess, TError>(
            ref RestResults<TSuccess, TError> summary,
            WebResponse response)
            where TSuccess : class
            where TError : class
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


        const string defaultPayload = "";


        /// <summary>
        /// If payload, serializedPayload, or formParamters are provided, adds the payload to the HttpRequest using a StreamWriter
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="httpWebRequest"></param>
        /// <param name="summary"></param>
        /// <param name="payload">Optional. A dotnet POCO object representing the object to be serialized, using the provided serializer, and transmitted over the wire.  Only provide this if both formParameters and serializedPayload are null </param>
        /// <param name="serializedPayload">Optional. A dotnet string representing the pre-rendered payload. Only provide this if both payload and formParameters are null</param>
        /// <param name="formPametersOptional. A collection of FormParameters, representing HTML From Parameters. Only provide this if both payload and serializedPayload are null"></param>
        private void RenderPayload<TSuccess, TError>(
            HttpWebRequest httpWebRequest,
            ref RestResults<TSuccess, TError> summary,
            object payload = null,
            string serializedPayload = defaultPayload,
            IEnumerable<FormParameter> formPameters = null
            )
            where TSuccess : class
            where TError : class
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
        /*
        protected byte[] GetFormData<TSuccess, TError>
        (
            ref RequestResponseSummary<TSuccess, TError> summary,
            params FormParameter[] parms
        )
            where TSuccess : class
            where TError : class
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
        }*/
    }
}