using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace UwpNetworkingEssentials.Channels.StreamSockets
{
    /// <summary>
    /// A message-based abstraction on top of the <see cref="StreamSocket"/> API.
    /// </summary>
    public class StreamSocketConnection : ConnectionBase
    {
        private readonly StreamSocket _socket;
        private readonly IObjectSerializer _serializer;
        private readonly DataReader _reader;
        private readonly DataWriter _writer;
        private readonly Task _receiverTask;
        private readonly CancellationTokenSource _receiverTaskCancellationTokenSource = new CancellationTokenSource();
        private readonly Dictionary<int, TaskCompletionSource<object>> _pendingRequests =
            new Dictionary<int, TaskCompletionSource<object>>();
        private DisconnectReason _disconnectReason = DisconnectReason.Unknown;
        private int _nextRequestId = 0;

        public StreamSocketInformation Information => _socket.Information;

        private StreamSocketConnection(string id, StreamSocket socket, IObjectSerializer serializer)
            : base(id)
        {
            _socket = socket;
            _serializer = serializer;
            _reader = new DataReader(socket.InputStream);
            _writer = new DataWriter(socket.OutputStream);
            _receiverTask = ReceiveAsync();
        }

        public static async Task<StreamSocketConnection> AcceptConnectionAsync(StreamSocket socket,
            IObjectSerializer serializer)
        {
            // Receive connection request, send connection response
            using (var reader = new DataReader(socket.InputStream))
            using (var writer = new DataWriter(socket.OutputStream))
            {
                var request = await serializer
                    .DeserializeFromStreamAsync(reader, CancellationToken.None)
                    .ContinueOnOtherContext() as ConnectionRequestMessage;

                if (request == null)
                {
                    // Wrong message received => reject connection
                    await serializer
                        .SerializeToStreamAsync(new ConnectionResponseMessage(null), writer)
                        .ContinueOnOtherContext();

                    socket.Dispose();
                    return null;
                }
                else
                {
                    // Accept connection request
                    var connectionId = "SSC_" + Guid.NewGuid();

                    await serializer
                        .SerializeToStreamAsync(new ConnectionResponseMessage(connectionId), writer)
                        .ContinueOnOtherContext();

                    reader.DetachStream();
                    writer.DetachStream();
                    var connection = new StreamSocketConnection(connectionId, socket, serializer);
                    return connection;
                }
            }
        }

        public static async Task<StreamSocketConnection> ConnectAsync(string hostName, string port,
            IObjectSerializer serializer)
        {
            var socket = new StreamSocket();
            var host = new HostName(hostName);
            await socket.ConnectAsync(host, port).ContinueOnOtherContext();
            return await ConnectAsync(socket, serializer).ContinueOnOtherContext();
        }

        internal static async Task<StreamSocketConnection> ConnectAsync(StreamSocket socket,
            IObjectSerializer serializer)
        {
            // Send connection request, receive connection response
            using (var reader = new DataReader(socket.InputStream))
            using (var writer = new DataWriter(socket.OutputStream))
            {
                await serializer
                    .SerializeToStreamAsync(new ConnectionRequestMessage(), writer)
                    .ContinueOnOtherContext();

                var response = await serializer
                    .DeserializeFromStreamAsync(reader, CancellationToken.None)
                    .ContinueOnOtherContext();

                if (response is ConnectionResponseMessage connectionResponse && connectionResponse.IsSuccessful)
                {
                    reader.DetachStream();
                    writer.DetachStream();
                    return new StreamSocketConnection(connectionResponse.ConnectionId, socket, serializer);
                }
                else
                {
                    socket.Dispose();
                    throw new InvalidOperationException(
                        $"Failed to connect to " +
                        $"'{socket.Information.RemoteHostName.ToString()}:{socket.Information.RemotePort}'");
                }
            }
        }

        protected override async Task<RequestResult> SendMessageCoreAsync(object message, RequestOptions options)
        {
            // Send request message
            var request = new StreamSocketRequestMessage
            {
                RequestId = _nextRequestId++,
                Message = message
            };

            await _serializer.SerializeToStreamAsync(request, _writer).ContinueOnOtherContext();

            if (options.IsResponseRequired)
            {
                // Wait for response message
                var responseTask = new TaskCompletionSource<object>();
                _pendingRequests.Add(request.RequestId, responseTask);

                try
                {
                    var response = await responseTask.Task
                        .TimeoutAfter(options.ResponseTimeout)
                        .ContinueOnOtherContext();

                    return new RequestResult(response, RequestStatus.Success);
                }
                catch (TimeoutException)
                {
                    return new RequestResult(null, RequestStatus.ResponseTimeout);
                }
                finally
                {
                    _pendingRequests.Remove(request.RequestId);
                }
            }
            else
            {
                // Do not wait for response message
                return new RequestResult(null, RequestStatus.Success);
            }
        }

        internal async Task<RespondResult> SendResponseAsync(int requestId, object message)
        {
            using (await _mutex.LockAsync().ContinueOnOtherContext())
            {
                if (_status == ConnectionStatus.Disposed)
                    return new RespondResult(ResponseStatus.Failure);

                var response = new StreamSocketResponseMessage
                {
                    RequestId = requestId,
                    Message = message
                };

                await _serializer.SerializeToStreamAsync(response, _writer).ContinueOnOtherContext();
                return new RespondResult(ResponseStatus.Success);
            }
        }

        protected override async Task CloseCoreAsync()
        {
            _disconnectReason = DisconnectReason.LocalPeerDisconnected;

            // Inform peer that we are closing the connection
            await _serializer
                .SerializeToStreamAsync(StreamSocketConnectionCloseMessage.Instance, _writer)
                .ContinueOnOtherContext();

            await Task.Delay(200).ContinueOnOtherContext(); // Give peer some time to read the message

            // Stop receiver task
            _receiverTaskCancellationTokenSource.Cancel();
            await _receiverTask.ContinueOnOtherContext();
        }

        protected override void DisposeCore()
        {
            _disconnected.OnNext(new DisconnectEventArgs(this, _disconnectReason));
            _reader.Dispose();
            _writer.Dispose();
            _socket.Dispose();
        }

        private async Task ReceiveAsync()
        {
            while (true)
            {
                try
                {
                    // Receive next message
                    var o = await _serializer
                        .DeserializeFromStreamAsync(_reader, _receiverTaskCancellationTokenSource.Token)
                        .ContinueOnOtherContext();

                    if (o is StreamSocketConnectionCloseMessage connectionCloseMessage)
                    {
                        // Peer informs us that he closes the connection now
                        _disconnectReason = DisconnectReason.RemotePeerDisconnected;
                        await DisposeAsync().ContinueOnOtherContext();
                        break; // stop receiving
                    }
                    else
                    {
                        // A request or response message has been received
                        HandleReceivedObject(o);
                    }
                }
                catch (COMException)
                {
                    // Socket closed, peer disconnected unexpectedly
                    _disconnectReason = DisconnectReason.UnexpectedDisconnect;
                    await DisposeAsync().ContinueOnOtherContext();
                    break;
                }
                catch
                {
                    // Some other error occurred (e.g. OperationCanceledException after DeserializeFromStreamAsync is
                    // cancelled). If CloseAsync() has been called before, _closeReason is 'LocalPeerDisconnected',
                    // otherwise it is 'Unknown'.
                    if (_disconnectReason == DisconnectReason.LocalPeerDisconnected)
                    {
                        // we are still within IConnection.CloseAsync(),
                        // do nothing here, IConnection.Dispose() will be called
                    }
                    else
                    {
                        await DisposeAsync().ContinueOnOtherContext();
                    }
                    break;
                }
            }

            // this is a fire-and-forget method so that multiple requests can be handled in parallel
            async void HandleReceivedObject(object message)
            {
                switch (message)
                {
                    case StreamSocketRequestMessage requestMessage:
                        var request = new StreamSocketRequest(requestMessage.RequestId, requestMessage.Message, this);
                        _requestReceived.OnNext(request);
                        await request.WaitForDeferralsAsync().ContinueOnOtherContext();

                        // if no one responded to the message, respond with an empty message now
                        if (!request.HasResponded)
                            await request.SendResponseAsync(null).ContinueOnOtherContext();

                        break;

                    case StreamSocketResponseMessage responseMessage:
                        if (_pendingRequests.TryGetValue(responseMessage.RequestId, out var t))
                        {
                            t.SetResult(responseMessage.Message);
                        }
                        break;
                }
            }
        }
    }
}
