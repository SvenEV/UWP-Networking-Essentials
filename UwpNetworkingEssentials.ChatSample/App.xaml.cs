using System;
using System.Reflection;
using UwpNetworkingEssentials.AppServices;
using UwpNetworkingEssentials.Rpc;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace UwpNetworkingEssentials.ChatSample
{
    sealed partial class App : Application
    {
        public static ASConnectionListener TheASConnectionListener { get; set; }

        public App()
        {
            InitializeComponent();

            TheASConnectionListener = new ASConnectionListener(
                new DefaultJsonSerializer(GetType().GetTypeInfo().Assembly));
            TheASConnectionListener.StartAsync();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }

            Window.Current.Activate();
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            if (TheASConnectionListener == null ||
                !await TheASConnectionListener.HandleBackgroundActivationAsync(args.TaskInstance))
            {
                // Handle other stuff
            }
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
