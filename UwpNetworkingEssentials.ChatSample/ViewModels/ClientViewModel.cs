using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using UwpNetworkingEssentials.Rpc;

namespace UwpNetworkingEssentials.ChatSample.ViewModels
{
    public class ClientViewModel : ViewModelBase, IRpcTarget
    {
        public RpcClient Client { get; private set; }

        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        public ClientViewModel(string port, string serverIp)
        {
            Init(port, serverIp);
        }

        private async void Init(string port, string serverIp)
        {
            try
            {
                var serializer = new DefaultJsonSerializer(GetType().GetTypeInfo().Assembly);
                Client = await RpcClient.ConnectAsync(serverIp, port, this, serializer);
                RaisePropertyChanged(nameof(Client));
            }
            catch
            {
                Messages.Add($"Failed to connect to {serverIp}:{port}");
            }
        }

        public async void SendMessage(string message)
        {
            // Add the message to own message list and send message to
            // server from where it is broadcasted to the other clients
            Messages.Add("I say: " + message);
            try
            {
                await Client.Server.BroadcastMessage(message);
            }
            catch
            {
                Messages.Add("Failed to send message to server");
            }
        }

        public async void AddMessage(string message, [RpcCaller]RpcConnection caller)
        {
            // RPC method called by the server to add a message that has been
            // sent by the server or another client to the message list
            await DispatcherHelper.RunAsync(() =>
                Messages.Add($"{caller.RemoteAddress}:{caller.RemotePort} said: " + message));
        }

        public void OnConnected(RpcConnection connection)
        {
            Messages.Add($"Connected to {connection.RemoteAddress}:{connection.RemotePort} (ID: {connection.Id})");
        }

        public void OnDisconnected(RpcConnection connection)
        {
            Messages.Add($"Disconnected from {connection.RemoteAddress}:{connection.RemotePort} (ID: {connection.Id})");
        }
    }
}
