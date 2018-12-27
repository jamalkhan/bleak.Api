//using System;
//using System.Net;
//using Newtonsoft.Json;
//using System.Collections.Generic;


//namespace bleak.Api.Rest
//{
//    public static class RestExtensionMethods
//    {
//        // TODO: Figure out how to parse the query string in .NET Standard 1.3 or below
//        /*
//        public static void AddQueryParameter(this UriBuilder builder, string key, string value)
//        {
//            if (string.IsNullOrEmpty(key))
//            {
//                return;
//            }
//            var query = HttpUtility.ParseQueryString(builder.Query);
//            query[key] = value;
//            builder.Query = query.ToString();
//        }

//        public static SortedDictionary<string, string> GetQueryParameters(this UriBuilder builder)
//        {
//            var col = System.Web.HttpUtility.ParseQueryString(builder.Uri.Query);
//            var sortedDict = new SortedDictionary<string, string>(col.AllKeys.ToDictionary(k => k, k => col[k]));
//            return sortedDict;
//        }
//        */

//        public static string UrlEncode(this string stringToUrlEncode)
//        {
//            return WebUtility.UrlEncode(stringToUrlEncode);
//        }
//        public static string UrlDecode(this string urlEncodedString)
//        {
//            return WebUtility.UrlDecode(urlEncodedString);
//        }

//        public static string Serialize(this object payload)
//        {
//            string retval;
//            JsonSerializerSettings settings = new JsonSerializerSettings();
//            settings.NullValueHandling = NullValueHandling.Ignore;
//            retval = JsonConvert.SerializeObject(payload,
//                Formatting.None,
//                settings);
//            return retval;
//        }

//        /*
//         * TODO: Add this logging behavior to a higher stack
//        public static void Trace(this Logger logger, HttpWebRequest request, byte[] formData = null)
//        {
//            var sb = new StringBuilder();
//            try
//            {
//                sb.AppendFormat("A HttpWebRequest to {0} was executed", request.RequestUri);
//                sb.AppendFormat("{0} {1}", request.Method, request.RequestUri);
//                sb.AppendFormat("ContentType = {0}", request.ContentType);
//                sb.AppendFormat("UserAgent = {0}", request.UserAgent);
//                sb.AppendFormat("ContentLength = {0}", request.ContentLength);
//                if (formData != null)
//                {
//                    sb.AppendFormat("FormData = {0}", Encoding.UTF8.GetString(formData));
//                }

//                int i = 0;

//                if (request.CookieContainer != null)
//                {
//                    foreach (var cookie in request.CookieContainer.GetCookies(request.RequestUri.GetLeftPart(System.UriPartial.Authority) + "/"))
//                    {
//                        sb.AppendFormat("Cookie[{0}].Name = {1} ; Cookie[{0}].Value = {2} ; Cookie[{0}].Domain = {3}", i++, cookie.Name, cookie.Value, cookie.Domain);
//                    }
//                }

//                logger.Trace(sb.ToString());
//            }
//            catch (System.Exception) { }
//        }
//*/
//        public static List<Cookie> GetCookies(this CookieContainer cookieContainers, string baseUri)
//        {
//            var retval = new List<Cookie>();
//            var cookies = cookieContainers.GetCookies(new Uri(baseUri));
//            foreach (var cookie in cookies)
//            {
//                retval.Add((Cookie)cookie);
//            }
//            return retval;
//        }

//        /// <summary>
//        /// Temporary Solution. Will be replaced by .NET 4.6 later this year.
//        /// </summary>
//        /// <param name="d"></param>
//        /// <returns></returns>
//        public static long ToUnixTimeSeconds(this DateTimeOffset d)
//        {
//            var epoch = d - new DateTimeOffset(1970, 1, 1, 0, 0, 0, d.Offset);
//            return (long)epoch.TotalSeconds;
//        }

//        /// <summary>
//        /// Temporary Solution. Will be replaced by .NET 4.6 later this year.
//        /// </summary>
//        /// <param name="d"></param>
//        /// <returns></returns>
//        public static long ToUnixTimeMilliseconds(this DateTimeOffset d)
//        {
//            return d.ToUnixTimeSeconds() * 1000;
//        }
//    }
//}
