using GalaSoft.MvvmLight;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace UwpNetworkingEssentials.ChatSample.ViewModels
{
    public class ApplicationViewAwareViewModel : ViewModelBase
    {
        private readonly CoreApplicationView _view;

        public ApplicationViewAwareViewModel(CoreApplicationView view)
        {
            _view = view;
        }

        public async Task RunAsync(DispatchedHandler action)
        {
            await _view.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, action);
        }
    }
}
