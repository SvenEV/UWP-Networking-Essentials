using Windows.Networking.Sockets;

namespace UwpNetworkingEssentials.Channels.StreamSockets
{
    public class StreamSocketConnectionReceiveEventArgs : IConnectionReceiveEventArgs
    {
        public StreamSocketConnectionListener Listener { get; }

        IConnectionListener IConnectionReceiveEventArgs.Listener => Listener;

        StreamSocket Socket { get; }

        public StreamSocketConnectionReceiveEventArgs(StreamSocketConnectionListener listener, StreamSocket socket)
        {
            Listener = listener;
            Socket = socket;
        }
    }
}
