using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using System;
using UwpNetworkingEssentials.Rpc;
using UwpNetworkingEssentials.StreamSockets;

namespace UwpNetworkingEssentials.ChatSample.ViewModels
{
    public partial class ServerViewModel : ViewModelBase, IRpcTarget
    {
        public async void BroadcastMessage(string message, [RpcCaller]RpcConnection caller)
        {
            // RPC method called by a client to broadcast a message
            // to all other connected clients
            await DispatcherHelper.RunAsync(() =>
                Messages.Add($"{caller.Id} said: " + message));

            Server.ClientsExcept(caller.Id).AddMessage(message);
        }

        public async void OnConnected(RpcConnection connection)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                Messages.Add($"Client connected: {connection.Id}");
                RaisePropertyChanged(nameof(ClientCount));
            });
        }

        public async void OnDisconnected(RpcConnection connection, IDisconnectEventArgs args)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                Messages.Add($"Client disconnected: {args.Connection.Id} (Reason: {((StreamSocketDisconnectEventArgs)args).Reason})");
                RaisePropertyChanged(nameof(ClientCount));
            });
        }

        public async void OnConnectionAttemptFailed(RpcConnectionAttemptFailedException exception)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                Messages.Add($"Client tried to connect but failed: {exception.RemoteHostName}:{exception.RemotePort}");
            });
        }
    }
}
