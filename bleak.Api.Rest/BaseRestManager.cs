using System.Net;
using System.Text;

namespace bleak.Api.Rest
{

    public class BaseRestManager
    {
        protected ISerializer _serializer { get; set; }
        protected IDeserializer _deserializer { get; set; }
        protected string _userAgent { get; set; }
        protected CookieContainer cookieContainer { get; set; } = new CookieContainer();

        protected BaseRestManager()
        {

        }
        protected BaseRestManager(ISerializer serializer, IDeserializer deserializer, string userAgent)
        {
            _serializer = serializer;
            _deserializer = deserializer;
            _userAgent = userAgent;
        }

        protected virtual byte[] GetFormData<TSuccess, TError>(
            RestResults<TSuccess, TError> summary,
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