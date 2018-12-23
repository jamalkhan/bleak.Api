//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;

//namespace bleak.Api.Rest
//{
//    public abstract class BaseRestManager
//    {
//        /// <summary>
//        /// Executes the rest method.
//        /// </summary>
//        /// <returns>The rest method.</returns>
//        /// <param name="uri">URI.</param>
//        /// <param name="verb">Verb.</param>
//        /// <param name="userAgent">User agent.</param>
//        /// <param name="payload">Payload.</param>
//        /// <param name="serializedPayload">Serialized payload.</param>
//        /// <param name="parameters">Parameters.</param>
//        /// <param name="headers">Headers.</param>
//        /// <param name="username">Username.</param>
//        /// <param name="password">Password.</param>
//        /// <param name="accept">Accept.</param>
//        /// <param name="cookieContainer">Cookie container.</param>
//        /// <param name="contentType">Content type.</param>
//        /// <typeparam name="TSuccess">The 1st type parameter.</typeparam>
//        /// <typeparam name="TError">The 2nd type parameter.</typeparam>
//        public  virtual RequestResponseSummary<TSuccess, TError> ExecuteRestMethod<TSuccess, TError>(
//            Uri uri,
//            HttpVerbs verb,
//            string userAgent,
//            object payload = null,
//            string serializedPayload = null,
//            List<FormParameter> parameters = null,
//            List<Header> headers = null,
//            string username = null,
//            string password = null,
//            string accept = null,
//            //CookieContainer cookieContainer = null,
//            string contentType = "application/json"

//            )
//        {
//            string url = uri.ToString();
//            string method = verb.ToString();

//            if (!AcceptedMethods.Contains(method))
//                throw new ArgumentException(method + " is not currently supported.", method);

//            if (payload != null && !string.IsNullOrEmpty(serializedPayload) && parameters != null)
//                throw new ArgumentOutOfRangeException("payload, serializedPayload, and pameters are mutually exclusive.");

//            if (string.IsNullOrEmpty(url))
//                throw new ArgumentNullException("url");

//            var summary = new RequestResponseSummary<TSuccess, TError>();
//            try
//            {
//                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

//                ManageHeaders(
//                    httpWebRequest: httpWebRequest,
//                    method: method,
//                    headers: headers,
//                    username: username,
//                    password: password,
//                    accept: accept,
//                    contentType: contentType,
//                    userAgent: userAgent,
//                    cookieContainer: cookieContainer
//                    );

//                BuildPayload(payload, serializedPayload, parameters, httpWebRequest, ref summary);

//                SubmitResponseUsing<TSuccess, TError>(ref summary, httpWebRequest);

//                return summary;
//            }
//            catch (WebException ex)
//            {
//                HandleWebException<TSuccess, TError>(
//                    summary: ref summary,
//                    url: url,
//                    ex: ex);
//                return summary;
//            }
//            catch (Exception ex)
//            {
//                summary.Results = default(TSuccess);
//                summary.UnhandledError = ex.Message;
//                summary.Status = 0;
//                return summary;
//            }
//        }
//        /// <summary>
//        /// Submits the multipart form.
//        /// </summary>
//        /// <returns>The multipart form.</returns>
//        /// <param name="uri">URI.</param>
//        /// <param name="verb">Verb.</param>
//        /// <param name="userAgent">User agent.</param>
//        /// <param name="encoding">Encoding.</param>
//        /// <param name="uploadFile">Upload file.</param>
//        /// <param name="headers">Headers.</param>
//        /// <param name="parameters">Parameters.</param>
//        /// <param name="username">Username.</param>
//        /// <param name="password">Password.</param>
//        /// <param name="accept">Accept.</param>
//        /// <param name="contentType">Content type.</param>
//        /// <param name="cookieContainer">Cookie container.</param>
//        /// <typeparam name="TSuccess">The 1st type parameter.</typeparam>
//        /// <typeparam name="TError">The 2nd type parameter.</typeparam>
//        public static RequestResponseSummary<TSuccess, TError> SubmitMultipartForm<TSuccess, TError>(
//            Uri uri,
//            HttpVerbs verb,
//            string userAgent,
//            Encoding encoding = null,
//            string uploadFile = null,
//            List<Header> headers = null,
//            List<FormParameter> parameters = null,
//            string username = null,
//            string password = null,
//            string accept = null,
//            string contentType = "multipart/form-data",
//            CookieContainer cookieContainer = null
//            )
//        {
//            var method = verb.ToString();
//            var summary = new RequestResponseSummary<TSuccess, TError>();
//            try
//            {
//                if (encoding == null)
//                {
//                    encoding = Encoding.UTF8;
//                }

//                var httpWebRequest = WebRequest.Create(uri) as HttpWebRequest;
//                if (httpWebRequest == null)
//                {
//                    throw new NullReferenceException("request is not a http request");
//                }

//                string formDataBoundary = "--" + DateTime.Now.ToString("ssfff");
//                contentType = string.Format("{0}; boundary={1}", contentType, formDataBoundary);

