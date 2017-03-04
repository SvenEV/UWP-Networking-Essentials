using System.Threading.Tasks;
using UwpNetworkingEssentials.Channels;
using UwpNetworkingEssentials.Rpc;

namespace UwpNetworkingEssentials.ChatSample.ViewModels
{
    public partial class ServerViewModel : ApplicationViewAwareViewModel, IRpcTarget, IServerInterface
    {
        public async Task BroadcastMessageAsync(string message)
        {
            var caller = RpcCallContext.Current.Connection;

            // RPC method called by a client to broadcast a message
            // to all other connected clients
            await RunAsync(() =>
                Messages.Add($"{caller.Id} said: " + message));

            Server.ClientsExcept(caller.Id).AddMessage(message);
        }

        public async void OnConnected(RpcConnectionBase connection)
        {
            await RunAsync(() =>
            {
                Messages.Add($"Client connected: {connection.Id}");
                RaisePropertyChanged(nameof(ClientCount));
            });
        }

        public async void OnDisconnected(RpcConnectionBase connection, DisconnectEventArgs args)
        {
            await RunAsync(() =>
            {
                Messages.Add($"Client disconnected: {args.Connection.Id} (Reason: {args.Reason})");
                RaisePropertyChanged(nameof(ClientCount));
            });
        }

        public async void OnConnectionAttemptFailed(RpcConnectionAttemptFailedException exception)
        {
            await RunAsync(() =>
            {
                Messages.Add($"Client tried to connect but failed: {exception.RemoteHostName}:{exception.RemotePort}");
            });
        }
    }
}
