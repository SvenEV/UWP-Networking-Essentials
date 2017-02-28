using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UwpNetworkingEssentials.ChatSample.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public static readonly Guid CustomBluetoothServiceId = Guid.Parse("34B1CF4D-1069-4AD6-89B6-E161D79BE4D8");

        private readonly Frame _frame;
        private string _port = "1234";
        private string _serverIp = "192.168.178.";
        private string _packageFamilyName = Package.Current.Id.FamilyName;

        public string Port { get => _port; set => Set(ref _port, value); }

        public string ServerIp { get => _serverIp; set => Set(ref _serverIp, value); }

        public string LocalIp { get; private set; }

        public string PackageFamilyName { get => _packageFamilyName; set => Set(ref _packageFamilyName, value); }

        public MainViewModel()
        {
            _frame = Window.Current.Content as Frame;
            DispatcherHelper.Initialize();
            LocalIp = GetLocalIp();
        }

        public void StartServer()
        {
            var vm = new ServerViewModel();
            _frame.Navigate(typeof(ServerPage), vm);
        }

        public async void ConnectClientViaStreamSockets()
        {
            var vm = new ClientViewModel();
            _frame.Navigate(typeof(ClientPage), vm);
            await vm.ConnectViaStreamSocketsAsync(Port, ServerIp);
        }

        public async void ConnectClientViaAppServices()
        {
            var vm = new ClientViewModel();
            _frame.Navigate(typeof(ClientPage), vm);
            await vm.ConnectViaAppServicesAsync(PackageFamilyName);
        }

        private string GetLocalIp()
        {
            var profile = NetworkInformation.GetInternetConnectionProfile();

            if (profile?.NetworkAdapter == null)
                return null;

            var hostNames = NetworkInformation.GetHostNames().Where(hn =>
                hn.IPInformation?.NetworkAdapter != null &&
                hn.IPInformation.NetworkAdapter.NetworkAdapterId == profile.NetworkAdapter.NetworkAdapterId);

            return hostNames.Any() ?
                string.Join("\r\n", hostNames.Select(hn => hn.CanonicalName)) :
                "(could not determine)";
        }
    }
}
