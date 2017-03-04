using System;
using System.Linq;
using UwpNetworkingEssentials.Channels;

namespace UwpNetworkingEssentials.Rpc
{
    public class RpcServer<T> : RpcServerBase<T, RpcConnection<T>>
    {
        public RpcServer(IConnectionListener listener, object callTarget) : base(listener, callTarget)
        {
        }

        /// <inheritdoc/>
        public override T AllClients
        {
            get
            {
                lock (_mutex)
                {
                    if (_isDisposed)
                        throw new ObjectDisposedException(GetType().FullName);

                    return TypedRpcProxy.Create<T>(_connections.Values.Select(c => c.UnderlyingConnection));
                }
            }
        }

        /// <inheritdoc/>
        public override T Client(string connectionId)
        {
            lock (_mutex)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return _connections[connectionId].Proxy;
            }
        }

        /// <inheritdoc/>
        public override T ClientsExcept(string connectionId)
        {
            lock (_mutex)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(RpcServer));

                return TypedRpcProxy.Create<T>(_connections.Values
                    .Where(c => c.Id != connectionId)
                    .Select(c => c.UnderlyingConnection));
            }
        }

        protected override RpcConnection<T> NewRpcConnection(IConnection connection, object callTarget,
            Action<RpcConnectionBase> beforeRaiseEvent)
        {
            return new RpcConnection<T>(connection, callTarget, beforeRaiseEvent);
        }
    }
}
