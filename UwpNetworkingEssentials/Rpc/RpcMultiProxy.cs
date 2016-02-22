using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials.Rpc
{
    /// <summary>
    /// A dynamic proxy that issues remote procedure calls
    /// for a group of clients.
    /// </summary>
    internal class RpcMultiProxy : DynamicObject, IRpcProxy
    {
        private RpcProxy[] _proxies;

        public RpcMultiProxy(IEnumerable<RpcProxy> proxies)
        {
            _proxies = proxies.ToArray();
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var call = new RpcCall(binder.Name, args);
            result = CallMethodAsync(call);
            return true;
        }

        internal async Task<object[]> CallMethodAsync(RpcCall call)
        {
            var results = await Task.WhenAll(_proxies.Select(proxy => proxy.CallMethodAsync(call)));
            return results;
        }
    }
}
