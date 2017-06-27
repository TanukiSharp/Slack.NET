using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SlackDotNet.WebApi;

namespace SlackDotNet.RealTimeMessaging
{
    /// <summary>
    /// WebSocket-based API that allows to receive events from Slack in real time.
    /// </summary>
    public class RtmApiClient
    {
        private readonly ILogger logger;

        private ClientWebSocket webSocketClient;

        private TaskCompletionSource<object> runTaskCompletionSource;
        private CancellationTokenSource runCancellationTokenSource;

        /// <summary>
        /// Initializes the <see cref="RtmApiClient"/> instance.
        /// </summary>
        /// <param name="loggerFactory">A logger factory for inner components to create loggers.</param>
        public RtmApiClient(ILoggerFactory loggerFactory = null)
        {
            logger = loggerFactory?.CreateLogger(nameof(RtmApiClient));
        }

        private int webSocketReadBufferSize = 4096;

        /// <summary>
        /// Gets or sets the WebSocket read buffer size, in bytes.
        /// Can be set only when the <see cref="RunState"/> property value is equal to <see cref="RunningState.Stopped"/>.
        /// </summary>
        public int WebSocketReadBufferSize
        {
            get { return webSocketReadBufferSize; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), $"The value set to '{nameof(WebSocketReadBufferSize)}' must be strictly greater than zero.");

                if (RunState != RunningState.Stopped)
                    throw new InvalidOperationException($"Invalid '{nameof(RunState)}' value. Currently '{RunState}' but only {RunningState.Stopped} is allowed.");

                if (webSocketReadBufferSize == value)
                    return;

                logger?.LogTrace($"{nameof(WebSocketReadBufferSize)} changed: {webSocketReadBufferSize} -> {value}");

