using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;

namespace UwpNetworkingEssentials.AppServices
{
    public class ASConnectionListener : ConnectionListenerBase<ASConnection>
    {
        private readonly IObjectSerializer _serializer;

        public ASConnectionListener(IObjectSerializer serializer)
        {
            _serializer = serializer;
        }

        /// <summary>
        /// Call this method in
        /// <see cref="Windows.UI.Xaml.Application.OnBackgroundActivated(BackgroundActivatedEventArgs)"/>
        /// (single process model) or
        /// <see cref="IBackgroundTask.Run(IBackgroundTaskInstance)"/> (multi process model)
        /// to accept an incoming app service connection.
        /// </summary>
        /// <param name="taskInstance">Background task instance</param>
        /// <returns>
        /// True if the background activation event has been handled by this <see cref="ASConnectionListener"/>.
        /// This is the case if the connection listener is started and the task instance contains app service activation
        /// details of type <see cref="AppServiceTriggerDetails"/>. If the listener is not started or the activation is
        /// of a different kind, false is returned.
        /// </returns>
        public async Task<bool> HandleBackgroundActivationAsync(IBackgroundTaskInstance taskInstance)
        {
            using (await _mutex.LockAsync())
            {
                if (_status == ConnectionListenerStatus.Disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                if (_status == ConnectionListenerStatus.Inactive)
                    return false; // not listening => activation event not handled

                if (taskInstance.TriggerDetails is AppServiceTriggerDetails e)
                {
                    var deferral = taskInstance.GetDeferral();

                    var connection = await ASConnection.AcceptConnectionAsync(e, deferral, _serializer);
                    if (connection != null)
                        _connectionReceived.OnNext(connection);

                    return true;
                }

                return false;
            }
        }

        protected override Task DisposeCoreAsync() => Task.CompletedTask; // nothing to do

        protected override Task StartCoreAsync() => Task.CompletedTask; // nothing to do
    }
}
