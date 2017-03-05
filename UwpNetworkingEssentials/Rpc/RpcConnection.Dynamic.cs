using System;
using UwpNetworkingEssentials.Channels;

namespace UwpNetworkingEssentials.Rpc
{
    public class RpcConnection : RpcConnectionBase
    {
        internal readonly RpcProxy _proxy;

        public dynamic Proxy => _proxy;

        public RpcConnection(IConnection connection, object callTarget) : this(connection, callTarget, null)
        {
        }

        internal RpcConnection(IConnection connection, object callTarget, Action<RpcConnectionBase> beforeRaiseEvent)
            : base(connection, callTarget, beforeRaiseEvent)
        {
            _proxy = new RpcProxy(connection);
            _initialization.Set(); // start handling incoming calls
        }
    }
}