                webSocketReadBufferSize = value;
            }
        }

        /// <summary>
        /// Represents the evolution of a running status.
        /// </summary>
        public enum RunningState
        {
            /// <summary>
            /// The process is starting, not yet running.
            /// </summary>
            Starting,

            /// <summary>
            /// The process is started and running.
            /// </summary>
            Started,

            /// <summary>
            /// The process is stopping, not running anymore.
            /// </summary>
            Stopping,

            /// <summary>
            /// The process is stopped and not running.
            /// </summary>
            Stopped
        }

        private readonly object runStateLock = new object();
        private RunningState runState = RunningState.Stopped;

        /// <summary>
        /// Gets the running state of the web socket connectivity process.
        /// </summary>
        public RunningState RunState
        {
            get { return runState; }
            private set
            {
                if (runState == value)
                    return;

                logger?.LogTrace($"{nameof(RunState)} changed: {runState} -> {value}");

                runState = value;
            }
        }

        /// <summary>
        /// Connect to the RTM service through web socket.
        /// </summary>
        /// <param name="websocketUri">The web socket URL or the real time messaging service to connect to.</param>
        /// <param name="timeout">A timeout for the connection, in milliseconds. See documentation of <see cref="CancellationTokenSource(int)"/> for more details.</param>
        /// <returns>Returns the response of the RTM service connect API request.</returns>
        public Task<RtmApiResult> Connect(string websocketUri, int timeout = 5000)
        {
            return Connect(new Uri(websocketUri, UriKind.Absolute), timeout);
        }

        /// <summary>
        /// Connect to the RTM service through web socket.
        /// </summary>
        /// <param name="websocketUri">The web socket URL or the real time messaging service to connect to.</param>
        /// <param name="timeout">A timeout for the connection, in milliseconds. See documentation of <see cref="CancellationTokenSource(int)"/> for more details.</param>
        /// <returns>Returns the response of the RTM service connect API request.</returns>
        public async Task<RtmApiResult> Connect(Uri websocketUri, int timeout = 5000)
        {
            if (websocketUri == null)
                throw new ArgumentNullException(nameof(websocketUri));
            if (timeout < Timeout.Infinite)
                throw new ArgumentOutOfRangeException(nameof(timeout), $"The '{nameof(timeout)}' argument must be greater than or equal to {Timeout.Infinite}.");

            lock (runStateLock)
            {
                if (RunState != RunningState.Stopped)
                    return new RtmApiResult(RtmApiResultType.InvalidRunningState, RunState);

                RunState = RunningState.Starting;
            }

            webSocketClient = new ClientWebSocket();
            runCancellationTokenSource = new CancellationTokenSource();

            var connectCancellationTokenSource = new CancellationTokenSource(timeout);

            try
            {
                await webSocketClient
                    .ConnectAsync(websocketUri, connectCancellationTokenSource.Token)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                CleanupConnect();

                if (ex is OperationCanceledException)
                {
                    logger?.LogError($"Connection to '{websocketUri}' timeout");
                    return new RtmApiResult(RtmApiResultType.WebSocketConnectTimeout, ex);
                }

                throw;
            }

            runTaskCompletionSource = new TaskCompletionSource<object>();

            try
            {
                StartRunLoop();
                RunState = RunningState.Started;
            }
            catch
            {
                CleanupConnect();
                throw;
            }

            return new RtmApiResult(RtmApiResultType.Success, null);
        }

        private void CleanupConnect()
        {
            webSocketClient = null;
            runCancellationTokenSource = null;
            runTaskCompletionSource = null;
            RunState = RunningState.Stopped;
        }

        private async void StartRunLoop()
        {
            var receiveBuffer = new byte[webSocketReadBufferSize];
            MemoryStream memoryStream = null;

            try
            {
                while (runCancellationTokenSource.IsCancellationRequested == false)
                {
                    WebSocketReceiveResult receiveResult;

                    var buffer = new ArraySegment<byte>(receiveBuffer);

                    try
                    {
                        receiveResult = await webSocketClient
                            .ReceiveAsync(buffer, runCancellationTokenSource.Token)
                            .ConfigureAwait(false);
                    }
                    catch (WebSocketException wsex) when (wsex.WebSocketErrorCode == WebSocketError.InvalidState)
                    {
                        logger?.LogTrace("socket closed");
                        Closed?.Invoke(this, new CloseEventArgs(CloseReason.Exception, wsex));
                        break;
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }

                    // TODO: what about AggregateException, ObjectDisposedException, etc... ?

                    if (receiveResult == null)
                        throw new InvalidOperationException($"{nameof(ClientWebSocket)}.{nameof(ClientWebSocket.ReceiveAsync)} returned null.");

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        Closed?.Invoke(this, new CloseEventArgs(CloseReason.CloseMessageFromServer, null));
                        break;
                    }

                    if (receiveResult.EndOfMessage == false)
                    {
                        if (memoryStream == null)
                            memoryStream = new MemoryStream();

                        memoryStream.Write(buffer.Array, buffer.Offset, receiveResult.Count);

                        continue;
                    }
                    else
                    {
                        if (memoryStream != null)
                        {
                            memoryStream.Write(buffer.Array, buffer.Offset, receiveResult.Count);

                            buffer = new ArraySegment<byte>(memoryStream.ToArray());

                            memoryStream.Dispose();
                            memoryStream = null;
                        }
                        else
                            buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset, receiveResult.Count);
                    }

                    if (receiveResult.MessageType == WebSocketMessageType.Text)
                    {
                        OnTextMessage(Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count));
                    }
                    else if (receiveResult.MessageType == WebSocketMessageType.Binary)
                    {
                        OnBinaryMessage(buffer.Array, buffer.Offset, buffer.Count);
                    }
                }

                if (runCancellationTokenSource.IsCancellationRequested)
                    Closed?.Invoke(this, new CloseEventArgs(CloseReason.UserRequested, null));
            }
            finally
            {
                runCancellationTokenSource = null;

                webSocketClient.Dispose();
                webSocketClient = null;

                runTaskCompletionSource.TrySetResult(default(object));

                lock (runStateLock)
                    RunState = RunningState.Stopped;
            }
        }

        /// <summary>
        /// Raised when the web socket channel is closed.
        /// </summary>
        public event EventHandler<CloseEventArgs> Closed;

        /// <summary>
        /// Raised for each incomming messages. This event is raised before any specific event.
        /// </summary>
        public event EventHandler<RawIncomingMessageInfo> RawIncomingMessage;

        /// <summary>
        /// Raised then the 'hello' message is received, right after the web socket connection has been established.
        /// </summary>
        public event EventHandler<EventArgs> Hello;

        /// <summary>
        /// Raised when a chat message is sent.
        /// </summary>
        public event EventHandler<MessageInfo> Message;

        /// <summary>
        /// Raised when a reaction is added to a message.
        /// </summary>
        public event EventHandler<ReactionInfo> ReactionAdded;

        private void ParseResponseAndRaiseEvent<T>(string message, EventHandler<T> handler)
        {
            var x = JsonConvert.DeserializeObject<T>(message);
            handler?.Invoke(this, x);
        }

        private void OnTextMessage(string message)
        {
            string type = GetObjectType(message);

            logger?.LogTrace($"type: '{type?.ToString() ?? "(null)"}'");
            logger?.LogTrace(message);

            RawIncomingMessage?.Invoke(this, new RawIncomingMessageInfo(type, message));

            switch (type)
            {
                case "hello": Hello?.Invoke(this, EventArgs.Empty); break;
                case "message": ParseResponseAndRaiseEvent(message, Message); break;
                case "reaction_added": ParseResponseAndRaiseEvent(message, ReactionAdded); break;
                default:
                    logger?.LogWarning($"message of type '{type?.ToString() ?? "(null)"}' not supported yet.");
                    break;
            }

            logger?.LogTrace("-----------------------------------------------------------------");
        }

        private void OnBinaryMessage(byte[] array, int offset, int count)
        {
            logger?.LogTrace($"--- binary message, {count} bytes long ---");
        }

        /// <summary>
        /// Requests a disconnection of the web socket channel.
        /// </summary>
        /// <returns>Returns true if disconnection could be done properly, false otherwise.
        /// Happens when in the wrong state.</returns>
        public async Task<bool> Disconnect()
        {
            if (RunState != RunningState.Started)
                return false;

            lock (runStateLock)
            {
                if (RunState != RunningState.Started)
                    return false;

                RunState = RunningState.Stopping;
            }

            runCancellationTokenSource.Cancel();

            try
            {
                await runTaskCompletionSource.Task.ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                runTaskCompletionSource = null;
            }

            return true;
        }

        private static string GetObjectType(string message)
        {
            var reader = new JsonTextReader(new StringReader(message));

            bool read = reader.Read();
            if (read == false)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                return null;

            read = reader.Read();
            if (read == false)
                return null;

            if (reader.TokenType != JsonToken.PropertyName || reader.Value as string != "type")
                return null;

            read = reader.Read();
            if (read == false)
                return null;

            if (reader.TokenType != JsonToken.String)
                return null;

            return reader.Value as string;
        }
    }
}
