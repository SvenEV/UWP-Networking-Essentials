using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using UwpNetworkingEssentials.Rpc;

namespace UwpNetworkingEssentials.ChatSample.ViewModels
{
    public class ServerViewModel : ViewModelBase, IRpcTarget
    {
        public RpcServer Server { get; private set; }

        public int ClientCount => Server.Connections.Count;

        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        public ServerViewModel(string port)
        {
            Init(port);
        }

        private async void Init(string port)
        {
            try
            {
                var serializer = new DefaultJsonSerializer(GetType().GetTypeInfo().Assembly);
                Server = await RpcServer.StartAsync(port, this, serializer);
                Messages.Add($"Server started on port {port}");
                RaisePropertyChanged(nameof(Server));
            }
            catch
            {
                Messages.Add($"Failed to start server on port {port}");
            }
        }

        public async void SendMessage(string message)
        {
            // Add message to own message list and broadcast message
            // to all connected clients
            Messages.Add("I say: " + message);
            await Server.AllClients.AddMessage(message);
        }

        public async void CloseServer()
        {
            await Server.DisposeAsync();
            Messages.Add("Server closed");
        }

        public async void BroadcastMessage(string message, [RpcCaller]RpcConnection caller)
        {
            // RPC method called by a client to broadcast a message
            // to all other connected clients
            await DispatcherHelper.RunAsync(() =>
                Messages.Add($"{caller.RemoteAddress}:{caller.RemotePort} said: " + message));

            Server.ClientsExcept(caller.Id).AddMessage(message);
        }

        public async void OnConnected(RpcConnection connection)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                Messages.Add($"Client connected: {connection.RemoteAddress}:{connection.RemotePort} (ID: {connection.Id})");
                RaisePropertyChanged(nameof(ClientCount));
            });
        }

        public async void OnDisconnected(RpcConnection connection)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                Messages.Add($"Client disconnected: {connection.RemoteAddress}:{connection.RemotePort} (ID: {connection.Id})");
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
