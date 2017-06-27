using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using SlackDotNet.WebApi;
using SlackDotNet.RealTimeMessaging;

namespace SlackDotNet.TestApp
{
    /// <summary>
    /// A client for the Slack service with high level features.
    /// </summary>
    public class SlackClient : IDisposable
    {
        private readonly WebApiClient webApiClient;
        private readonly RtmApiClient rtmApiClient;

        private readonly ILogger logger;

        private ConnectResponseTeamInfo teamInfo;
        private ConnectResponseUserInfo selfInfo;

        private readonly List<ChannelInfo> allChannels = new List<ChannelInfo>();
        private readonly List<UserInfo> allUsers = new List<UserInfo>();
        private readonly List<IMInfo> allDirectMessages = new List<IMInfo>();

        /// <summary>
        /// Gets the WebApi client instance to perform HTTP requests.
        /// </summary>
        public WebApiClient WebApi
        {
            get { return webApiClient; }
        }

        /// <summary>
        /// Gets the real time messaging API client.
        /// </summary>
        public RtmApiClient RtmApi
        {
            get { return rtmApiClient; }
        }

        /// <summary>
        /// Initializes the <see cref="SlackClient"/> instance.
        /// </summary>
        /// <param name="accessToken">The API token you get from Slack integration settings page.</param>
        /// <param name="loggerFactory">A logger factory for inner components to create loggers.</param>
        public SlackClient(string accessToken, ILoggerFactory loggerFactory = null)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException($"Argument '{nameof(accessToken)}' must be a non-empty string", nameof(accessToken));

            webApiClient = new WebApiClient(accessToken, loggerFactory);
            rtmApiClient = new RtmApiClient(loggerFactory);

            rtmApiClient.Hello += RtmApiClient_Hello;
            rtmApiClient.Message += RtmApiClient_Message;
            rtmApiClient.ReactionAdded += RtmApiClient_ReactionAdded;

            if (loggerFactory != null)
                logger = loggerFactory.CreateLogger(nameof(SlackClient));
        }

        /// <summary>
        /// Unsubscribes events and clears collections.
        /// </summary>
        public void Dispose()
        {
            if (rtmApiClient.RunState == RtmApiClient.RunningState.Started)
                Stop();

            rtmApiClient.Hello -= RtmApiClient_Hello;
            rtmApiClient.Message -= RtmApiClient_Message;
            rtmApiClient.ReactionAdded -= RtmApiClient_ReactionAdded;

            allChannels.Clear();
            allUsers.Clear();
            allDirectMessages.Clear();
        }

        /// <summary>
        /// Connects to the RTM service and keeps it running, raising events from incomming messages.
        /// </summary>
        /// <returns>Returns true when everything goes right, false otherwise.</returns>
        public async Task<bool> Start(int timeout = 5000)
        {
            // tells the web api service you want to connect to real time messaging service
            Response<ConnectResponse> connectResponse = await webApiClient.Rtm.Connect();

            // check whether HTTP request succeeded
            if (connectResponse.Status != ExtendedResponseStatus.HttpCallSuccess)
                return false;

            // check wether the service answered positively
            if (connectResponse.ResponseObject.HasError)
                return false;

            // connect to the real time messaging service through web socket
            RtmApiResult x = await rtmApiClient.Connect(connectResponse.ResponseObject.Url, timeout);

            // check whether everything is alright
            if (x.ResultType == RtmApiResultType.Success)
            {
                teamInfo = connectResponse.ResponseObject.Team;
                selfInfo = connectResponse.ResponseObject.Self;
            }

            return true;
        }

        private async void RtmApiClient_Hello(object sender, EventArgs e)
        {
            Task<Response<UserListResponse>> usersListResponseTask = webApiClient.Users.List(false);
            Task<Response<ChannelListResponse>> channelsListResonseTask = webApiClient.Channels.List();
            Task<Response<IMListResponse>> imListResponseTask = webApiClient.IM.List();

            await Task.WhenAll(
                usersListResponseTask,
                channelsListResonseTask,
                imListResponseTask 
            );

            Response<UserListResponse> usersResponse = usersListResponseTask.Result;
            if (usersResponse.Status != ExtendedResponseStatus.HttpCallSuccess)
            {
                LogBadReponse(usersResponse);
            }
            else
            {
                if (usersResponse.ResponseObject.HasError == false)
                    allUsers.AddRange(usersResponse.ResponseObject.Members);
            }

            Response<ChannelListResponse> channelsResponse = channelsListResonseTask.Result;
            if (channelsResponse.Status != ExtendedResponseStatus.HttpCallSuccess)
            {
                LogBadReponse(channelsResponse);
            }
            else
            {
                if (channelsResponse.ResponseObject.HasError == false)
                    allChannels.AddRange(channelsResponse.ResponseObject.Channels);
            }

            Response<IMListResponse> imsResponse = imListResponseTask.Result;
            if (imsResponse.Status != ExtendedResponseStatus.HttpCallSuccess)
            {
                LogBadReponse(imsResponse);
            }
            else
            {
                if (imsResponse.ResponseObject.HasError == false)
                    allDirectMessages.AddRange(imsResponse.ResponseObject.DirectMessages);
            }
        }

