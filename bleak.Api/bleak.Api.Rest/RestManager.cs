using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace bleak.Api.Rest
{
    [Obsolete("This has been replaced by the non-static RestManagers in bleak.Api.Rest.Core and bleak.Api.Rest.Xamarin")]
    public static class RestManager
    {
        /// <summary>
        /// The accepted HTTP methods.
        /// </summary>
        readonly static string[] acceptedMethods = { "GET", "POST", "PUT", "DELETE" };


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
        public static RequestResponseSummary<TSuccess, TError> ExecuteRestMethod<TSuccess, TError>(
            Uri uri,
            HttpVerbs verb,
            string userAgent,
            object payload = null,
            string serializedPayload = null,
            List<FormParameter> parameters = null,
            List<Header> headers = null,
            string username = null,
            string password = null,
            string accept = null,
            CookieContainer cookieContainer = null,
            string contentType = "application/json"

            )
        {
            string url = uri.ToString();
            string method = verb.ToString();

            if (!acceptedMethods.Contains(method))
                throw new ArgumentException(method + " is not currently supported.", method);

            if (payload != null && !string.IsNullOrEmpty(serializedPayload) && parameters != null)
                throw new ArgumentOutOfRangeException("payload, serializedPayload, and pameters are mutually exclusive.");

            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

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
                    contentType: contentType,
                    userAgent: userAgent,
                    cookieContainer: cookieContainer
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
        /// Submits the multipart form.
        /// </summary>
        /// <returns>The multipart form.</returns>
        /// <param name="uri">URI.</param>
        /// <param name="verb">Verb.</param>
        /// <param name="userAgent">User agent.</param>
        /// <param name="encoding">Encoding.</param>
        /// <param name="uploadFile">Upload file.</param>
        /// <param name="headers">Headers.</param>
        /// <param name="parameters">Parameters.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="accept">Accept.</param>
        /// <param name="contentType">Content type.</param>
        /// <param name="cookieContainer">Cookie container.</param>
        /// <typeparam name="TSuccess">The 1st type parameter.</typeparam>
        /// <typeparam name="TError">The 2nd type parameter.</typeparam>
        public static RequestResponseSummary<TSuccess, TError> SubmitMultipartForm<TSuccess, TError>(
            Uri uri,
            HttpVerbs verb,
            string userAgent,
            Encoding encoding = null,
            string uploadFile = null,
            List<Header> headers = null,
            List<FormParameter> parameters = null,
            string username = null,
            string password = null,
            string accept = null,
            string contentType = "multipart/form-data",
            CookieContainer cookieContainer = null
            )
        {
            var method = verb.ToString();
            var summary = new RequestResponseSummary<TSuccess, TError>();
            try
            {
                if (encoding == null)
                {
                    encoding = Encoding.UTF8;
                }

                var httpWebRequest = WebRequest.Create(uri) as HttpWebRequest;
                if (httpWebRequest == null)
                {
                    throw new NullReferenceException("request is not a http request");
                }

                string formDataBoundary = "--" + DateTime.Now.ToString("ssfff");
                contentType = string.Format("{0}; boundary={1}", contentType, formDataBoundary);

                ManageHeaders(
                    httpWebRequest: httpWebRequest,
                    method: method,
                    headers: headers,
                    username: username,
                    password: password,
                    accept: accept,
                    contentType: contentType,
                    userAgent: userAgent,
                    cookieContainer: cookieContainer
                    );

                BuildMultiPartPayload(
                    encoding: encoding,
                    parameters: parameters,
                    contentType: contentType,
                    request: httpWebRequest,
                    formDataBoundary: formDataBoundary);

                SubmitRequestDispose<TSuccess, TError>(
                    summary: ref summary,
                    httpWebRequest: httpWebRequest);


                return summary;
            }
            catch (WebException ex)
            {
                HandleWebException<TSuccess, TError>(
                    summary: ref summary,
                    url: uri.ToString(),
                    ex: ex);
                return summary;
            }
            catch (Exception ex)
            {
                summary.SerializedResponse = ex.Message;
                summary.Status = 0;
                summary.UnhandledError = ex.Message;
                return summary;
            }
        }
        /// <summary>
        /// Builds the multi part payload.
        /// </summary>
        /// <param name="encoding">Encoding.</param>
        /// <param name="parameters">Parameters.</param>
        /// <param name="contentType">Content type.</param>
        /// <param name="request">Request.</param>
        /// <param name="formDataBoundary">Form data boundary.</param>
        private static void BuildMultiPartPayload(
            Encoding encoding,
            List<FormParameter> parameters,
            string contentType,
            HttpWebRequest request,
            string formDataBoundary)
        {
            contentType = string.Format("{0}; boundary={1}", contentType, formDataBoundary);

            var formData = GetMultipartFormData(
                parameters: parameters,
                boundary: formDataBoundary,
                encoding: encoding);

            // Send the form data to the request.
            throw new NotImplementedException("Sorry Guys, Still a WIP");
            //TODO: Uncomment this block:
            //using (var requestStream = request.GetRequestStream())
            //{
            //    requestStream.Write(formData, 0, formData.Length);
            //}
        }
        /// <summary>
        /// Gets the multipart form data.
        /// </summary>
        /// <returns>The multipart form data.</returns>
        /// <param name="parameters">Parameters.</param>
        /// <param name="boundary">Boundary.</param>
        /// <param name="encoding">Encoding.</param>
        private static byte[] GetMultipartFormData(
            List<FormParameter> parameters
            , string boundary
            , Encoding encoding
            )
        {
            Stream formDataStream = new MemoryStream();
            var needsClrf = false;

            foreach (var param in parameters)
            {
                // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
                // Skip it on the first parameter, add it to subsequent parameters.
                if (needsClrf)
                    formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                needsClrf = true;

                var value = param.Value as FileParameter;
                if (value != null)
                {
                    var fileToUpload = value;

                    // Add just the first part of this param, since we will write the file data directly to the Stream
                    var header =
                        string.Format(
                            "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n",
                            boundary,
                            param.Name,
                            fileToUpload.FileName ?? param.Name,
                            fileToUpload.ContentType ?? "application/octet-stream");

                    formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                    // Write the file data directly to the Stream, rather than serializing it to a string.
                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                }
                else
                {
                    var postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                                                    boundary,
                                                    param.Name,
                                                    param.Value);
                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                }
            }

            // Add the end of the request.  Start with a newline
            var footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            var formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            // TODO: Do we need to close this any more? .NET Standard 1.3
            //formDataStream.Close();


            return formData;
        }

        /// <summary>
        /// Submits the request dispose.
        /// </summary>
        /// <param name="summary">Summary.</param>
        /// <param name="httpWebRequest">Http web request.</param>
        /// <typeparam name="TSuccess">The 1st type parameter.</typeparam>
        /// <typeparam name="TError">The 2nd type parameter.</typeparam>
        private static void SubmitRequestDispose<TSuccess, TError>(
            ref RequestResponseSummary<TSuccess, TError> summary,
            HttpWebRequest httpWebRequest)
        {
            var asyncResponse = httpWebRequest.GetResponseAsync();
            // TODO: Should probably change this to suppor true Async behavior.
            var response = asyncResponse.Result;
            try
            {
                ProcessResponse<TSuccess, TError>(
                    summary: ref summary,
                    response: response);
            }
            finally
            {
                if (response != null)
                {
                    response.Dispose();
                }
            }
        }


        /// <summary>
        /// TODO: Could probably consolidate with ExecuteRestMethod if formParameters can be &= or multiparted.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="summary"></param>
        /// <param name="httpWebRequest"></param>
        /// <returns></returns>
        private static void SubmitResponseUsing<TSuccess, TError>(
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

        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <returns>The response.</returns>
        /// <param name="summary">Summary.</param>
        /// <param name="response">Response.</param>
        /// <typeparam name="TSuccess">The 1st type parameter.</typeparam>
        /// <typeparam name="TError">The 2nd type parameter.</typeparam>
        private static RequestResponseSummary<TSuccess, TError> ProcessResponse<TSuccess, TError>(
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
                        summary.Results = JsonConvert.DeserializeObject<TSuccess>(responseText);
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

        private static void BuildPayload<TSuccess, TError>(
            object payload,
            string serializedPayload,
            List<FormParameter> formPameters,
            HttpWebRequest httpWebRequest,
            ref RequestResponseSummary<TSuccess, TError> summary
            )
        {
            if (payload != null)
            {
                serializedPayload = payload.Serialize();
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

        private static void ManageHeaders(
            HttpWebRequest httpWebRequest,
            string method,
            List<Header> headers,
            string username,
            string password,
            string accept,
            string contentType,
            string userAgent,
            CookieContainer cookieContainer
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
                headers.Add(authHeader);
            }

            httpWebRequest.ContentType = contentType;
            httpWebRequest.Method = method;
            if (cookieContainer != null)
            {
                httpWebRequest.CookieContainer = cookieContainer;
            }
            if (!string.IsNullOrEmpty(userAgent))
            {
                try
                {
#if NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_0 || NETCOREAPP2_1
                    httpWebRequest.UserAgent = userAgent;
#else
                    httpWebRequest.Headers["User-Agent"] = userAgent;
#endif
                }
                catch
                {
                    httpWebRequest.Headers["User-Agent"] = userAgent;
                }
                // TODO: Test with .NET Standard 1.3
#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472
                httpWebRequest.UserAgent = userAgent;
#elif NETSTANDARD1_3 || NETSTANDARD1_2
                //httpWebRequest.Headers["User-Agent"] = userAgent;
                httpWebRequest.UserAgent = userAgent;
#elif NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6 || NETSTANDARD2_0
                httpWebRequest.Headers["User-Agent"] = userAgent;
#elif NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_0 || NETCOREAPP2_1
                httpWebRequest.Headers["User-Agent"] = userAgent;
#elif NETSTANDARD1_1 || NETSTANDARD1_0
                // Do Nothing because ???
#else
                httpWebRequest.Headers["User-Agent"] = userAgent;
#endif
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

        private static void HandleWebException<TSuccess, TError>(
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
                            var errorObject = JsonConvert.DeserializeObject<TError>(errorText);
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
            summary.Status = 0;
            summary.UnhandledError = ex.Message;
        }

        private static byte[] GetFormData<TSuccess, TError>(
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
