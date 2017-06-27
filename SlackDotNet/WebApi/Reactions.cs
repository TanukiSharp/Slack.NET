using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SlackDotNet.WebApi
{
    /// <summary>
    /// Get information on reactions.
    /// </summary>
    public class Reactions
    {
        private WebApiClient client;

        internal Reactions(WebApiClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            this.client = client;
        }

        /// <summary>
        /// This method adds a reaction (emoji) to an item (file, file comment, channel message, group message, or direct message).
        /// One of file, file_comment, or the combination of channel and timestamp must be specified.
        /// </summary>
        /// <param name="name">Reaction (emoji) name.</param>
        /// <param name="channelIdentifier">Channel where the message to add reaction to was posted.</param>
        /// <param name="fileIdentifier">File to add reaction to.</param>
        /// <param name="fileCommentIdentifier">File comment to add reaction to.</param>
        /// <param name="timestamp">Timestamp of the message to add reaction to.</param>
        /// <returns>Returns whether the call succeeded or not.</returns>
        public Task<Response<ResponseBase>> Add(
            string name,
            string channelIdentifier = null,
            string fileIdentifier = null,
            string fileCommentIdentifier = null,
            string timestamp = null)
        {
            IQueryBuilder query = QueryBuilder.Shared.Clear();

            if (name != null)
                query.Append("name", name);

            if (channelIdentifier != null)
                query.Append("channel", channelIdentifier);

            if (fileIdentifier != null)
                query.Append("file", fileIdentifier);

            if (fileCommentIdentifier != null)
                query.Append("file_comment", fileCommentIdentifier);

            if (timestamp != null)
                query.Append("timestamp", timestamp);

            return client.Call<ResponseBase>("reactions.add", query);
        }
    }
}
