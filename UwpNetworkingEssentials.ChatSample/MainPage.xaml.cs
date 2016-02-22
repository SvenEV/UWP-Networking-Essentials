using UwpNetworkingEssentials.ChatSample.ViewModels;
using Windows.UI.Xaml.Controls;

namespace UwpNetworkingEssentials.ChatSample
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }

        public MainPage()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            DataContext = this;
        }
    }
}
