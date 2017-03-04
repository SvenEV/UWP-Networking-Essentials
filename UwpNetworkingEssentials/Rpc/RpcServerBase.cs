using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Channels;

namespace UwpNetworkingEssentials.Rpc
{
    public abstract class RpcServerBase<T, TRpcConnection> where TRpcConnection : RpcConnectionBase
    {
        private readonly IConnectionListener _listener;
        private readonly AsyncLock _sema = new AsyncLock();
        private readonly IDisposable _connectionReceivedSubscription;
        private readonly object _callTarget;

        protected readonly object _mutex = new object();
        protected bool _isDisposed = false;

        protected readonly Dictionary<string, TRpcConnection> _connections =
            new Dictionary<string, TRpcConnection>();

        public IReadOnlyDictionary<string, TRpcConnection> Connections => _connections;

        public IConnectionListener Listener => _listener;

        public abstract T AllClients { get; }

        public abstract T Client(string connectionId);

        public abstract T ClientsExcept(string connectionId);

        protected abstract TRpcConnection NewRpcConnection(IConnection connection, object callTarget,
            Action<RpcConnectionBase> beforeRaiseEvent);

        /// <summary>
        /// Initializes an <see cref="RpcServer"/> using the specified underlying connection listener.
        /// </summary>
        /// <param name="listener">The listener for incoming connections</param>
        /// <param name="callTarget">
        /// The object where remote procedure calls from clients are executed on. Set this to null if you do not want
        /// to allow RPC calls from clients to the server.
        /// </param>
        public RpcServerBase(IConnectionListener listener, object callTarget)
        {
            _listener = listener ?? throw new ArgumentNullException(nameof(listener));
            _callTarget = callTarget;
            _connectionReceivedSubscription = _listener.ConnectionReceived.Subscribe(OnConnectionReceived);
        }

        private async void OnConnectionReceived(IConnection connection)
        {
            using (await _sema.LockAsync().ContinueOnOtherContext())
            {
                var rpcConnection = NewRpcConnection(connection, _callTarget,
                    c => _connections.Add(c.Id, (TRpcConnection)c));
                rpcConnection.Disconnected += OnClientDisconnected;
            }
        }

        private async void OnClientDisconnected(RpcConnectionBase connection, DisconnectEventArgs args)
        {
            connection.Disconnected -= OnClientDisconnected;

            using (await _sema.LockAsync().ContinueOnOtherContext())
            {
                _connections.Remove(connection.Id);
            }
        }

        /// <inheritdoc/>
        public async Task DisposeAsync()
        {
            using (await _sema.LockAsync().ContinueOnOtherContext())
            {
                lock (_mutex)
                {
                    _isDisposed = true;
                }

                // Stop listener
                _connectionReceivedSubscription.Dispose();
                await _listener.DisposeAsync();

                // Dispose all connections
                var disposalTasks = _connections.Values.Select(conn => conn.DisposeAsync());
                await Task.WhenAll(disposalTasks);
                _connections.Clear();
            }
        }
    }
}
