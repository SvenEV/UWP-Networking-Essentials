using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials.Rpc
{
    /// <summary>
    /// A dynamic proxy that issues remote procedure calls
    /// for methods invoked on it.
    /// </summary>
    internal class RpcProxy : DynamicObject, IRpcProxy
    {
        private readonly StreamSocketConnection _connection;

        public RpcProxy(StreamSocketConnection connection)
        {
            _connection = connection;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var call = new RpcCall(binder.Name, args);
            result = CallMethodAsync(call);
            return true;
        }

        internal async Task<object> CallMethodAsync(RpcCall call)
        {
            var result = await _connection.RequestAsync<RpcReturn>(call);

            if (result == null || !result.IsSuccessful)
                throw new InvalidOperationException($"The remote procedure call to '{call.MethodName}' failed ({result?.Error ?? "no details"})");

            return result.Value;
        }
    }
}
