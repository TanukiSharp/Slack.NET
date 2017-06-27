using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SlackDotNet.WebApi
{
    /// <summary>
    /// Represents an access to Real Time Messaging sessions.
    /// </summary>
    public class Rtm
    {
        private WebApiClient client;

        internal Rtm(WebApiClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            this.client = client;
        }

        /// <summary>
        /// This method begins a Real Time Messaging API session and reserves your application a specific URL with which to connect via websocket.
        /// </summary>
        /// <param name="batchPresenceAware">Group presence change notices as 'presence_change_batch' events when possible. See batching on Slack documentation for more information.</param>
        /// <param name="presenceSub">Only deliver presence events when requested by subscription. See presence subscriptions on Slack documentation for more information.</param>
        /// <returns>This method returns a WebSocket Message Server URL and limited information about the team.</returns>
        public Task<Response<ConnectResponse>> Connect(bool batchPresenceAware = false, bool presenceSub = false)
        {
            IQueryBuilder query = QueryBuilder.Shared.Clear();

            if (batchPresenceAware)
                query.Append("batch_presence_aware", StringConstants.True);

            if (presenceSub)
                query.Append("presence_sub", StringConstants.True);

            return client.Call<ConnectResponse>("rtm.connect", query);
        }
    }
}
