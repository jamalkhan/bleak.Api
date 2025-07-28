using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace bleak.Api.Rest
{

    public class RestClient : IRestManagerAsync
    {
        private readonly HttpClient _httpClient;
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;

        public RestClient(HttpClient httpClient = null,
        ISerializer serializer = null,
         IDeserializer deserializer = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _serializer = serializer ?? new JsonSerializer();
            _deserializer = deserializer ?? new JsonSerializer();
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
            where TSuccess : class
            where TError : class
        {
            if (payload != null && !string.IsNullOrEmpty(serializedPayload) && parameters != null)
                throw new ArgumentOutOfRangeException("payload, serializedPayload, and parameters are mutually exclusive.");

            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var summary = new RestResults<TSuccess, TError>();
            var request = new HttpRequestMessage(new HttpMethod(verb.ToString()), uri);


            try
            {
                ManageHeaders(request, headers, username, password, accept, contentType);

                await RenderPayloadAsync(request, summary, payload, serializedPayload, parameters, contentType);

                var response = await _httpClient.SendAsync(request);

                await ProcessResponseAsync(response, summary);

                return summary;
            }
            catch (HttpRequestException ex)
            {
                summary.UnhandledError = ex.Message;
                summary.Status = 0;
                return summary;
            }
            catch (Exception ex)
            {
                summary.UnhandledError = ex.Message;
                summary.Status = 0;
                return summary;
            }
        }

        private void ManageHeaders(
            HttpRequestMessage request,
            IEnumerable<Header> headers,
            string username,
            string password,
            string accept,
            string contentType)
        {
            if (!string.IsNullOrEmpty(username))
            {
                var byteArray = Encoding.UTF8.GetBytes($"{username}:{password}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            if (!string.IsNullOrWhiteSpace(accept))
            {
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(accept));
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Name, header.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(contentType) &&
                request.Content != null &&
                request.Content.Headers.ContentType == null)
            {
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            }
        }

        private async Task RenderPayloadAsync<TSuccess, TError>(
            HttpRequestMessage request,
            RestResults<TSuccess, TError> summary,
            object payload,
            string serializedPayload,
            IEnumerable<FormParameter> formParameters,
            string contentType)
            where TSuccess : class
            where TError : class
        {
            if (payload != null)
                serializedPayload = _serializer.Serialize(payload);

            if (!string.IsNullOrEmpty(serializedPayload))
            {
                summary.SerializedRequest = serializedPayload;
                request.Content = new StringContent(serializedPayload, Encoding.UTF8, contentType);
            }
            else if (formParameters != null && formParameters.Any())
            {
                // TODO: May need to convert to non-linq select
                var formContent = new FormUrlEncodedContent(
                    formParameters
                        .Where(p => p.Name != null && p.Value != null)
                        .Select(p => new KeyValuePair<string, string>(p.Name, p.Value.ToString()))
                );
                request.Content = formContent;
                summary.SerializedRequest = await formContent.ReadAsStringAsync();
            }
        }
        
        
        private async Task ProcessResponseAsync<TSuccess, TError>(
            HttpResponseMessage response,
            RestResults<TSuccess, TError> summary)
            where TSuccess : class
            where TError : class
        {
            summary.Status = response.StatusCode;

            string responseText = await response.Content.ReadAsStringAsync();
            summary.SerializedResponse = responseText;

            if (response.IsSuccessStatusCode)
            {
                if (string.IsNullOrWhiteSpace(responseText))
                {
                    summary.Results = default;
                    return;
                }

                try
                {
                    summary.Results = typeof(TSuccess).Name == "String"
                        ? (TSuccess)(object)responseText.Trim()
                        : _deserializer.Deserialize<TSuccess>(responseText);
                }
                catch (Exception ex)
                {
                    summary.UnhandledError = ex.Message;
                }
            }
            else
            {
                try
                {
                    summary.Error = typeof(TError).Name == "String"
                        ? (TError)(object)responseText.Trim()
                        : _deserializer.Deserialize<TError>(responseText);
                }
                catch (Exception ex)
                {
                    summary.UnhandledError = ex.Message;
                }
            }
        }

    }
}