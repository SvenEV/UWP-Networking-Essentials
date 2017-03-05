using System;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace UwpNetworkingEssentials.Channels.StreamSockets
{
    public class StreamSocketConnectionListener : ConnectionListenerBase<StreamSocketConnection>
    {
        private readonly StreamSocketListener _listener = new StreamSocketListener();
        private readonly IObjectSerializer _serializer;

        /// <summary>
        /// Gets the local port that is listened on.
        /// </summary>
        public string Port { get; }

        public StreamSocketConnectionListener(string port, IObjectSerializer serializer)
        {
            Port = port;
            _serializer = serializer;
            _listener.ConnectionReceived += OnConnectionReceived;
        }

        private async void OnConnectionReceived(StreamSocketListener sender,
            StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                var connection = await StreamSocketConnection
                    .AcceptConnectionAsync(args.Socket, _serializer)
                    .ContinueOnOtherContext();

                if (connection != null)
                    _connectionReceived.OnNext(connection);
            }
            catch
            {
                // Connection attempt failed
            }
        }

        protected override async Task StartCoreAsync()
        {
            await _listener.BindServiceNameAsync(Port).ContinueOnOtherContext();
        }

        protected override Task DisposeCoreAsync()
        {
            _listener.ConnectionReceived -= OnConnectionReceived;
            _listener.Dispose();
            return Task.CompletedTask;
        }
    }
}
