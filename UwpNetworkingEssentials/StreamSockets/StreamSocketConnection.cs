using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace UwpNetworkingEssentials.StreamSockets
{
    /// <summary>
    /// A message-based abstraction on top of the <see cref="StreamSocket"/> API.
    /// </summary>
    public class StreamSocketConnection : ConnectionBase<StreamSocketRequest, StreamSocketResponse, StreamSocketDisconnectEventArgs>
    {
        private readonly StreamSocket _socket;
        private readonly IObjectSerializer _serializer;
        private readonly DataReader _reader;
        private readonly DataWriter _writer;
        private readonly Task _receiverTask;
        private readonly CancellationTokenSource _receiverTaskCancellationTokenSource = new CancellationTokenSource();
        private readonly Subject<StreamSocketRequest> _requestReceived = new Subject<StreamSocketRequest>();
        private readonly Subject<StreamSocketDisconnectEventArgs> _disconnected = new Subject<StreamSocketDisconnectEventArgs>();
        private readonly Dictionary<int, TaskCompletionSource<StreamSocketResponse>> _pendingRequests = new Dictionary<int, TaskCompletionSource<StreamSocketResponse>>();
        private readonly SemaphoreSlim _sema = new SemaphoreSlim(1);
        private bool _isDisposed = false;
        private int _nextRequestId = 0;

        public override string Id { get; }

        public string LocalAddress => $"{_socket.Information.LocalAddress.ToString()}:{_socket.Information.LocalPort}";

        public string RemoteAddress => $"{_socket.Information.RemoteAddress.ToString()}:{_socket.Information.RemotePort}";

        public StreamSocketInformation Information => _socket.Information;

        public override IObservable<StreamSocketRequest> RequestReceived => _requestReceived;

        public override IObservable<StreamSocketDisconnectEventArgs> Disconnected => _disconnected;

        private StreamSocketConnection(string id, StreamSocket socket, IObjectSerializer serializer)
        {
            Id = id;
            _socket = socket;
            _serializer = serializer;
            _reader = new DataReader(socket.InputStream);
            _writer = new DataWriter(socket.OutputStream);
            _receiverTask = ReceiveAsync();
        }

        public static async Task<StreamSocketConnection> AcceptConnectionAsync(StreamSocket socket, IObjectSerializer serializer)
        {
            // Receive connection request, send connection response
            using (var reader = new DataReader(socket.InputStream))
            using (var writer = new DataWriter(socket.OutputStream))
            {
                var request = await serializer.DeserializeFromStreamAsync(reader, CancellationToken.None) as ConnectionRequestMessage;

                if (request == null)
                {
                    // Wrong message received => reject connection
                    await serializer.SerializeToStreamAsync(new ConnectionResponseMessage(null), writer);
                    socket.Dispose();
                    return null;
                }
                else
                {
                    // Accept connection request
                    var connectionId = "SSC_" + Guid.NewGuid().ToString();
                    await serializer.SerializeToStreamAsync(new ConnectionResponseMessage(connectionId), writer);
                    reader.DetachStream();
                    writer.DetachStream();
                    var connection = new StreamSocketConnection(connectionId, socket, serializer);
                    return connection;
                }
            }
        }

        public static async Task<StreamSocketConnection> ConnectAsync(string hostName, string port, IObjectSerializer serializer)
        {
            var socket = new StreamSocket();
            var host = new HostName(hostName);
            await socket.ConnectAsync(host, port);
            return await ConnectAsync(socket, serializer);
        }

        internal static async Task<StreamSocketConnection> ConnectAsync(StreamSocket socket, IObjectSerializer serializer)
        {
            // Send connection request, receive connection response
            using (var reader = new DataReader(socket.InputStream))
            using (var writer = new DataWriter(socket.OutputStream))
            {
                await serializer.SerializeToStreamAsync(new ConnectionRequestMessage(), writer);
                var response = await serializer.DeserializeFromStreamAsync(reader, CancellationToken.None) as ConnectionResponseMessage;

                if (response != null && response.IsSuccessful)
                {
                    reader.DetachStream();
                    writer.DetachStream();
                    return new StreamSocketConnection(response.ConnectionId, socket, serializer);
                }
                else
                {
                    socket.Dispose();
                    throw new InvalidOperationException($"Failed to connect to '{socket.Information.RemoteHostName.ToString()}:{socket.Information.RemotePort}'");
                }
            }
        }

        public override async Task<StreamSocketResponse> SendMessageAsync(object message)
        {
            await _sema.WaitAsync();

            try
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(StreamSocketConnection));

                // Send request message
                var request = new StreamSocketRequestMessage
                {
                    RequestId = _nextRequestId++,
                    Message = message
                };

                await _serializer.SerializeToStreamAsync(request, _writer);

                // Wait for response message
                var responseTask = new TaskCompletionSource<StreamSocketResponse>();
                _pendingRequests.Add(request.RequestId, responseTask);
                var response = await responseTask.Task; // TODO: Maybe implement a timeout here
                _pendingRequests.Remove(request.RequestId);
                return response;
            }
            finally
            {
                _sema.Release();
            }
        }

        internal async Task SendResponseAsync(int requestId, object message)
        {
            await _sema.WaitAsync();

            try
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(StreamSocketConnection));

                var response = new StreamSocketResponseMessage
                {
                    RequestId = requestId,
                    Message = message
                };

                await _serializer.SerializeToStreamAsync(response, _writer);
            }
            finally
            {
                _sema.Release();
            }
        }
        
        public override async Task DisposeAsync()
        {
            await _sema.WaitAsync();

            try
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;

                // Inform peer that we are closing the connection
                await _serializer.SerializeToStreamAsync(new ConnectionCloseMessage(ConnectionCloseReason.RemotePeerDisconnected), _writer);
                await Task.Delay(200); // Give peer some time to read the message

                // Stop receiver task
                _receiverTaskCancellationTokenSource.Cancel();
                await _receiverTask;
            }
            finally
            {
                _sema.Release();
            }
        }

        private void HandleReceivedObject(object message)
        {
            switch (message)
            {
                case StreamSocketRequestMessage requestMessage:
                    var request = new StreamSocketRequest(requestMessage.RequestId, requestMessage.Message, this);
                    _requestReceived.OnNext(request);
                    break;

                case StreamSocketResponseMessage responseMessage:
                    if (_pendingRequests.TryGetValue(responseMessage.RequestId, out var t))
                        t.SetResult(new StreamSocketResponse(responseMessage.Message));
                    break;
            }
        }
        
        private async Task ReceiveAsync()
        {
            while (!_isDisposed)
            {
                try
                {
                    // Receive next message
                    var o = await _serializer.DeserializeFromStreamAsync(_reader, _receiverTaskCancellationTokenSource.Token);

                    if (o is ConnectionCloseMessage connectionCloseMessage)
                    {
                        // Peer informs us that he closes the connection now
                        DisposeInternal(connectionCloseMessage.Reason);
                    }
                    else
                    {
                        // A user defined message has been received
                        HandleReceivedObject(o);
                    }
                }
                catch (COMException)
                {
                    // Socket closed, peer disconnected unexpectedly
                    DisposeInternal(ConnectionCloseReason.UnexpectedDisconnect);
                }
                catch
                {
                    // Some other error occurred
                    // (e.g. OperationCanceledException after DeserializeAsync is cancelled).
                    // If DisposeAsync() has been called before, _isDisposed is true.
                    var reason = _isDisposed ?
                        ConnectionCloseReason.LocalPeerDisconnected :
                        ConnectionCloseReason.Unknown;

                    DisposeInternal(reason);
                }
            }
        }

        private void DisposeInternal(ConnectionCloseReason connectionCloseReason)
        {
            _disconnected.OnNext(new StreamSocketDisconnectEventArgs(this, connectionCloseReason));
            _disconnected.OnCompleted();
            _isDisposed = true;
            _reader.Dispose();
            _writer.Dispose();
            _socket.Dispose();
            _requestReceived.OnCompleted();
        }
    }
}