//                ManageHeaders(
//                    httpWebRequest: httpWebRequest,
//                    method: method,
//                    headers: headers,
//                    username: username,
//                    password: password,
//                    accept: accept,
//                    contentType: contentType,
//                    userAgent: userAgent,
//                    cookieContainer: cookieContainer
//                    );

//                BuildMultiPartPayload(
//                    encoding: encoding,
//                    parameters: parameters,
//                    contentType: contentType,
//                    request: httpWebRequest,
//                    formDataBoundary: formDataBoundary);

//                SubmitRequestDispose<TSuccess, TError>(
//                    summary: ref summary,
//                    httpWebRequest: httpWebRequest);


//                return summary;
//            }
//            catch (WebException ex)
//            {
//                HandleWebException<TSuccess, TError>(
//                    summary: ref summary,
//                    url: uri.ToString(),
//                    ex: ex);
//                return summary;
//            }
//            catch (Exception ex)
//            {
//                summary.SerializedResponse = ex.Message;
//                summary.Status = 0;
//                summary.UnhandledError = ex.Message;
//                return summary;
//            }
//        }
//        /// <summary>
//        /// Builds the multi part payload.
//        /// </summary>
//        /// <param name="encoding">Encoding.</param>
//        /// <param name="parameters">Parameters.</param>
//        /// <param name="contentType">Content type.</param>
//        /// <param name="request">Request.</param>
//        /// <param name="formDataBoundary">Form data boundary.</param>
//        private static void BuildMultiPartPayload(
//            Encoding encoding,
//            List<FormParameter> parameters,
//            string contentType,
//            HttpWebRequest request,
//            string formDataBoundary)
//        {
//            contentType = string.Format("{0}; boundary={1}", contentType, formDataBoundary);

//            var formData = GetMultipartFormData(
//                parameters: parameters,
//                boundary: formDataBoundary,
//                encoding: encoding);

//            // Send the form data to the request.
//            throw new NotImplementedException("Sorry Guys, Still a WIP");
//            //TODO: Uncomment this block:
//            //using (var requestStream = request.GetRequestStream())
//            //{
//            //    requestStream.Write(formData, 0, formData.Length);
//            //}
//        }
//        /// <summary>
//        /// Gets the multipart form data.
//        /// </summary>
//        /// <returns>The multipart form data.</returns>
//        /// <param name="parameters">Parameters.</param>
//        /// <param name="boundary">Boundary.</param>
//        /// <param name="encoding">Encoding.</param>
//        private static byte[] GetMultipartFormData(
//            List<FormParameter> parameters
//            , string boundary
//            , Encoding encoding
//            )
//        {
//            Stream formDataStream = new MemoryStream();
//            var needsClrf = false;

//            foreach (var param in parameters)
//            {
//                // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
//                // Skip it on the first parameter, add it to subsequent parameters.
//                if (needsClrf)
//                    formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

//                needsClrf = true;

//                var value = param.Value as FileParameter;
//                if (value != null)
//                {
//                    var fileToUpload = value;

//                    // Add just the first part of this param, since we will write the file data directly to the Stream
//                    var header =
//                        string.Format(
//                            "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n",
//                            boundary,
//                            param.Name,
//                            fileToUpload.FileName ?? param.Name,
//                            fileToUpload.ContentType ?? "application/octet-stream");

//                    formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

//                    // Write the file data directly to the Stream, rather than serializing it to a string.
//                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
//                }
//                else
//                {
//                    var postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
//                                                    boundary,
//                                                    param.Name,
//                                                    param.Value);
//                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
//                }
//            }

//            // Add the end of the request.  Start with a newline
//            var footer = "\r\n--" + boundary + "--\r\n";
//            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

//            // Dump the Stream into a byte[]
//            formDataStream.Position = 0;
//            var formData = new byte[formDataStream.Length];
//            formDataStream.Read(formData, 0, formData.Length);
//            // TODO: Do we need to close this any more? .NET Standard 1.3
//            //formDataStream.Close();


//            return formData;
//        }

//        /// <summary>
//        /// Submits the request dispose.
//        /// </summary>
//        /// <param name="summary">Summary.</param>
//        /// <param name="httpWebRequest">Http web request.</param>
//        /// <typeparam name="TSuccess">The 1st type parameter.</typeparam>
//        /// <typeparam name="TError">The 2nd type parameter.</typeparam>
//        private static void SubmitRequestDispose<TSuccess, TError>(
//            ref RequestResponseSummary<TSuccess, TError> summary,
//            HttpWebRequest httpWebRequest)
//        {
//            var asyncResponse = httpWebRequest.GetResponseAsync();
//            // TODO: Should probably change this to suppor true Async behavior.
//            var response = asyncResponse.Result;
//            try
//            {
//                ProcessResponse<TSuccess, TError>(
//                    summary: ref summary,
//                    response: response);
//            }
//            finally
//            {
//                if (response != null)
//                {
//                    response.Dispose();
//                }
//            }
//        }


        


//    }
//}
