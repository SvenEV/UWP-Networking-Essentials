using UwpNetworkingEssentials.ChatSample.ViewModels;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace UwpNetworkingEssentials.ChatSample
{
    public sealed partial class ClientPage : Page
    {
        public ClientViewModel ViewModel { get; private set; }

        public ClientPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = e.Parameter as ClientViewModel;
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
