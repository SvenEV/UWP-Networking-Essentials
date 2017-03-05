using Nito.AsyncEx;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Channels;
using Windows.Foundation;

namespace UwpNetworkingEssentials.Rpc
{
    public abstract class RpcConnectionBase
    {
        protected readonly AsyncManualResetEvent _initialization = new AsyncManualResetEvent();
        private IDisposable _requestReceivedSubscription;
        private IDisposable _disconnectedSubscription;

        internal event TypedEventHandler<RpcConnectionBase, DisconnectEventArgs> Disconnected;

        /// <summary>
        /// The object on which methods are remotely invoked by the opposite endpoint.
        /// If null, remote invocations are not allowed on this endpoint.
        /// </summary>
        public object CallTarget { get; }

        public IConnection UnderlyingConnection { get; }

        public string Id => UnderlyingConnection.Id;

        internal RpcConnectionBase(IConnection connection, object callTarget,
            Action<RpcConnectionBase> beforeRaiseEvent)
        {
            UnderlyingConnection = connection ?? throw new ArgumentNullException(nameof(connection));
            CallTarget = callTarget;

            _requestReceivedSubscription = connection.RequestReceived
                .Where(r => r.Message is RpcCall)
                .Subscribe(OnCallReceived);

            _disconnectedSubscription = connection.Disconnected.Subscribe(OnDisconnected);

            beforeRaiseEvent?.Invoke(this);
            (callTarget as IRpcTarget)?.OnConnected(this);
        }

        private async void OnCallReceived(IRequest request)
        {
            using (request.GetDeferral())
            {
                await _initialization.WaitAsync(); // wait for constructor to be fully executed
                await RpcHelper.HandleMethodCall(this, request);
            }
        }

        private void OnDisconnected(DisconnectEventArgs args)
        {
            Disconnected?.Invoke(this, args);
            (CallTarget as IRpcTarget)?.OnDisconnected(this, args);
        }

        public async Task DisposeAsync()
        {
            await UnderlyingConnection.CloseAsync();
            _requestReceivedSubscription.Dispose();
            _disconnectedSubscription.Dispose();
        }
    }
}