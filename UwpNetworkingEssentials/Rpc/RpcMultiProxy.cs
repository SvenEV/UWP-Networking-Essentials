using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Channels;

namespace UwpNetworkingEssentials.Rpc
{
    /// <summary>
    /// A dynamic proxy that issues remote procedure calls for a group of connections.
    /// </summary>
    internal class RpcMultiProxy : DynamicObject
    {
        private readonly IReadOnlyList<IConnection> _connections;

        public RpcMultiProxy(IEnumerable<IConnection> connections)
        {
            _connections = connections.ToList();
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var call = new RpcCall(binder.Name, args);
            result = CallMethodAsync(call);
            return true;
        }

        internal async Task<object[]> CallMethodAsync(RpcCall call)
        {
            var tasks = _connections.Select(connection => RpcHelper.CallMethodAsync(connection, call));
            var results = await Task.WhenAll(tasks);
            return results;
        }
    }
}
