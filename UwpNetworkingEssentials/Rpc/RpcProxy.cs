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
        private readonly IConnection _connection;

        public RpcProxy(IConnection connection)
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
            var response = await _connection.SendMessageAsync(call);

            if (response.Message is RpcReturn returnMessage)
            {
                if (!returnMessage.IsSuccessful)
                {
                    throw new InvalidOperationException($"The remote procedure call to '{call.MethodName}' failed " +
                        $"({returnMessage.Error ?? "no details"})");
                }

                return returnMessage.Value;
            }

            throw new InvalidOperationException(
                $"The remote procedure call to '{call.MethodName}' failed (invalid response message)");
        }
    }
}
