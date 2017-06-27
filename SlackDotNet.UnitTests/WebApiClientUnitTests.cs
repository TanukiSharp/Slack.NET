using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SlackDotNet.WebApi;
using Xunit;

namespace SlackDotNet.UnitTests
{
    public class WebApiClientUnitTests
    {
        [Fact]
        public void Constructor_AccessToken_ArgumentNullException()
        {
            ArgumentNullException ex = Assert.Throws<ArgumentNullException>("accessToken", () => new WebApiClient(null, null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t \n")]
        public void Constructor_AccessToken_ArgumentException(string accessToken)
        {
            ArgumentException ex = Assert.Throws<ArgumentException>("accessToken", () => new WebApiClient(accessToken, null));
        }

        [Fact]
        public void Constructor_Handler_ArgumentNullException()
        {
            ArgumentNullException ex = Assert.Throws<ArgumentNullException>("httpMessageHandler", () => new WebApiClient("abc", null, null));
        }

        [Fact]
        public void Constructor_CustomHandler()
        {
            new WebApiClient("abc", new HttpClientHandler(), null);
        }

        [Theory]
        [InlineData(-51)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(int.MaxValue)]
        public void CanChangeRequestTimeout(int timeout)
        {
            var x = new WebApiClient("abc", null);
            x.RequestTimeout = timeout;
        }

        [Fact]
        public async Task WrongAccessToken()
        {
            var x = new WebApiClient("abc");
            Response<ChannelInfoResponse> result = await x.Channels.Info("any");
            Assert.Equal(ExtendedResponseStatus.HttpCallSuccess, result.Status);
            Assert.Equal("invalid_auth", result.ResponseObject.Error);
        }

        [Fact]
        public async Task TestOfflineTesting()
        {
            var x = new WebApiClient("abc", new FakeAccessTokenChecker());
            Response<ChannelInfoResponse> result = await x.Channels.Info("any");
            Assert.Equal(ExtendedResponseStatus.HttpCallSuccess, result.Status);
            Assert.Equal("invalid_auth", result.ResponseObject.Error);
        }
    }

    internal class FakeAccessTokenChecker : HttpMessageHandler
    {
        public const string ValidAccessToken = "dfe712d7-8121-4e8e-9a10-f8a041ac06fe";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Task<string> contentResponseTask = request?.Content?.ReadAsStringAsync();
            if (contentResponseTask == null)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            string contentResponse = await contentResponseTask.ConfigureAwait(false);

            if (Utils.GetValueInUrlEncode(contentResponse, "token", out string foundToken) && foundToken == ValidAccessToken)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StringContent("{\"ok\":true}")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new StringContent("{\"error\":\"invalid_auth\"}")
            };
        }
    }
}
