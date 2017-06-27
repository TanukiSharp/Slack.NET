using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SlackDotNet.WebApi
{
    /// <summary>
    /// Get information on your team's Slack channels, create or archive channels, invite users, set the topic and purpose, and mark a channel as read.
    /// </summary>
    public class Channels
    {
        private WebApiClient client;

        internal Channels(WebApiClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            this.client = client;
        }

        /// <summary>
        /// This method returns information about a team channel.
        /// To retrieve information on a private channel, use groups.info
        /// </summary>
        /// <param name="channelIdentifier">The channel to get info on.</param>
        /// <returns>Returns information about a team channel.</returns>
        public Task<Response<ChannelInfoResponse>> Info(string channelIdentifier)
        {
            IQueryBuilder query = QueryBuilder.Shared.Clear();

            if (channelIdentifier != null)
                query.Append("channel", channelIdentifier);

            return client.Call<ChannelInfoResponse>("channels.info", query);
        }

        /// <summary>
        /// This method returns a list of all channels in the team.
        /// This includes channels the caller is in, channels they are not currently in, and archived channels but does not include private channels.
        /// The number of (non-deactivated) members in each channel is also returned.
        /// To retrieve a list of private channels, use groups.list.
        /// Having trouble getting a successful response from this method? Try excluding the members list from each channel object using the <paramref name="excludeMembers"/> parameter.
        /// </summary>
        /// <param name="excludeArchived">Exclude archived channels from the list.</param>
        /// <param name="excludeMembers">Exclude the members collection from each channel.</param>
        /// <returns>Returns a list of all channels in the team.</returns>
        public Task<Response<ChannelListResponse>> List(bool excludeArchived = false, bool excludeMembers = false)
        {
            IQueryBuilder query = QueryBuilder.Shared.Clear();

            if (excludeArchived)
                query.Append("exclude_archived", StringConstants.True);

            if (excludeMembers)
                query.Append("exclude_members", StringConstants.True);

            return client.Call<ChannelListResponse>("channels.list", query);
        }
    }
}
