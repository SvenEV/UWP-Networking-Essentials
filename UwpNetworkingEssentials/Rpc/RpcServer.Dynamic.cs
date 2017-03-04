using System;
using System.Linq;
using UwpNetworkingEssentials.Channels;

namespace UwpNetworkingEssentials.Rpc
{
    public class RpcServer : RpcServerBase<dynamic, RpcConnection>
    {
        public RpcServer(IConnectionListener listener, object callTarget) : base(listener, callTarget)
        {
        }

        /// <inheritdoc/>
        public override dynamic AllClients
        {
            get
            {
                lock (_mutex)
                {
                    if (_isDisposed)
                        throw new ObjectDisposedException(GetType().FullName);

                    return new RpcMultiProxy(_connections.Values.Select(c => c.UnderlyingConnection));
                }
            }
        }

        /// <inheritdoc/>
        public override dynamic Client(string connectionId)
        {
            lock (_mutex)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return _connections[connectionId].Proxy;
            }
        }

        /// <inheritdoc/>
        public override dynamic ClientsExcept(string connectionId)
        {
            lock (_mutex)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(RpcServer));

                return new RpcMultiProxy(_connections.Values
                    .Where(c => c.Id != connectionId)
                    .Select(c => c.UnderlyingConnection));
            }
        }

        protected override RpcConnection NewRpcConnection(IConnection connection, object callTarget,
            Action<RpcConnectionBase> beforeRaiseEvent)
        {
            return new RpcConnection(connection, callTarget, beforeRaiseEvent);
        }
    }
}
