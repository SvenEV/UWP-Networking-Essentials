using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Channels.AppServices;
using UwpNetworkingEssentials.Channels.DebugChannel;
using UwpNetworkingEssentials.Channels.MultiChannel;
using UwpNetworkingEssentials.Channels.StreamSockets;
using UwpNetworkingEssentials.Rpc;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UwpNetworkingEssentials.ChatSample.ViewModels
{
    public partial class ServerViewModel : ApplicationViewAwareViewModel, IRpcTarget, IServerInterface
    {
        private readonly MultiChannelConnectionListener _multiChannelListener;
        private readonly Frame _frame = Window.Current.Content as Frame;
        private readonly IObjectSerializer _serializer;
        private string _port = "1234";

        public RpcServer<IClientInterface> Server { get; private set; }

        public string Port { get => _port; set => Set(ref _port, value); }

        public int ClientCount => Server?.Connections.Count ?? 0;

        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        public ServerViewModel(CoreApplicationView view) : base(view)
        {
            _serializer = new DefaultJsonSerializer(GetType().GetTypeInfo().Assembly);

            _multiChannelListener = new MultiChannelConnectionListener();
            _multiChannelListener.StartAsync().Wait();

            Server = new RpcServer<IClientInterface>(_multiChannelListener, this);
            Messages.Add($"Server started");
            RaisePropertyChanged(nameof(Server));
        }

        public async void StartStreamSocketConnectionListener()
        {
            try
            {
                var listener = new StreamSocketConnectionListener(Port, _serializer);
                await listener.StartAsync();
                _multiChannelListener.Listeners.Add(listener);
                Messages.Add("Started StreamSocketConnectionListener");
            }
            catch
            {
                Messages.Add($"Failed to start {nameof(StreamSocketConnectionListener)}");
            }
        }

        public async void StartASConnectionListener()
        {
            var listener = new ASConnectionListener("Chat", _serializer);
            await listener.StartAsync();
            _multiChannelListener.Listeners.Add(listener);
            App.TheASConnectionListener = listener;
            Messages.Add("Started ASConnectionListener");
        }

        public async void StartDebugConnectionListener()
        {
            var listener = new DebugConnectionListener();
            await listener.StartAsync();
            _multiChannelListener.Listeners.Add(listener);
            Messages.Add("Started DebugConnectionListener");
        }

        public async void ConnectDebugClient()
        {
            var debugListener = _multiChannelListener.Listeners
                .OfType<DebugConnectionListener>()
                .FirstOrDefault();

            if (debugListener != null)
            {
                var view = CoreApplication.CreateNewView();
                var viewId = 0;

                await view.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var frame = new Frame();
                    Window.Current.Content = frame;

                    var vm = new ClientViewModel(view);
                    vm.ConnectViaDebugChannel(debugListener);

                    frame.Navigate(typeof(ClientPage), vm);

                    Window.Current.Activate();
                    viewId = ApplicationView.GetForCurrentView().Id;
                });

                var viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(viewId);
            }
        }

        public void SendMessage(string message)
        {
            // Add message to own message list and broadcast message
            // to all connected clients
            Messages.Add("I say: " + message);
            Server.AllClients.AddMessage(message);
        }

        public async void CloseServer()
        {
            await Server.DisposeAsync();
            Messages.Add("Server closed");
            await Task.Delay(2000);
            if (_frame.CanGoBack)
                _frame.GoBack();
        }
    }
}
