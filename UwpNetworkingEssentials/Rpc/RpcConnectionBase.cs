using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Channels;
using Windows.Foundation;

namespace UwpNetworkingEssentials.Rpc
{
    public abstract class RpcConnectionBase
    {
        private readonly IDisposable _requestReceivedSubscription;
        private readonly IDisposable _disconnectedSubscription;

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
                .Subscribe(r => RpcHelper.HandleMethodCall(this, r));

            _disconnectedSubscription = connection.Disconnected.Subscribe(OnDisconnected);

            beforeRaiseEvent?.Invoke(this);
            (callTarget as IRpcTarget)?.OnConnected(this);
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