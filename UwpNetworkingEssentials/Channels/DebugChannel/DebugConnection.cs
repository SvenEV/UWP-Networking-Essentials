using System;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials.Channels.DebugChannel
{
    public class DebugConnection : ConnectionBase
    {
        private DisconnectReason _disconnectReason;

        internal DebugConnection OppositeConnection { get; private set; }

        public static DebugConnection Connect(DebugConnectionListener target)
        {
            var serverToClientConnection = target.OnConnectionReceived();
            var clientToServerConnection = new DebugConnection(serverToClientConnection.Id);
            serverToClientConnection.OppositeConnection = clientToServerConnection;
            clientToServerConnection.OppositeConnection = serverToClientConnection;
            return clientToServerConnection;
        }

        public DebugConnection(string id) : base(id)
        {
        }

        protected override async Task CloseCoreAsync()
        {
            _disconnectReason = DisconnectReason.LocalPeerDisconnected;
            await OppositeConnection.NotifyRemoteEndpointClosedAsync().ContinueOnOtherContext();
        }

        protected override async Task<RequestResult> SendMessageCoreAsync(object message, RequestOptions options)
        {
            var request = new DebugRequest(message, this);

            if (options.IsResponseRequired)
            {
                try
                {
                    return await OppositeConnection
                        .HandleIncomingRequestAsync(request)
                        .TimeoutAfter(options.ResponseTimeout)
                        .ContinueOnOtherContext();
                }
                catch (TimeoutException)
                {
                    return new RequestResult(null, RequestStatus.ResponseTimeout);
                }
            }
            else
            {
                var task = OppositeConnection.HandleIncomingRequestAsync(request); // do not wait for response
                return new RequestResult(null, RequestStatus.Success);
            }
        }

        private async Task<RequestResult> HandleIncomingRequestAsync(DebugRequest request)
        {
            _requestReceived.OnNext(request);
            await request.WaitForDeferralsAsync().ContinueOnOtherContext();
            return request.Response;
        }

        private async Task NotifyRemoteEndpointClosedAsync()
        {
            _disconnectReason = DisconnectReason.RemotePeerDisconnected;
            await DisposeAsync().ContinueOnOtherContext();
        }

        protected override void DisposeCore()
        {
            _disconnected.OnNext(new DisconnectEventArgs(this, _disconnectReason));
        }
    }
}
