using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace UwpNetworkingEssentials
{
    /// <summary>
    /// A message-based abstraction on top of the <see cref="StreamSocket"/> API.
    /// </summary>
    public class StreamSocketConnection
    {
        private readonly StreamSocket _socket;
        private readonly IObjectSerializer _serializer;
        private readonly DataReader _reader;
        private readonly DataWriter _writer;
        private readonly Task _receiverTask;
        private readonly CancellationTokenSource _receiverTaskCancellationTokenSource = new CancellationTokenSource();
        private readonly Subject<object> _objectReceived = new Subject<object>();
        private readonly SemaphoreSlim _sema = new SemaphoreSlim(1);
        private bool _isDisposed = false;

        public string Id { get; }

        public StreamSocketInformation Information => _socket.Information;

        public IObservable<object> ObjectReceived => _objectReceived;

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
                var request = await serializer.DeserializeAsync(reader, CancellationToken.None) as StreamSocketConnectionRequest;

                if (request == null)
                {
                    // Wrong message received => reject connection
                    await serializer.SerializeAsync(new StreamSocketConnectionResponse(null), writer);
                    socket.Dispose();
                    return null;
                }
                else
                {
                    // Accept connection request
                    var connectionId = Guid.NewGuid().ToString();
                    await serializer.SerializeAsync(new StreamSocketConnectionResponse(connectionId), writer);
                    reader.DetachStream();
                    writer.DetachStream();
                    var connection = new StreamSocketConnection(connectionId, socket, serializer);
                    return connection;
                }
            }
        }

        public static async Task<StreamSocketConnection> ConnectAsync(string hostName, string port, IObjectSerializer serializer)
        {
            // Send connection request, receive connection response
            var socket = new StreamSocket();
            var host = new HostName(hostName);

            await socket.ConnectAsync(host, port);

            using (var reader = new DataReader(socket.InputStream))
            using (var writer = new DataWriter(socket.OutputStream))
            {
                await serializer.SerializeAsync(new StreamSocketConnectionRequest(), writer);
                var response = await serializer.DeserializeAsync(reader, CancellationToken.None) as StreamSocketConnectionResponse;

                if (response != null && response.IsSuccessful)
                {
                    reader.DetachStream();
                    writer.DetachStream();
                    return new StreamSocketConnection(response.ConnectionId, socket, serializer);
                }
                else
                {
                    socket.Dispose();
                    throw new InvalidOperationException($"Failed to connect to '{hostName}:{port}'");
                }
            }
        }

        public async Task SendAsync(object o)
        {
            await _sema.WaitAsync();

            try
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(StreamSocketConnection));

                await _serializer.SerializeAsync(o, _writer);
            }
            finally
            {
                _sema.Release();
            }
        }

        public async Task<TResponse> RequestAsync<TResponse>(object requestObject)
        {
            await _sema.WaitAsync();

            try
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(StreamSocketConnection));

                await _serializer.SerializeAsync(requestObject, _writer);
            }
            finally
            {
                _sema.Release();
            }

            var response = await _objectReceived.OfType<TResponse>().FirstOrDefaultAsync();
            return response;
        }

        public async Task DisposeAsync()
        {
            await _sema.WaitAsync();

            try
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;

                // Inform peer that we are closing the connection
                await _serializer.SerializeAsync(new StreamSocketConnectionCloseMessage(), _writer);

                _receiverTaskCancellationTokenSource.Cancel();
                await _receiverTask;
            }
            finally
            {
                _sema.Release();
            }
        }

        private async Task ReceiveAsync()
        {
            while (!_isDisposed)
            {
                try
                {
                    // Receive next message
                    var o = await _serializer.DeserializeAsync(_reader, _receiverTaskCancellationTokenSource.Token);

                    if (o is StreamSocketConnectionCloseMessage)
                    {
                        // Peer informs us that he closes the connection now
                        _objectReceived.OnNext(o);
                        DisposeInternal();
                    }
                    else
                    {
                        // A user defined message has been received
                        _objectReceived.OnNext(o);
                    }
                }
                catch (COMException)
                {
                    // Socket closed, peer disconnected unexpectedly
                    _objectReceived.OnNext(new StreamSocketConnectionCloseMessage());
                    DisposeInternal();
                }
                catch
                {
                    // Some other error occurred
                    // (e.g. OperationCanceledException after DeserializeAsync is cancelled)
                    _objectReceived.OnNext(new StreamSocketConnectionCloseMessage());
                    DisposeInternal();
                }
            }
        }

        private void DisposeInternal()
        {
            _isDisposed = true;
            _reader.Dispose();
            _writer.Dispose();
            _socket.Dispose();
            _objectReceived.OnCompleted();
        }
    }
}