        private void LogBadReponse<T>(Response<T> response) where T : ResponseBase
        {
            logger?.LogWarning($"Request to '{response.RequestedApi}' failed (status: {response.Status}, payload: {response.StatusPayload})");
        }

        private async void RtmApiClient_Message(object sender, MessageInfo message)
        {
            UserInfo user = FindUserByIdentifier(message.UserIdentifier);
            ChannelInfo channel = FindChannelByIdentifier(message.ChannelIdentifier);
            IMInfo im = FindDirectMessageByIdentifier(message.ChannelIdentifier);

            if (user != null && message.UserIdentifier != selfInfo.Identifier && // <- for me
                message.ThreadTimestamp == null) // <- a channel message, not a thread message, to avoid thread response recursion
            {
                logger?.LogDebug(message.Text);

                if (channel == null && im != null)
                {
                    if (message.Text == "throw")
                        throw new Exception("I have been asked to throw!");
                }

                if (message.Text == "?")
                {
                    await webApiClient.Chat.PostMessage(
                        message.ChannelIdentifier,
                        null,
                        attachments: new AttachmentInfo[] { new AttachmentInfo {
                            Text = "Do you have a question?",
                            //CallbackId = "question_cb",
                            //AttachmentType = "default",
                            //Actions = new AttachmentActionInfo[]
                            //{
                            //    new AttachmentActionInfo
                            //    {
                            //        Text = "Yes I do",
                            //        Name = "Yes",
                            //        Value = "yep",
                            //        Type = "button"
                            //    },
                            //    new AttachmentActionInfo
                            //    {
                            //        Text = "Nope",
                            //        Name = "No",
                            //        Value = "nop",
                            //        Type = "button"
                            //    }
                            //}
                        } }
                    );

                    return;
                }

                await webApiClient.Chat.PostMessage(
                    message.ChannelIdentifier,
                    null,
                    threadTimestamp: message.Timestamp,
                    attachments: new AttachmentInfo[] { new AttachmentInfo { Text = "5", Color = Color.Good } }
                );

                await Task.Delay(1000);

                await webApiClient.Chat.PostMessage(
                    message.ChannelIdentifier,
                    null,
                    threadTimestamp: message.Timestamp,
                    attachments: new AttachmentInfo[] { new AttachmentInfo { Text = "4", Color = Color.Warning } }
                );

                await Task.Delay(1000);

                await webApiClient.Chat.PostMessage(
                    message.ChannelIdentifier,
                    null,
                    threadTimestamp: message.Timestamp,
                    attachments: new AttachmentInfo[] { new AttachmentInfo { Text = "3", Color = Color.Danger } }
                );

                await Task.Delay(1000);

                await webApiClient.Chat.PostMessage(
                    message.ChannelIdentifier,
                    null,
                    threadTimestamp: message.Timestamp,
                    attachments: new AttachmentInfo[] { new AttachmentInfo { Text = "et après paf, la pastèque!", Color = new Color(58, 163, 227) } }
                );
            }
        }

        private void RtmApiClient_ReactionAdded(object sender, ReactionInfo e)
        {
            if (e.Item.Type == ReactionItemType.Message)
            {
                webApiClient.Reactions.Add(e.Reaction, e.Item.Channel, timestamp: e.Item.Timestamp);
            }
        }

        /// <summary>
        /// Disconnects the web socket channel and stops listening for incomming events.
        /// </summary>
        /// <returns>Returns true if it could disconnect properly, false otherwise.
        /// Happens when in wrong state.</returns>
        public Task<bool> Stop()
        {
            return rtmApiClient.Disconnect();
        }

        /// <summary>
        /// Finds a user by its identifier.
        /// </summary>
        /// <param name="identifier">The identifier of the user.</param>
        /// <returns>Returns the user instance if found, null otherwise.</returns>
        public UserInfo FindUserByIdentifier(string identifier)
        {
            foreach (UserInfo user in allUsers)
            {
                if (user.Identifier == identifier)
                    return user;
            }

            return null;
        }

        /// <summary>
        /// Finds a channel by its identifier.
        /// </summary>
        /// <param name="identifier">The identifier of the channel.</param>
        /// <returns>Returns the channel instance if found, null otherwise.</returns>
        public ChannelInfo FindChannelByIdentifier(string identifier)
        {
            foreach (ChannelInfo channel in allChannels)
            {
                if (channel.Identifier == identifier)
                    return channel;
            }

            return null;
        }

        /// <summary>
        /// Finds a direct message by its identifier.
        /// </summary>
        /// <param name="identifier">The identifier of the direct message.</param>
        /// <returns>Returns the direct message instance if found, null otherwise.</returns>
        public IMInfo FindDirectMessageByIdentifier(string identifier)
        {
            foreach (IMInfo im in allDirectMessages)
            {
                if (im.Identifier == identifier)
                    return im;
            }

            return null;
        }
    }
}
