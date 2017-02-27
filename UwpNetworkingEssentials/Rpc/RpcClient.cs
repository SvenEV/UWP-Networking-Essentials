using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials.Rpc
{
    public class RpcClient : IRpcClient
    {
        private readonly RpcConnection _connection;
        private readonly IDisposable _requestReceivedSubscription;
        private readonly IDisposable _disconnectedSubscription;
        private readonly object _rpcTarget;

        /// <inheritdoc/>
        public RpcConnection Connection => _connection;

        /// <inheritdoc/>
        public dynamic Server => _connection.Proxy;

        /// <summary>
        /// Initializes an <see cref="RpcClient"/> using the specified connection.
        /// </summary>
        /// <param name="connection">
        /// The underlying connection used to exchange messages
        /// </param>
        /// <param name="rpcTarget">
        /// The object where remote procedure calls from the server
        /// are executed on. Set this to null if you do not want to allow
        /// RPC calls from the server to the client.
        /// </param>
        public RpcClient(IConnection connection, object rpcTarget)
        {
            _connection = new RpcConnection(connection);
            _rpcTarget = rpcTarget;

            _requestReceivedSubscription = connection.RequestReceived
                .Where(r => r.Message is RpcCall)
                .Subscribe(r => RpcHelper.HandleMethodCall(_connection, r, _rpcTarget));

            _disconnectedSubscription = connection.Disconnected.Subscribe(OnDisconnected);
            (rpcTarget as IRpcTarget)?.OnConnected(_connection);
        }
        
        private void OnDisconnected(IDisconnectEventArgs args)
        {
            (_rpcTarget as IRpcTarget)?.OnDisconnected(_connection, args);
        }

        /// <inheritdoc/>
        public async Task DisposeAsync()
        {
            await _connection.DisposeAsync();
            _requestReceivedSubscription.Dispose();
            _disconnectedSubscription.Dispose();
        }
    }
}