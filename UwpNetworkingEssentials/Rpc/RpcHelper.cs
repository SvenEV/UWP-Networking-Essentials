using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials.Rpc
{
    internal static class RpcHelper
    {
        public static async void HandleMethodCall(RpcConnection connection, RpcCall call, object rpcTarget)
        {
            try
            {
                if (rpcTarget == null)
                    await connection.SocketConnection.SendAsync(RpcReturn.Faulted("No remote procedure calls are allowed on this connection"));

                var result = await InvokeMethodAsync(rpcTarget, connection, call);
                await connection.SocketConnection.SendAsync(result);
            }
            catch
            {
                await connection.SocketConnection.SendAsync(RpcReturn.Faulted("Unknown error"));
            }
        }

        public static async Task<RpcReturn> InvokeMethodAsync(object rpcTarget, RpcConnection connection, RpcCall call)
        {
            // Current limitations
            // - no support for overloaded methods

            var method = rpcTarget.GetType().GetMethod(call.MethodName);

            if (method == null)
                return RpcReturn.Faulted("No such method");

            // Parameter check
            var formalParams = method.GetParameters();
            var actualParams = call.Parameters.ToList();

            if (call.Parameters.Length > formalParams.Length)
                return RpcReturn.Faulted("Parameter mismatch: Too many arguments specified");

            for (var i = 0; i < formalParams.Length; i++)
            {
                var formalParam = formalParams[i];
                var actualParam = (actualParams.Count <= i) ? null : actualParams[i];

                if (actualParam == null)
                {
                    actualParams.Add(actualParam = Type.Missing);
                }
                else if (!formalParam.ParameterType.IsAssignableFrom(actualParam.GetType()))
                {
                    return RpcReturn.Faulted($"Parameter mismatch: Got value of type '{actualParam.GetType().FullName}' for parameter '{formalParam.GetType().FullName} {formalParam.Name}'");
                }

                if (formalParam.CustomAttributes.Any(a => a.AttributeType == typeof(RpcCallerAttribute)) &&
                    formalParam.ParameterType.IsAssignableFrom(typeof(RpcConnection)))
                {
                    // Insert RpcProxy into [RpcCaller]-annotated parameter
                    actualParams[i] = connection;
                }
                else if (actualParam == Type.Missing && !formalParam.IsOptional)
                {
                    return RpcReturn.Faulted("Parameter mismatch: Not enough parameters specified");
                }
            }

            // Invoke method
            var returnValue = method.Invoke(rpcTarget, actualParams.ToArray());

            if (returnValue is Task)
            {
                await (Task)returnValue;

                if (returnValue.GetType() == typeof(Task<>))
                    returnValue = ((dynamic)returnValue).Result;
                else
                    returnValue = null;
            }

            return RpcReturn.Success(returnValue);
        }
    }
}
