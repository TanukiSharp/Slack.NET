using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SlackDotNet.WebApi
{
    /// <summary>
    /// Represents the Slack HTTP RPC-style methods.
    /// </summary>
    public partial class WebApiClient
    {
        private readonly string accessToken;
        private readonly HttpClient httpClient;

        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;

        /// <summary>
        /// Gets or sets the total amount of time allowed for a HTTP request, in milliseconds.
        /// See documentation of <see cref="CancellationTokenSource(int)"/> for more details.
        /// </summary>
        public int RequestTimeout { get; set; } = Timeout.Infinite;

        /// <summary>
        /// Gets authentication related APIs.
        /// </summary>
        public Auth Auth { get; }

        /// <summary>
        /// Gets channels related APIs.
        /// </summary>
        public Channels Channels { get; }

        /// <summary>
        /// Gets chats related APIs.
        /// </summary>
        public Chat Chat { get; }

        /// <summary>
        /// Gets direct messages related APIs.
        /// </summary>
        public IM IM { get; }

        /// <summary>
        /// Gets reactions related APIs.
        /// </summary>
        public Reactions Reactions { get; }

        /// <summary>
        /// Gets real time messaging related APIs.
        /// </summary>
        public Rtm Rtm { get; }

        /// <summary>
        /// Gets users related APIs.
        /// </summary>
        public Users Users { get; }

        /// <summary>
        /// Initializes the <see cref="WebApiClient"/> instance.
        /// </summary>
        /// <param name="accessToken">The API token accessible from integration settings.</param>
        /// <param name="loggerFactory">A logger factory for inner components to create loggers.</param>
        public WebApiClient(string accessToken, ILoggerFactory loggerFactory = null)
            : this(accessToken, new HttpClientHandler(), loggerFactory)
        {
        }

        /// <summary>
        /// Initializes the <see cref="WebApiClient"/> instance.
        /// </summary>
        /// <param name="accessToken">The API token accessible from integration settings.</param>
        /// <param name="httpMessageHandler">A <see cref="HttpMessageHandler"/> used as HTTP transport.</param>
        /// <param name="loggerFactory">A logger factory for inner components to create loggers.</param>
        public WebApiClient(string accessToken, HttpMessageHandler httpMessageHandler, ILoggerFactory loggerFactory = null)
        {
            if (accessToken == null)
                throw new ArgumentNullException(nameof(accessToken));
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException($"Argument '{nameof(accessToken)}' cannot be an empty string", nameof(accessToken));

            if (httpMessageHandler == null)
                throw new ArgumentNullException(nameof(httpMessageHandler));

            this.accessToken = accessToken;
            this.loggerFactory = loggerFactory;

            logger = loggerFactory?.CreateLogger(nameof(WebApi));

            httpClient = new HttpClient(httpMessageHandler)
            {
                BaseAddress = new Uri("https://slack.com/api/", UriKind.Absolute),
            };

            Auth = new Auth(this);
            Channels = new Channels(this);
            Chat = new Chat(this);
            IM = new IM(this);
            Reactions = new Reactions(this);
            Rtm = new Rtm(this);
            Users = new Users(this);
        }

        private IEnumerable<KeyValuePair<string, string>> ConcatWithToken(IQueryBuilder queryBuilder)
        {
            yield return new KeyValuePair<string, string>("token", accessToken);

            for (int i = 0; i < queryBuilder.Items.Count; i++)
                yield return queryBuilder.Items[i];
        }

        /// <summary>
        /// Slack test API.
        /// </summary>
        /// <typeparam name="T">A specific type of response content.</typeparam>
        /// <param name="parameters">Parameters to pass to the Slack server test API.</param>
        /// <returns>Returns a response object containing the supplied arguments.</returns>
        public Task<Response<T>> Test<T>(params (string key, string value)[] parameters) where T : ResponseBase
        {
            IQueryBuilder builder = QueryBuilder.Shared.Clear();

            foreach (var p in parameters)
                builder.Append(p.key, p.value);

            return Call<T>("api.test", builder);
        }

        internal Task<Response<T>> Call<T>(string api) where T : ResponseBase
        {
            return Call<T>(api, EmptyQueryBuilder.Instance);
        }

        internal async Task<Response<T>> Call<T>(string api, IQueryBuilder queryBuilder) where T : ResponseBase
        {
            if (api == null)
                throw new ArgumentNullException(nameof(api));
            if (queryBuilder == null)
                throw new ArgumentNullException(nameof(queryBuilder));

            var request = new HttpRequestMessage(HttpMethod.Post, api)
            {
                Content = new FormUrlEncodedContent(ConcatWithToken(queryBuilder)),
            };

            HttpResponseMessage response;

            try
            {
                response = await httpClient
                    .SendAsync(
                        request,
                        HttpCompletionOption.ResponseContentRead,
                        RequestTimeout < 0 ? CancellationToken.None : new CancellationTokenSource(RequestTimeout).Token
                    )
                    .ConfigureAwait(false);
            }
            catch (TaskCanceledException tcex)
            {
                return new Response<T>(api, ExtendedResponseStatus.HttpTimeout, tcex);
            }

            if (response.IsSuccessStatusCode == false)
            {
                logger?.LogError(response.ToString());
                return new Response<T>(api, ExtendedResponseStatus.HttpError, response);
            }

            Task<string> readTask = response?.Content?.ReadAsStringAsync();
            if (readTask == null)
            {
                return new Response<T>(
                    api,
                    ExtendedResponseStatus.InvalidHttpResponseContent,
                    "No content available"
                );
            }

            string responseContent;

            try
            {
                responseContent = await readTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new Response<T>(api, ExtendedResponseStatus.HttpReadContentFailed, ex);
            }

            JObject responseRawObject;

            try
            {
                responseRawObject = (JObject)JsonConvert.DeserializeObject(responseContent);
            }
            catch (Exception ex)
            {
                return new Response<T>(
                    api,
                    ExtendedResponseStatus.InvalidHttpResponseContent,
                    (responseContent: responseContent, exception: ex)
                );
            }

            try
            {
                T responseWorkObject = responseRawObject.ToObject<T>();

                var result = new Response<T>(api, ExtendedResponseStatus.HttpCallSuccess, null, responseWorkObject);

                result.LogWarnings(logger);

                return result;
            }
            catch (Exception ex)
            {
                return new Response<T>(api, ExtendedResponseStatus.InvalidWebApiResponseContent, ex);
            }
        }
    }
}
