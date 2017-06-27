using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SlackDotNet.WebApi
{
    /// <summary>
    /// Post chat messages to Slack.
    /// </summary>
    public class Chat
    {
        private WebApiClient client;

        internal Chat(WebApiClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            this.client = client;
        }

        private const ParseMode PostMessageApiDefaultParseMode = ParseMode.None;

        /// <summary>
        /// This method posts a message to a public channel, private channel, or direct message/IM channel.
        /// </summary>
        /// <param name="channel">Channel, private group, or IM channel to send message to. Can be an encoded identifier, or a name.</param>
        /// <param name="text">Text of the message to send. This argument is usually required, unless you're providing only <paramref name="attachments"/> instead.</param>
        /// <param name="asUser">Pass true to post the message as the authed user, instead of as a bot. Defaults to false. See authorship on Slack documentation for more details.</param>
        /// <param name="attachments">Structured message attachments.</param>
        /// <param name="iconEmoji">Emoji to use as the icon for this message. Overrides <paramref name="iconUrl"/>. Must be used in conjunction with <paramref name="asUser"/> set to false, otherwise ignored. See authorship on Slack documentation for more details.</param>
        /// <param name="iconUrl">URL to an image to use as the icon for this message. Must be used in conjunction with <paramref name="asUser"/> set to false, otherwise ignored. See authorship on Slack documentation for more details.</param>
        /// <param name="linkNames">Find and link channel names and usernames.</param>
        /// <param name="parse">Change how messages are treated. Defaults to none.</param>
        /// <param name="replyBroadcast">Used in conjunction with <paramref name="threadTimestamp"/> and indicates whether reply should be made visible to everyone in the channel or conversation. Defaults to false.</param>
        /// <param name="threadTimestamp">Provide another message's timestamp value to make this message a reply. Avoid using a reply's timestamp value; use its parent instead.</param>
        /// <param name="unfurlLinks">Pass true to enable unfurling of primarily text-based content.</param>
        /// <param name="unfurlMedia">Pass false to disable unfurling of media content.</param>
        /// <param name="username">Set your bot's user name. Must be used in conjunction with <paramref name="asUser"/> set to false, otherwise ignored. See authorship on Slack documentation for more details.</param>
        /// <returns>The response includes the timestamp and channel for the posted message. It also includes the complete message object, as it was parsed by the Slack servers. This may differ from the provided arguments as the Slack servers sanitize links, attachments and other properties.</returns>
        public Task<Response<PostMessageResponse>> PostMessage(
            string channel,
            string text,
            bool asUser = false,
            AttachmentInfo[] attachments = null,
            string iconEmoji = null,
            string iconUrl = null,
            bool linkNames = false,
            ParseMode parse = PostMessageApiDefaultParseMode,
            bool replyBroadcast = false,
            string threadTimestamp = null,
            bool unfurlLinks = false,
            bool unfurlMedia = true,
            string username = null
        )
        {
            IQueryBuilder query = QueryBuilder.Shared.Clear();

            if (channel != null)
                query.Append("channel", channel);

            if (text != null)
                query.Append("text", text);

            if (asUser)
                query.Append("as_user", StringConstants.True);

            if (attachments != null && attachments.Length > 0)
            {
                string json = JsonConvert.SerializeObject(attachments);
                query.Append("attachments", json);
            }

            if (iconEmoji != null)
                query.Append("icon_emoji", iconEmoji);

            if (iconUrl != null)
                query.Append("icon_url", iconUrl);

            if (linkNames)
                query.Append("link_names", StringConstants.True);

            Utils.AppendParseMode(query, parse, PostMessageApiDefaultParseMode);

            if (replyBroadcast)
                query.Append("reply_broadcast", StringConstants.True);

            if (threadTimestamp != null)
                query.Append("thread_ts", threadTimestamp);

            if (unfurlLinks)
                query.Append("unfurl_links", StringConstants.True);

            if (unfurlMedia == false)
                query.Append("unfurl_media", StringConstants.False);

            if (username != null)
                query.Append("username", username);

            return client.Call<PostMessageResponse>("chat.postMessage", query);
        }

        private const ParseMode UpdateApiDefaultParseMode = ParseMode.Client;

        /// <summary>
        /// This method updates a message in a channel. Though related to chat.postMessage, some parameters of chat.update are handled differently.
        /// </summary>
        /// <param name="channel">Channel containing the message to be updated.</param>
        /// <param name="newText">New text for the message, using the default formatting rules.</param>
        /// <param name="timestamp">Timestamp of the message to be updated.</param>
        /// <param name="asUser">Pass true to update the message as the authed user. Bot users in this context are considered authed users.</param>
        /// <param name="attachments">Structured message attachments.</param>
        /// <param name="linkNames">Find and link channel names and usernames. Defaults to none. This parameter should be used in conjunction with <paramref name="parse"/>. To set <paramref name="linkNames"/> to 1, specify a parse mode of full.</param>
        /// <param name="parse">Change how messages are treated. Defaults to client, unlike chat.postMessage.</param>
        /// <returns>The response includes the text, channel and timestamp properties of the updated message so clients can keep their local copies of the message in sync.</returns>
        public Task<Response<PostMessageResponse>> Update(
            string channel,
            string newText,
            string timestamp,
            bool asUser = false,
            AttachmentInfo[] attachments = null,
            bool linkNames = false,
            ParseMode parse = UpdateApiDefaultParseMode
        )
        {
            IQueryBuilder query = QueryBuilder.Shared.Clear();

            if (channel != null)
                query.Append("channel", channel);

            if (newText != null)
                query.Append("text", newText);

            if (timestamp != null)
                query.Append("ts", timestamp);

            if (asUser)
                query.Append("as_user", StringConstants.True);

            if (attachments != null && attachments.Length > 0)
            {
                string json = JsonConvert.SerializeObject(attachments);
                query.Append("attachments", json);
            }

            if (linkNames)
                query.Append("link_names", StringConstants.True);

            Utils.AppendParseMode(query, parse, UpdateApiDefaultParseMode);

            return client.Call<PostMessageResponse>("chat.update", query);
        }
    }
}
