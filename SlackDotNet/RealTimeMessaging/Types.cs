using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SlackDotNet.RealTimeMessaging
{
    /// <summary>
    /// Represent a detailed reason why the web socket has been closed.
    /// </summary>
    public enum CloseReason
    {
        /// <summary>
        /// The server sent a 'close' web socket message for client to close the socket cleanly.
        /// </summary>
        CloseMessageFromServer,

        /// <summary>
        /// The client closed the socket.
        /// </summary>
        UserRequested,

        /// <summary>
        /// An error occured that caused the socket to be closed.
        /// </summary>
        Exception
    }

    /// <summary>
    /// Provides additional information when the RTM web socket channel is closed.
    /// </summary>
    public class CloseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the reason that closed the web socket.
        /// </summary>
        public CloseReason Reason { get; }

        /// <summary>
        /// Gets the exception that made the web socket to be closed.
        /// </summary>
        public Exception Exception { get; }

        internal CloseEventArgs(CloseReason reason, Exception exception)
        {
            Reason = reason;
            Exception = exception;
        }
    }

    /// <summary>
    /// Represents a real time messaging raw incoming message.
    /// </summary>
    public struct RawIncomingMessageInfo
    {
        /// <summary>
        /// Gets the type of the incoming message. It may be null if it could not be determined.
        /// </summary>
        public readonly string Type;

        /// <summary>
        /// Gets the full data of the incoming message.
        /// </summary>
        public readonly string FullMessage;

        internal RawIncomingMessageInfo(string type, string fullMessage)
        {
            Type = type;
            FullMessage = fullMessage;
        }
    }

    /// <summary>
    /// A high level information for RTM service responses.
    /// </summary>
    public enum RtmApiResultType
    {
        /// <summary>
        /// The request succeeded.
        /// </summary>
        Success,

        /// <summary>
        /// The <see cref="RtmApiClient"/> is not in a correct state to perform requests.
        /// </summary>
        InvalidRunningState,

        /// <summary>
        /// The RTM service request has timed out.
        /// </summary>
        WebSocketConnectTimeout,
    }

    /// <summary>
    /// Represent a <see cref="RtmApiResultType"/> with an additional piece of information.
    /// </summary>
    public struct RtmApiResult
    {
        /// <summary>
        /// Gets the high level information of RTM service response.
        /// </summary>
        public RtmApiResultType ResultType { get; }

        /// <summary>
        /// Gets a case-dependent additional information. Usually set on error and null on success.
        /// </summary>
        public object Payload { get; }

        /// <summary>
        /// Initializes the <see cref="RtmApiResult"/> instance.
        /// </summary>
        /// <param name="resultType">The high level information of RTM service response.</param>
        /// <param name="payload">A case-dependent additional information. Usually set on error and null on success.</param>
        internal RtmApiResult(RtmApiResultType resultType, object payload)
        {
            ResultType = resultType;
            Payload = payload;
        }
    }

    /// <summary>
    /// Information of a reaction.
    /// </summary>
    public class ReactionInfo
    {
        /// <summary>
        /// Gets the identifier of the user who performed the reaction event.
        /// </summary>
        [JsonProperty("user")]
        public string User { get; set; }

        /// <summary>
        /// Gets the reaction name.
        /// </summary>
        [JsonProperty("reaction")]
        public string Reaction { get; set; }

        /// <summary>
        /// Gets the identifier of the user that created the original item that has been reacted to.
        /// </summary>
        [JsonProperty("item_user")]
        public string ItemUser { get; set; }

        /// <summary>
        /// Gets additional information about the reaction.
        /// </summary>
        [JsonProperty("item")]
        public ReactionItemInfo Item { get; set; }

        /// <summary>
        /// Gets the reaction timestamp.
        /// </summary>
        [JsonProperty("event_ts")]
        public string Timestamp { get; set; }
    }

    /// <summary>
    /// Represents the specific type of reaction.
    /// </summary>
    public enum ReactionItemType
    {
        /// <summary>
        /// Reaction on a message.
        /// </summary>
        Message,

        /// <summary>
        /// Reaction on a file.
        /// </summary>
        File,

        /// <summary>
        /// Reaction on the comment of a file.
        /// </summary>
        FileComment
    }

    /// <summary>
    /// Represents either a message, file or file comment reaction information.
    /// </summary>
    public class ReactionItemInfo
    {
        /// <summary>
        /// Gets the type of event on which there reaction happened.
        /// </summary>
        [JsonProperty("type"), JsonConverter(typeof(ReactionItemTypeJsonConverter))]
        public ReactionItemType Type { get; set; }

        /// <summary>
        /// Gets the channel where the reaction event happened.
        /// </summary>
        [JsonProperty("channel")]
        public string Channel { get; set; }

        /// <summary>
        /// Gets the timestamp of the message on which the reaction happened.
        /// </summary>
        [JsonProperty("ts")]
        public string Timestamp { get; set; }

        /// <summary>
        /// Gets the file identifier on which the reaction event happened.
        /// </summary>
        [JsonProperty("file")]
        public string File { get; set; }

        /// <summary>
        /// Gets the file comment identifier on which the reaction event happened.
        /// </summary>
        [JsonProperty("file_comment")]
        public string FileComment { get; set; }
    }
}
