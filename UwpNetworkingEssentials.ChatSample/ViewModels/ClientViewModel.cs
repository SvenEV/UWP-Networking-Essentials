using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Channels.AppServices;
using UwpNetworkingEssentials.Channels.DebugChannel;
using UwpNetworkingEssentials.Channels.StreamSockets;
using UwpNetworkingEssentials.Rpc;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UwpNetworkingEssentials.ChatSample.ViewModels
{
    public partial class ClientViewModel : ApplicationViewAwareViewModel, IRpcTarget
    {
        private readonly Frame _frame = Window.Current.Content as Frame;

        public RpcClient Client { get; private set; }

        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        public ClientViewModel(CoreApplicationView view) : base(view)
        {
        }

        public async Task ConnectViaStreamSocketsAsync(string port, string serverIp)
        {
            try
            {
                var serializer = new DefaultJsonSerializer(GetType().GetTypeInfo().Assembly);
                var connection = await StreamSocketConnection.ConnectAsync(serverIp, port, serializer);
                //var connection = await BluetoothConnection.ConnectAsync(MainViewModel.CustomBluetoothServiceId, serializer);
                Client = new RpcClient(connection, this);
                RaisePropertyChanged(nameof(Client));
            }
            catch
            {
                Messages.Add($"Failed to connect to {serverIp}:{port}");
            }
        }

        public async Task ConnectViaAppServicesAsync(string packageFamilyName)
        {
            try
            {
                var serializer = new DefaultJsonSerializer(GetType().GetTypeInfo().Assembly);
                var result = await ASConnection.ConnectLocallyAsync("Chat", packageFamilyName, serializer);
                Client = new RpcClient(result.Connection, this);
                RaisePropertyChanged(nameof(Client));
            }
            catch
            {
                Messages.Add($"Failed to connect to app service in package {packageFamilyName}");
            }
        }

        public void ConnectViaDebugChannel(DebugConnectionListener target)
        {
            try
            {
                var result = DebugConnection.Connect(target);
                Client = new RpcClient(result, this);
                RaisePropertyChanged(nameof(Client));
            }
            catch
            {
                Messages.Add($"Failed to connect to debug channel");
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

        public async void Disconnect()
        {
            if (Client != null)
                await Client.DisposeAsync();

            Messages.Add("Client disconnected");
        }
    }
}
