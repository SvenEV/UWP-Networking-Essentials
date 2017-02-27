using UwpNetworkingEssentials.ChatSample.ViewModels;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace UwpNetworkingEssentials.ChatSample
{
    public sealed partial class ServerPage : Page
    {
        public ServerViewModel ViewModel { get; private set; }

        public ServerPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = e.Parameter as ServerViewModel;
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && e.KeyStatus.RepeatCount == 0)
            {
                ViewModel.SendMessage(messageTextBox.Text);
                messageTextBox.Text = "";
            }
        }
    }
}
