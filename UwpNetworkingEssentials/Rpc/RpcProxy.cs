using System.Dynamic;
using UwpNetworkingEssentials.Channels;

namespace UwpNetworkingEssentials.Rpc
{
    /// <summary>
    /// A dynamic proxy that issues remote procedure calls for methods invoked on it.
    /// </summary>
    internal class RpcProxy : DynamicObject
    {
        private readonly IConnection _connection;

        public RpcProxy(IConnection connection)
        {
            _connection = connection;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var call = new RpcCall(binder.Name, args);
            result = RpcHelper.CallMethodAsync(_connection, call);
            return true;
        }
    }
}
