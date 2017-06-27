using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SlackDotNet.RealTimeMessaging;
using SlackDotNet.WebApi;
using Xunit;

namespace SlackDotNet.UnitTests
{
    public class RtmApiClientUnitTests
    {
        [Theory]
        [InlineData(-51)]
        [InlineData(-1)]
        [InlineData(0)]
        public void Wrong_WebSocketReadBufferSize(int size)
        {
            ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var x = new RtmApiClient();
                x.WebSocketReadBufferSize = size;
            });
        }

        [Theory]
        [InlineData(1)]
        [InlineData(37)]
        public void Valid_WebSocketReadBufferSize(int size)
        {
            var x = new RtmApiClient();
            x.WebSocketReadBufferSize = size;
        }

        [Fact]
        public void RunState()
        {
            var x = new RtmApiClient();
            Assert.Equal(RtmApiClient.RunningState.Stopped, x.RunState);
        }

        [Fact]
        public async Task Disconnect()
        {
            var x = new RtmApiClient();
            Assert.Equal(false, await x.Disconnect());
        }
    }
}
