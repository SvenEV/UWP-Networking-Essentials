using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using System.Linq;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UwpNetworkingEssentials.ChatSample.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly Frame _frame;
        private string _port = "1234";
        private string _serverIp = "192.168.178.";

        public string Port
        {
            get { return _port; }
            set { Set(ref _port, value); }
        }

        public string ServerIp
        {
            get { return _serverIp; }
            set { Set(ref _serverIp, value); }
        }

        public string LocalIp { get; private set; }

        public MainViewModel()
        {
            _frame = Window.Current.Content as Frame;
            DispatcherHelper.Initialize();
            LocalIp = GetLocalIp();
        }

        public void StartServer()
        {
            var vm = new ServerViewModel(Port);
            _frame.Navigate(typeof(ServerPage), vm);
        }

        public void ConnectClient()
        {
            var vm = new ClientViewModel(Port, ServerIp);
            _frame.Navigate(typeof(ClientPage), vm);
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
