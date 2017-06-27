using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SlackDotNet.WebApi
{
    /// <summary>
    /// Status related to HTTP service calls.
    /// </summary>
    public enum ExtendedResponseStatus
    {
        /// <summary>
        /// Call to HTTP service succeeded and returned a 2XX status code.
        /// </summary>
        HttpCallSuccess,

        /// <summary>
        /// Call to HTTP service timed out.
        /// </summary>
        HttpTimeout,

        /// <summary>
        /// Call to HTTP service responded with a status code different than 2XX.
        /// </summary>
        HttpError,

        /// <summary>
        /// Call to HTTP service, the service responded but reading the response body failed.
        /// </summary>
        HttpReadContentFailed,

        /// <summary>
        /// Call to HTTP service successfully responded but provided invalid JSON data.
        /// </summary>
        InvalidHttpResponseContent,

        /// <summary>
        /// Call to HTTP service successfully responded with valid JSON data but not matching web API requirements.
        /// </summary>
        InvalidWebApiResponseContent,
    }

    /// <summary>
    /// Provides additional information related to a HTTP service call.
    /// </summary>
    /// <typeparam name="T">Type that matches the API reponse content.</typeparam>
    public struct Response<T> where T : ResponseBase
    {
        /// <summary>
        /// Gets the requested API.
        /// </summary>
        public string RequestedApi { get; }

        /// <summary>
        /// Gets the status of the HTTP service call.
        /// </summary>
        public ExtendedResponseStatus Status { get; }

        /// <summary>
        /// Gets an extra information giving more context.
        /// </summary>
        public object StatusPayload { get; internal set; }

        /// <summary>
        /// Gets the parsed and transformed JSON root object of the HTTP service response.
        /// </summary>
        public T ResponseObject { get; }

        internal Response(string requestedApi, T responseObject)
            : this(requestedApi, ExtendedResponseStatus.HttpCallSuccess, null, responseObject)
        {
        }

        internal Response(string requestedApi, ExtendedResponseStatus status)
            : this(requestedApi, status, null, default(T))
        {
        }

        internal Response(string requestedApi, ExtendedResponseStatus status, object statusPayload)
            : this(requestedApi, status, statusPayload, default(T))
        {
        }

        internal Response(string requestedApi, ExtendedResponseStatus status, object statusPayload, T responseObject)
        {
            RequestedApi = requestedApi;
            Status = status;
            StatusPayload = statusPayload;
            ResponseObject = responseObject;
        }

        internal void LogWarnings(ILogger logger)
        {
            if (ResponseObject?.Warnings != null && ResponseObject.Warnings.Length > 0)
                logger?.LogWarning($"{RequestedApi}: {string.Join(", ", ResponseObject.Warnings)}");
        }
    }

    /// <summary>
    /// Contains common response properties from the Web API.
    /// </summary>
    public class ResponseBase
    {
        /// <summary>
        /// Gets the warnings, if any.
        /// </summary>
        [JsonProperty("warning", Required = Required.DisallowNull), JsonConverter(typeof(WarningsJsonConverter))]
        public string[] Warnings { get; private set; }

        /// <summary>
        /// Gets the error, if any.
        /// </summary>
        [JsonProperty("error", Required = Required.DisallowNull)]
        public string Error { get; private set; }

        /// <summary>
        /// Gets a value indicating whether there is an error or not.
        /// </summary>
        public bool HasError => Error != null;
    }

    /// <summary>
    /// The Team part from the 'rtm.connect' Web API reponse.
    /// </summary>
    public struct ConnectResponseTeamInfo
    {
        /// <summary>
        /// Gets the team identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Identifier { get; private set; }

        /// <summary>
        /// Gets the team name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the team domain.
        /// </summary>
        [JsonProperty("domain")]
        public string Domain { get; private set; }

        /// <summary>
        /// Gets the enterprise identifier.
        /// </summary>
        [JsonProperty("enterprise_id")]
        public string EnterpriseIdentifier { get; private set; }

        /// <summary>
        /// Gets the enterprise name.
        /// </summary>
        [JsonProperty("enterprise_name")]
        public string EnterpriseName { get; private set; }
    }

    /// <summary>
    /// The User part from the 'rtm.connect' Web API reponse.
    /// </summary>
    public struct ConnectResponseUserInfo
    {
        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Identifier { get; private set; }

        /// <summary>
        /// Gets the user name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }
    }

    /// <summary>
    /// The response from 'auth.test' Web API.
    /// </summary>
    public class AuthTestResponse : ResponseBase
    {
        /// <summary>
        /// Gets the team url.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; private set; }

        /// <summary>
        /// Gets the team name.
        /// </summary>
        [JsonProperty("team")]
        public string Team { get; private set; }

        /// <summary>
        /// Gets the user name.
        /// </summary>
        [JsonProperty("user")]
        public string User { get; private set; }

        /// <summary>
        /// Gets the team identifier.
        /// </summary>
        [JsonProperty("team_id")]
        public string TeamIdentifier { get; private set; }

        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        [JsonProperty("user_id")]
        public string UserIdentifier { get; private set; }

        /// <summary>
        /// Gets the enterprise identifier.
        /// </summary>
        [JsonProperty("enterprise_id")]
        public string EnterpriseIdentifier { get; private set; }
    }

    /// <summary>
    /// The response from 'auth.revoke' Web API.
    /// </summary>
    public class AuthRevokeResponse : ResponseBase
    {
        /// <summary>
        /// Gets the team url.
        /// </summary>
        [JsonProperty("revoke")]
        public bool IsRevoked { get; private set; }
    }

    /// <summary>
    /// The response from the 'rtm.connect' Web API.
    /// </summary>
    public class ConnectResponse : ResponseBase
    {
        /// <summary>
        /// Gets teh WebSocket URL to connect to.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; private set; }

        /// <summary>
        /// Gets information about the team.
        /// </summary>
        [JsonProperty("team")]
        public ConnectResponseTeamInfo Team { get; private set; }

        /// <summary>
        /// Gets information about oneself.
        /// </summary>
        [JsonProperty("self")]
        public ConnectResponseUserInfo Self { get; private set; }
    }

    /// <summary>
    /// The response from the 'channel.info' Web API.
    /// </summary>
    public class ChannelInfoResponse : ResponseBase
    {
        /// <summary>
        /// Gets information about the channel.
        /// </summary>
        [JsonProperty("channel")]
        public ChannelInfo Channel { get; private set; }
    }

    /// <summary>
    /// The response from the 'channel.list' Web API.
    /// </summary>
    public class ChannelListResponse : ResponseBase
    {
        /// <summary>
        /// Gets the channels.
        /// </summary>
        [JsonProperty("channels")]
        public ChannelInfo[] Channels { get; private set; }
    }

    /// <summary>
    /// The response from the 'users.list' Web API.
    /// </summary>
    public class UserListResponse : ResponseBase
    {
        /// <summary>
        /// Gets the members.
        /// </summary>
        [JsonProperty("members")]
        public UserInfo[] Members { get; private set; }

        /// <summary>
        /// Gets the cache timestamp.
        /// </summary>
        [JsonProperty("cache_ts"), JsonConverter(typeof(UnixTimestampJsonConverter))]
        public DateTime CacheTimestamp { get; private set; }

        /// <summary>
        /// Gets metadata information.
        /// </summary>
        [JsonProperty("response_metadata")]
        public UserListResponseMetadata Metadata { get; private set; }
    }

    /// <summary>
    /// Metadata information for the <see cref="UserListResponse"/>.
    /// </summary>
    public class UserListResponseMetadata
    {
        /// <summary>
        /// Gets the next cursor to advances the users collection.
        /// </summary>
        [JsonProperty("next_cursor")]
        public string NextCursor { get; private set; }
    }

    /// <summary>
    /// Represents a 32 bits color.
    /// </summary>
    public struct Color
    {
        /// <summary>
        /// Gets the color representing 'good'. Usually green.
        /// </summary>
        public static readonly Color Good = new Color(255, 54, 166, 79);

        /// <summary>
        /// Gets the color representing 'warning'. Usually yellow or orange.
        /// </summary>
        public static readonly Color Warning = new Color(255, 218, 160, 56);

        /// <summary>
        /// Gets the color representing 'danger'. Usually red.
        /// </summary>
        public static readonly Color Danger = new Color(255, 208, 0, 0);

        /// <summary>
        /// Gets the alpha channel value.
        /// </summary>
        public byte Alpha { get; }

        /// <summary>
        /// Gets the red channel value.
        /// </summary>
        public byte Red { get; }

        /// <summary>
        /// Gets the green channel value.
        /// </summary>
        public byte Green { get; }

        /// <summary>
        /// Gets the blue channel value.
        /// </summary>
        public byte Blue { get; }

        /// <summary>
        /// Initializes the <see cref="Color"/> instance. The alpha channel is set to 255.
        /// </summary>
        /// <param name="red">The red channel.</param>
        /// <param name="green">The green channel.</param>
        /// <param name="blue">The blue channel.</param>
        public Color(byte red, byte green, byte blue)
            : this(255, red, green, blue)
        {
        }

        /// <summary>
        /// Initializes the <see cref="Color"/> instance.
        /// </summary>
        /// <param name="alpha">The alpha channel.</param>
        /// <param name="red">The red channel.</param>
        /// <param name="green">The green channel.</param>
        /// <param name="blue">The blue channel.</param>
        public Color(byte alpha, byte red, byte green, byte blue)
        {
            Alpha = alpha;
            Red = red;
            Green = green;
            Blue = blue;
        }

        /// <summary>
        /// Returns the hexadecimal notation that represents the color as a string.
        /// </summary>
        /// <returns>Returns the hexadecimal notation that represents the color as a string.</returns>
        public string ToHexString()
        {
            if (Alpha == 255)
                return $"#{Red:X2}{Green:X2}{Blue:X2}";
            else
                return $"#{Alpha:X2}{Red:X2}{Green:X2}{Blue:X2}";
        }
    }

    /// <summary>
    /// The response from the 'users.list' Web API.
    /// </summary>
    public class UserInfo : ResponseBase
    {
        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Identifier { get; private set; }

        /// <summary>
        /// Gets the team identifier.
        /// </summary>
        [JsonProperty("team_id")]
        public string TeamIdentifier { get; private set; }

        /// <summary>
        /// Gets the user name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the user has been deleted or not.
        /// </summary>
        [JsonProperty("deleted")]
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// Gets the user representative color.
        /// </summary>
        [JsonProperty("color"), JsonConverter(typeof(ColorJsonConverter))]
        public Color Color { get; private set; }

        /// <summary>
        /// Gets the user real name.
        /// </summary>
        [JsonProperty("real_name")]
        public string RealName { get; private set; }

        /// <summary>
        /// Gets the user timezone.
        /// </summary>
        [JsonProperty("tx")]
        public string TimeZone { get; private set; }

        /// <summary>
        /// Gets the user timezone label.
        /// </summary>
        [JsonProperty("tz_label")]
        public string TimeZoneLabel { get; private set; }

        /// <summary>
        /// Gets the user timezone offset.
        /// </summary>
        [JsonProperty("tz_offset")]
        public int TimeZoneOffset { get; private set; }

        /// <summary>
        /// Gets the user profile.
        /// </summary>
        [JsonProperty("profile")]
        public ProfileInfo Profile { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the user is an administrator or not.
        /// </summary>
        [JsonProperty("is_admin")]
        public bool IsAdmin { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the user is the team owner or not.
        /// </summary>
        [JsonProperty("is_owner")]
        public bool IsOwner { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the user is a the team primary owner or not.
        /// </summary>
        [JsonProperty("is_primary_owner")]
        public bool IsPrimaryOwner { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the user is restricted or not.
        /// </summary>
        [JsonProperty("is_restricted")]
        public bool IsRestricted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether ultra restricted or not.
        /// </summary>
        [JsonProperty("is_ultra_restricted")]
        public bool IsUltraRestricted { get; private set; }

        /// <summary>
        /// Gets the user last update date and time.
        /// </summary>
        [JsonProperty("updated"), JsonConverter(typeof(UnixTimestampJsonConverter))]
        public DateTime Updated { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the user has setup its two factor authentication or not.
        /// </summary>
        [JsonProperty("has_2fa")]
        public bool HasTwoFactorAuthentication { get; private set; }

        /// <summary>
        /// Gets the user two factor authentication type.
        /// </summary>
        [JsonProperty("two_factor_type")]
        public string TwoFactorAuthenticationType { get; private set; }
    }

    /// <summary>
    /// Represents a profile information.
    /// </summary>
    public class ProfileInfo
    {
        /// <summary>
        /// Gets the avatar hash.
        /// </summary>
        [JsonProperty("avatar_hash")]
        public string AvatarHash { get; private set; }

        /// <summary>
        /// Gets the profile current status.
        /// </summary>
        [JsonProperty("current_status")]
        public string CurrentStatus { get; private set; }

        /// <summary>
        /// Gets the profile first name.
        /// </summary>
        [JsonProperty("first_name")]
        public string FirstName { get; private set; }

        /// <summary>
        /// Gets the profile last name.
        /// </summary>
        [JsonProperty("last_name")]
        public string LastName { get; private set; }

        /// <summary>
        /// Gets the profile real name.
        /// </summary>
        [JsonProperty("real_name")]
        public string RealName { get; private set; }

        /// <summary>
        /// Gets the profile email.
        /// </summary>
        [JsonProperty("color")]
        public string Email { get; private set; }

        /// <summary>
        /// Gets the profile Skype account name.
        /// </summary>
        [JsonProperty("skype")]
        public string Skype { get; private set; }

        /// <summary>
        /// Gets the profile phone number.
        /// </summary>
        [JsonProperty("phone")]
        public string Phone { get; private set; }

        /// <summary>
        /// Gets the profile 24px by 24px image URL.
        /// </summary>
        [JsonProperty("image_24")]
        public string Image24x24 { get; private set; }

        /// <summary>
        /// Gets the profile 32px by 32px image URL.
        /// </summary>
        [JsonProperty("image_32")]
        public string Image32x32 { get; private set; }

        /// <summary>
        /// Gets the profile 48px by 48px image URL.
        /// </summary>
        [JsonProperty("image_48")]
        public string Image48x48 { get; private set; }

        /// <summary>
        /// Gets the profile 72px by 72px image URL.
        /// </summary>
        [JsonProperty("image_72")]
        public string Image72x72 { get; private set; }

        /// <summary>
        /// Gets the profile 192px by 192px image URL.
        /// </summary>
        [JsonProperty("image_192")]
        public string Image192x192 { get; private set; }

        /// <summary>
        /// Gets the profile 256px by 256px image URL.
        /// </summary>
        [JsonProperty("image_256")]
        public string Image256x256 { get; private set; }
    }

    /// <summary>
    /// The response from the 'chat.postMessage' Web API.
    /// </summary>
    public class PostMessageResponse : ResponseBase
    {
        /// <summary>
        /// Gets the message timestamp.
        /// </summary>
        [JsonProperty("ts")]
        public string Timestamp { get; private set; }

        /// <summary>
        /// Gets the identifier of the channel where the message has been posted.
        /// </summary>
        [JsonProperty("channel")]
        public string Channel { get; private set; }

        /// <summary>
        /// Gets the message information.
        /// </summary>
        [JsonProperty("message")]
        public MessageResponse Message { get; private set; }
    }

    /// <summary>
    /// The Message part from the 'chat.postMessage' Web API reponse.
    /// </summary>
    public class MessageResponse
    {
    }

    /// <summary>
    /// The response from the 'im.list' Web API.
    /// </summary>
    public class IMListResponse : ResponseBase
    {
        /// <summary>
        /// Gets a collection of IM channels.
        /// </summary>
        [JsonProperty("ims")]
        public IMInfo[] DirectMessages { get; private set; }
    }

    /// <summary>
    /// Represents an IM channel.
    /// </summary>
    public class IMInfo
    {
        /// <summary>
        /// Gets the IM channel identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Identifier { get; private set; }

        /// <summary>
        /// TBC
        /// </summary>
        [JsonProperty("is_im")]
        public bool IsDirectMessage { get; private set; }

        /// <summary>
        /// Gets the identifier or the calling user.
        /// </summary>
        [JsonProperty("user")]
        public string UserIdentifier { get; private set; }

        /// <summary>
        /// Gets the date and time when the direct message has been created.
        /// </summary>
        [JsonProperty("created"), JsonConverter(typeof(UnixTimestampJsonConverter))]
        public DateTime Created { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the calling user has been deleted or not.
        /// </summary>
        [JsonProperty("is_user_deleted")]
        public bool IsUserDeleted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the direct message channel is open.
        /// </summary>
        [JsonProperty("is_open")]
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Gets the date and time of the last message the calling user has read.
        /// </summary>
        [JsonProperty("last_read "), JsonConverter(typeof(UnixTimestampJsonConverter))]
        public DateTime LastRead { get; private set; }

        /// <summary>
        /// Gets the number of messages the calling user did not read yet.
        /// </summary>
        [JsonProperty("unread_count"), DefaultValue(-1), JsonConverter(typeof(UnixTimestampJsonConverter))]
        public int UnreadCount { get; private set; }
    }

    /// <summary>
    /// Represents a public channel.
    /// </summary>
    public class ChannelInfo
    {
        /// <summary>
        /// Gets the channel identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Identifier { get; private set; }

        /// <summary>
        /// Gets the channel name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// TBC
        /// </summary>
        [JsonProperty("is_channel")]
        public bool IsChannel { get; private set; }

        /// <summary>
        /// Gets the data and time when the channel has been created.
        /// </summary>
        [JsonProperty("created"), JsonConverter(typeof(UnixTimestampJsonConverter))]
        public DateTime Created { get; private set; }

        /// <summary>
        /// Gets the identifier of the channel creator.
        /// </summary>
        [JsonProperty("creator")]
        public string CreatorIdentifier { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the channel has been archived or not.
        /// </summary>
        [JsonProperty("is_archived")]
        public bool IsArchived { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the channel is general channel or not. In most team, this is the #general channel.
        /// </summary>
        [JsonProperty("is_general")]
        public bool IsGeneral { get; private set; }

        /// <summary>
        /// Gets the collection of users that are members of the channel.
        /// </summary>
        [JsonProperty("members")]
        public string[] Members { get; private set; }

        /// <summary>
        /// Gets the topic information of the channel.
        /// </summary>
        [JsonProperty("topic")]
        public TopicInfo Topic { get; private set; }

        /// <summary>
        /// Gets the purpose information of the channel.
        /// </summary>
        [JsonProperty("purpose")]
        public TopicInfo Purpose { get; private set; }

        /// <summary>
        /// Gets a value indicated whether the calling member is part of the channel.
        /// </summary>
        [JsonProperty("is_member")]
        public bool IsMember { get; private set; }

        /// <summary>
        /// Gets the timestamp of the last read message.
        /// </summary>
        [JsonProperty("last_read")]
        public string LastReadTimestamp { get; private set; }

        /// <summary>
        /// Gets the latest message.
        /// </summary>
        [JsonProperty("lastest")]
        public MessageInfo LatestMessage { get; private set; }

        /// <summary>
        /// Gets the number of messages the calling user has not read yet.
        /// </summary>
        [JsonProperty("unread_count"), DefaultValue(-1)]
        public int UnreadCount { get; private set; }

        /// <summary>
        /// Gets the number of messages that matter the calling user has not read yet. (excluding join/leave/etc...)
        /// </summary>
        [JsonProperty("unread_count_display")]
        public int UnreadCountDisplay { get; private set; }
    }

    /// <summary>
    /// Represents a topic information.
    /// </summary>
    public class TopicInfo
    {
        /// <summary>
        /// Gets the topic value.
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; private set; }

        /// <summary>
        /// Gets the identifier of the creator of the topic.
        /// </summary>
        [JsonProperty("creator")]
        public string CreatorIdentifier { get; private set; }

        /// <summary>
        /// Gets the date and time of the last time the topic has been set.
        /// </summary>
        [JsonProperty("last_set"), JsonConverter(typeof(UnixTimestampJsonConverter))]
        public DateTime LastSet { get; private set; }
    }

    /// <summary>
    /// Message parsing mode.
    /// </summary>
    public enum ParseMode
    {
        /// <summary>
        /// Slack does not perform any processing on the message.
        /// </summary>
        None,
        /// <summary>
        /// TBD
        /// </summary>
        Client,
        /// <summary>
        /// Slack will apply some processing on the message.
        /// </summary>
        Full
    }

    /// <summary>
    /// Represents an outgoing and incoming message.
    /// </summary>
    public class MessageInfo
    {
        /// <summary>
        /// Gets the channel, private group, or IM channel. Can be an encoded ID, or a name.
        /// </summary>
        [JsonProperty("channel")]
        public string ChannelIdentifier { get; private set; }

        /// <summary>
        /// TBD
        /// </summary>
        [JsonProperty("user")]
        public string UserIdentifier { get; private set; }

        /// <summary>
        /// Gets the text of the message. This property is usually set, unless only <see cref="Attachments"/> property is set.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; private set; }

        /// <summary>
        /// Gets a valud indicating whether the message has been posted as the authed user, instead of as a bot.
        /// </summary>
        [JsonProperty("as_user")]
        public bool AsUser { get; private set; }

        /// <summary>
        /// Gets the structured message attachments.
        /// </summary>
        [JsonProperty("attachments", TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<AttachmentInfo> Attachments { get; private set; }

        /// <summary>
        /// Gets the emoji used as the icon for this message. Overrides <see cref="IconUrl"/>.
        /// Is used in conjunction with <see cref="AsUser"/> set to false, otherwise ignored.
        /// </summary>
        [JsonProperty("icon_emoji")]
        public string IconEmoji { get; private set; }

        /// <summary>
        /// Gets the URL to an image used as the icon for this message.
        /// Is used in conjunction with <see cref="AsUser"/> set to false, otherwise ignored.
        /// </summary>
        [JsonProperty("icon_url")]
        public string IconUrl { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to find and link channel names and usernames or not.
        /// </summary>
        [JsonProperty("link_names")]
        public bool LinkNames { get; private set; }

        /// <summary>
        /// Gets a value indicating how messages are treated.
        /// </summary>
        [JsonProperty("parse"), JsonConverter(typeof(ParseModeJsonConverter))]
        public ParseMode ParseMode { get; private set; }

        /// <summary>
        /// Gets a value indicating whether reply should be made visible to everyone in the channel or conversation.
        /// Used in conjunction with <see cref="ThreadTimestamp"/>.
        /// </summary>
        [JsonProperty("reply_broadcast")]
        public bool ReplyBroadcast { get; private set; }

        /// <summary>
        /// Gets another message's timestamp value to make this message a reply. Avoid using a reply's timestamp value; use its parent instead.
        /// </summary>
        [JsonProperty("thread_ts")]
        public string ThreadTimestamp { get; private set; }

        /// <summary>
        /// Gets a value indicating whether unfurling of primarily text-based content is enabled or not.
        /// </summary>
        [JsonProperty("unfurl_links")]
        public bool UnfurlLinks { get; private set; }

        /// <summary>
        /// Gets a value indicating whether unfurling of media content is enabled or not.
        /// </summary>
        [JsonProperty("unfurl_media")]
        public bool UnfurlMedia { get; private set; }

        /// <summary>
        /// Gets your bot's user name. Is used in conjunction with <see cref="AsUser"/> set to false, otherwise ignored.
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; private set; }

        /// <summary>
        /// Gets the message timestamp.
        /// </summary>
        [JsonProperty("ts")]
        public string Timestamp { get; private set; }

        /// <summary>
        /// TBD
        /// </summary>
        [JsonProperty("source_team")]
        public string SourceTeam { get; private set; }

        /// <summary>
        /// TBD
        /// </summary>
        [JsonProperty("team")]
        public string Team { get; private set; }

        private MessageInfo()
        {
        }
    }

    /// <summary>
    /// Represents a structured message attachment.
    /// </summary>
    public class AttachmentInfo
    {
        /// <summary>
        /// Gets the plain-text summary of the attachment.
        /// </summary>
        [JsonProperty("fallback")]
        public string Fallback { get; set; }

        /// <summary>
        /// Gets an optional value that can either be one of good, warning, danger, or any hex color code (eg. #439FE0). This value is used to color the border along the left side of the message attachment.
        /// </summary>
        [JsonProperty("color"), JsonConverter(typeof(ColorJsonConverter))]
        public Color? Color { get; set; }

        /// <summary>
        /// Gets a optional text that appears above the attachment block.
        /// </summary>
        [JsonProperty("pretext")]
        public string PreText { get; set; }

        /// <summary>
        /// Gets a small text used to display the author's name.
        /// </summary>
        [JsonProperty("author_name")]
        public string AuthorName { get; set; }

        /// <summary>
        /// Gets a valid URL that will hyperlink the <see cref="AuthorName"/> text. Will only work if <see cref="AuthorName"/> is present.
        /// </summary>
        [JsonProperty("author_link")]
        public string AuthorLink { get; set; }

        /// <summary>
        /// Gets a valid URL that displays a small 16x16px image to the left of the <see cref="AuthorName"/> text. Will only work if <see cref="AuthorName"/> is present.
        /// </summary>
        [JsonProperty("author_icon")]
        public string AuthorIconUrl { get; set; }

        /// <summary>
        /// Gets a larger, bold text near the top of a message attachment.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets a valid URL that will hyperlink the <see cref="Title"/> text. Will only work if <see cref="Title"/> is present.
        /// </summary>
        [JsonProperty("title_link")]
        public string TitleLink { get; set; }

        /// <summary>
        /// Gets the main text in a message attachment.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets an array of fields. Fields are key/value pairs displayed in a table inside the message attachment.
        /// </summary>
        [JsonProperty("fields", TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<AttachmentFieldInfo> Fields { get; set; }

        /// <summary>
        /// Gets a valid URL to the image file that is displayed inside a message attachment.
        /// </summary>
        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets a valid URL to the image file that is displayed as a thumbnail on the right side of a message attachment.
        /// </summary>
        [JsonProperty("thumb_url")]
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// Gets a brief text that help contextualize and identify an attachment. Limited to 300 characters, and may be truncated further when displayed to users in environments with limited screen real estate.
        /// </summary>
        [JsonProperty("footer")]
        public string Footer { get; set; }

        /// <summary>
        /// Gets the small icon rendered beside the footer text, providing a publicly accessible URL string.
        /// </summary>
        [JsonProperty("footer_icon")]
        public string FooterIconUrl { get; set; }

        /// <summary>
        /// Gets the additional timestamp value as part of the attachment's footer.
        /// </summary>
        [JsonProperty("ts"), JsonConverter(typeof(UnixTimestampJsonConverter))]
        public DateTime? Timestamp { get; set; }

        // for interactive messages, available only with Events API, which is not supported
        //[JsonProperty("callback_id")]
        //public string CallbackId { get; set; }

        // for interactive messages, available only with Events API, which is not supported
        //[JsonProperty("attachment_type")]
        //public string AttachmentType { get; set; }

        // for interactive messages, available only with Events API, which is not supported
        //[JsonProperty("actions", TypeNameHandling = TypeNameHandling.None)]
        //public IEnumerable<AttachmentActionInfo> Actions { get; set; }
    }

    /// <summary>
    /// Represents an attachment field.
    /// </summary>
    public class AttachmentFieldInfo
    {
        /// <summary>
        /// Gets the bold heading above the value text. It cannot contain markup and will be escaped for you.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets the text value of the field. It may contain standard message markup and must be escaped as normal. May be multi-line.
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets an optional flag indicating whether the value is short enough to be displayed side-by-side with other values.
        /// </summary>
        [JsonProperty("short")]
        public bool? IsShort { get; set; }
    }

    // for interactive messages, available only with Events API, which is not supported
    //public class AttachmentActionInfo
    //{
    //    [JsonProperty("name")]
    //    public string Name { get; set; }

    //    [JsonProperty("text")]
    //    public string Text { get; set; }

    //    [JsonProperty("style")]
    //    public string Style { get; set; }

    //    [JsonProperty("type")]
    //    public string Type { get; set; }

    //    [JsonProperty("value")]
    //    public string Value { get; set; }

    //    [JsonProperty("cofirm")]
    //    public AttachmentActionConfirmInfo Confirm { get; set; }
    //}

    // for interactive messages, available only with Events API, which is not supported
    //public class AttachmentActionConfirmInfo
    //{
    //    [JsonProperty("title")]
    //    public string Title { get; set; }

    //    [JsonProperty("text")]
    //    public string Text { get; set; }

    //    [JsonProperty("ok_text")]
    //    public string AcceptText { get; set; }

    //    [JsonProperty("dismiss_text")]
    //    public string DismissText { get; set; }
    //}
}
