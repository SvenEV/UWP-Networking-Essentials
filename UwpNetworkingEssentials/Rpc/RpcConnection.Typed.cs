using System;
using UwpNetworkingEssentials.Channels;

namespace UwpNetworkingEssentials.Rpc
{
    public class RpcConnection<T> : RpcConnectionBase
    {
        public T Proxy { get; }

        public RpcConnection(IConnection connection, object callTarget) : this(connection, callTarget, null)
        {
        }

        internal RpcConnection(IConnection connection, object callTarget, Action<RpcConnectionBase> beforeRaiseEvent)
            : base(connection, callTarget, beforeRaiseEvent)
        {
            Proxy = TypedRpcProxy.Create<T>(connection);
            _initialization.Set(); // start handling incoming calls
        }
    }
}
