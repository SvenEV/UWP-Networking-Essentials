using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace UwpNetworkingEssentials.StreamSockets
{
    public class StreamSocketConnectionListener : ConnectionListenerBase<StreamSocketConnection>
    {
        private readonly StreamSocketListener _listener = new StreamSocketListener();
        private readonly Subject<StreamSocketConnection> _connectionReceived = new Subject<StreamSocketConnection>();
        private readonly IObjectSerializer _serializer;

        /// <summary>
        /// Gets the local port that is listened on.
        /// </summary>
        public string Port { get; }

        public override IObservable<StreamSocketConnection> ConnectionReceived => _connectionReceived;

        public StreamSocketConnectionListener(string port, IObjectSerializer serializer)
        {
            Port = port;
            _serializer = serializer;
            _listener.ConnectionReceived += OnConnectionReceived;
        }

        private async void OnConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                var connection = await StreamSocketConnection.AcceptConnectionAsync(args.Socket, _serializer);
                if (connection != null)
                    _connectionReceived.OnNext(connection);
            }
            catch
            {
                // Connection attempt failed
            }
        }

        public override async Task StartAsync()
        {
            await _listener.BindServiceNameAsync(Port);
        }

        public override Task DisposeAsync()
        {
            _listener.ConnectionReceived -= OnConnectionReceived;
            _listener.Dispose();
            _connectionReceived.OnCompleted();
            return Task.CompletedTask;
        }
    }
}
