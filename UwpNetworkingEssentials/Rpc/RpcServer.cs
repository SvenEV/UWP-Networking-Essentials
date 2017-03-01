using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Channels;

namespace UwpNetworkingEssentials.Rpc
{

    public class RpcServer : IRpcServer
    {
        private readonly Dictionary<string, RpcConnectionInfo> _connections =
            new Dictionary<string, RpcConnectionInfo>();

        private readonly IConnectionListener _listener;
        private readonly SemaphoreSlim _sema = new SemaphoreSlim(1);
        private readonly IDisposable _connectionReceivedSubscription;
        private readonly object _rpcTarget;
        private readonly object _lock = new object();
        private bool _isDisposed = false;

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, RpcConnection> Connections { get; }

        /// <inheritdoc/>
        public dynamic AllClients
        {
            get
            {
                lock (_lock)
                {
                    if (_isDisposed)
                        throw new ObjectDisposedException(GetType().FullName);

                    return new RpcMultiProxy(_connections.Values.Select(c => c.Connection._proxy));
                }
            }
        }

        /// <inheritdoc/>
        public IConnectionListener Listener => _listener;

        /// <summary>
        /// Initializes an <see cref="RpcServer"/> using the specified
        /// underlying connection listener.
        /// </summary>
        /// <param name="listener">
        /// The listener for incoming connections.
        /// </param>
        /// <param name="rpcTarget">
        /// The object where remote procedure calls from clients are
        /// executed on. Set this to null if you do not want to allow
        /// RPC calls from clients to the server.
        /// </param>
        public RpcServer(IConnectionListener listener, object rpcTarget)
        {
            _listener = listener ?? throw new ArgumentNullException(nameof(listener));
            _rpcTarget = rpcTarget;
            _connectionReceivedSubscription = _listener.ConnectionReceived.Subscribe(OnConnectionReceived);
            Connections = _connections.ToDictionaryAccessor(info => info.Connection);
        }

        /// <inheritdoc/>
        public dynamic Client(string connectionId)
        {
            lock (_lock)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return _connections[connectionId].Connection.Proxy;
            }
        }

        /// <inheritdoc/>
        public dynamic ClientsExcept(string connectionId)
        {
            lock (_lock)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(RpcServer));

                return new RpcMultiProxy(_connections.Values
                    .Where(c => c.Connection.Id != connectionId)
                    .Select(c => c.Connection._proxy));
            }
        }

        private async void OnConnectionReceived(IConnection connection)
        {
            if (connection != null)
            {
                await _sema.WaitAsync();
                var rpcConnection = new RpcConnection(connection);

                var connectionInfo = new RpcConnectionInfo(rpcConnection,
                    connection.RequestReceived
                        .Where(r => r.Message is RpcCall)
                        .Subscribe(r => RpcHelper.HandleMethodCall(rpcConnection, r, _rpcTarget)),
                    connection.Disconnected
                        .Subscribe(args => OnClientDisconnected(rpcConnection, args)));

                _connections.Add(connection.Id, connectionInfo);

                (_rpcTarget as IRpcTarget)?.OnConnected(rpcConnection);
                _sema.Release();
            }
        }
        
        private async void OnClientDisconnected(RpcConnection connection, DisconnectEventArgs args)
        {
            await _sema.WaitAsync();

            try
            {
                _connections.Remove(connection.Id);
                (_rpcTarget as IRpcTarget)?.OnDisconnected(connection, args);
            }
            finally
            {
                _sema.Release();
            }
        }

        /// <inheritdoc/>
        public async Task DisposeAsync()
        {
            await _sema.WaitAsync();

            try
            {
                lock (_lock)
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
            finally
            {
                _sema.Release();
            }
        }

        class RpcConnectionInfo
        {
            private readonly IDisposable _requestReceivedSubscription;
            private readonly IDisposable _disconnectedSubscription;

            public RpcConnection Connection { get; }

            public RpcConnectionInfo(RpcConnection connection, IDisposable requestReceivedSubscription,
                IDisposable disconnectedSubscription)
            {
                Connection = connection;
                _requestReceivedSubscription = requestReceivedSubscription;
                _disconnectedSubscription = disconnectedSubscription;
            }

            public async Task DisposeAsync()
            {
                _requestReceivedSubscription.Dispose();
                _disconnectedSubscription.Dispose();
                await Connection.DisposeAsync();
            }
        }
    }
}
