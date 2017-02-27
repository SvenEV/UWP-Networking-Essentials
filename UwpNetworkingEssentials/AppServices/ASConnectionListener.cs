using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;

namespace UwpNetworkingEssentials.AppServices
{
    public class ASConnectionListener : ConnectionListenerBase<ASConnection>
    {
        private readonly Subject<ASConnection> _connectionReceived = new Subject<ASConnection>();
        private readonly IObjectSerializer _serializer;
        private bool _isRunning = false;

        public override IObservable<ASConnection> ConnectionReceived => _connectionReceived;

        public ASConnectionListener(IObjectSerializer serializer)
        {
            _serializer = serializer;
        }

        public async Task<bool> HandleBackgroundActivationAsync(BackgroundActivatedEventArgs args)
        {
            if (!_isRunning)
                return false; // activation event not handled by AppServiceConnectionListener

            if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails e)
            {
                var deferral = args.TaskInstance.GetDeferral();

                var connection = await ASConnection.AcceptConnectionAsync(e, deferral, _serializer);
                if (connection != null)
                    _connectionReceived.OnNext(connection);

                return true;
            }

            return false;
        }

        public override Task DisposeAsync()
        {
            _isRunning = false;
            return Task.CompletedTask;
        }

        public override Task StartAsync()
        {
            _isRunning = true;
            return Task.CompletedTask;
        }
    }
}
