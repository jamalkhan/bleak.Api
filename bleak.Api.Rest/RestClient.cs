using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace bleak.Api.Rest
{

    public class RestClient : IRestClientAsync
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;

        public RestClient
        (
            HttpClient httpClient = null,
            ISerializer serializer = null,
            IDeserializer deserializer = null,
            ILogger logger = null
        )
        {
            _httpClient = httpClient ?? new HttpClient();
            _serializer = serializer ?? new JsonSerializer();
            _deserializer = deserializer ?? serializer as IDeserializer ?? new JsonSerializer();
            _logger = logger;
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
                string contentType = "application/json",
                CancellationToken cancellationToken = default
        )
            where TSuccess : class
            where TError : class
        {
            _logger?.LogInformation($"Executing REST method: {verb} {uri}");

            if (payload != null && !string.IsNullOrEmpty(serializedPayload) && parameters != null)
                throw new ArgumentOutOfRangeException("payload, serializedPayload, and parameters are mutually exclusive.");

            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var summary = new RestResults<TSuccess, TError>();
            var request = new HttpRequestMessage(new HttpMethod(verb.ToString()), uri);


            try
            {
                ManageHeaders
                (
                    request: request,
                    headers: headers,
                    username: username,
                    password: password,
                    accept: accept,
                    contentType: contentType
                );

                await RenderPayloadAsync
                (
                    request: request,
                    summary: summary,
                    payload: payload,
                    serializedPayload: serializedPayload,
                    formParameters: parameters,
                    contentType: contentType,
                    cancellationToken: cancellationToken,
                    headers: headers
                ).ConfigureAwait(false);

                var response = await _httpClient.SendAsync
                (
                    request: request,
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);

                await ProcessResponseAsync(
                    response: response,
                    summary: summary,
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);

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

        private void ManageHeaders
        (
            HttpRequestMessage request,
            IEnumerable<Header> headers,
            string username,
            string password,
            string accept,
            string contentType
            )
        {
            if (!string.IsNullOrEmpty(username))
            {
                var byteArray = Encoding.UTF8.GetBytes($"{username}:{password}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            if (!string.IsNullOrWhiteSpace(accept))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (!request.Headers.TryAddWithoutValidation(header.Name, header.Value))
                    {
                        Console.WriteLine($"Header {header.Name} not added to request headers, trying content headers.");
                        request.Content?.Headers.TryAddWithoutValidation(header.Name, header.Value);
                    }
                }
            }

            if
            (
                !string.IsNullOrWhiteSpace(contentType) &&
                request.Content != null &&
                request.Content.Headers.ContentType == null
            )
            {
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }

            _logger?.LogTrace($"Request Headers: {string.Join(", ", request.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}");
            if (request.Content != null)
            {
                _logger?.LogTrace($"Content-Type: {request.Content.Headers.ContentType?.MediaType}");
            }
        }

        private async Task RenderPayloadAsync<TSuccess, TError>
        (
            HttpRequestMessage request,
            RestResults<TSuccess, TError> summary,
            object payload,
            string serializedPayload,
            IEnumerable<FormParameter> formParameters,
            string contentType,
            CancellationToken cancellationToken,
            IEnumerable<Header> headers
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
                summary.SerializedRequest = serializedPayload;
                var finalContentType =
                    headers?.FirstOrDefault(h => h.Name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))?.Value
                    ?? contentType
                    ?? "application/json";

                request.Content = new StringContent(serializedPayload, Encoding.UTF8, finalContentType);
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
#if NET8_0_OR_GREATER
                summary.SerializedRequest = await formContent.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
                summary.SerializedRequest = await formContent.ReadAsStringAsync();
#endif
            }
            _logger?.LogTrace($"Serialized Request: {summary.SerializedRequest}");
        }


        private async Task ProcessResponseAsync<TSuccess, TError>
        (
            HttpResponseMessage response,
            RestResults<TSuccess, TError> summary,
            CancellationToken cancellationToken
        )
            where TSuccess : class
            where TError : class
        {
            summary.Status = response.StatusCode;

            // TODO: Later implement a streaming approach for this.
#if NET8_0_OR_GREATER
            string responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
            string responseText = await response.Content.ReadAsStringAsync();
#endif
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
            _logger?.LogTrace($"Status: {summary.Status}, Serialized Response: {summary.SerializedResponse}");
        }

    }
}