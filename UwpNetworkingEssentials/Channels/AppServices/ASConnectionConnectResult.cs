using Windows.ApplicationModel.AppService;

namespace UwpNetworkingEssentials.Channels.AppServices
{
    public class ASConnectionConnectResult
    {
        public AppServiceConnectionStatus ConnectionStatus { get; }

        public AppServiceHandshakeStatus HandshakeStatus { get; }

        public bool IsSuccessful =>
            ConnectionStatus == AppServiceConnectionStatus.Success &&
            HandshakeStatus == AppServiceHandshakeStatus.Success;

        public ASConnection Connection { get; }

        public ASConnectionConnectResult(
            AppServiceConnectionStatus connectionStatus,
            AppServiceHandshakeStatus handshakeStatus,
            ASConnection connection)
        {
            ConnectionStatus = connectionStatus;
            HandshakeStatus = handshakeStatus;
            Connection = connection;
        }
    }
}