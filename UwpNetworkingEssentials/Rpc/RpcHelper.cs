using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials.Rpc
{
    internal static class RpcHelper
    {
        public static async void HandleMethodCall(RpcConnection connection, IRequest rpcCallRequest, object rpcTarget)
        {
            using (var deferral = rpcCallRequest.GetDeferral())
            {
                try
                {
                    if (rpcTarget == null)
                    {
                        await rpcCallRequest.SendResponseAsync(
                            RpcReturn.Faulted("No remote procedure calls are allowed on this connection"));
                    }

                    var call = (RpcCall)rpcCallRequest.Message;
                    var result = await InvokeMethodAsync(rpcTarget, connection, call);
                    await rpcCallRequest.SendResponseAsync(result);
                }
                catch
                {
                    await rpcCallRequest.SendResponseAsync(RpcReturn.Faulted("Unknown error"));
                }
            }
        }

        public static async Task<RpcReturn> InvokeMethodAsync(object rpcTarget, RpcConnection connection, RpcCall call)
        {
            // Current limitations
            // - no support for overloaded methods

            if (string.IsNullOrEmpty(call.MethodName))
                return RpcReturn.Faulted("No method name specified");

            var method = rpcTarget.GetType().GetMethod(call.MethodName);

            if (method == null)
            {
                return RpcReturn.Faulted(
                     $"No method with name '{call.MethodName}' could be found (attempted call: {call.ToString()})");
            }

            // Parameter check
            var formalParams = method.GetParameters();
            var actualParams = call.Parameters.ToList();

            if (call.Parameters.Length > formalParams.Length)
            {
                return RpcReturn.Faulted("Parameter mismatch: Too many arguments specified " +
                    $"(attempted call: {call.ToString()}, local method: {method.ToDescriptionString()})");
            }

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
                    return RpcReturn.Faulted("Parameter mismatch: Got value of type " +
                        $"'{actualParam.GetType().FullName}' for parameter " +
                        $"'{formalParam.ParameterType.FullName} {formalParam.Name}' " +
                        $"(attempted call: {call.ToString()}, local method: {method.ToDescriptionString()})");
                }

                if (formalParam.CustomAttributes.Any(a => a.AttributeType == typeof(RpcCallerAttribute)) &&
                    formalParam.ParameterType.IsAssignableFrom(typeof(RpcConnection)))
                {
                    // Insert RpcProxy into [RpcCaller]-annotated parameter
                    actualParams[i] = connection;
                }
                else if (actualParam == Type.Missing && !formalParam.IsOptional)
                {
                    return RpcReturn.Faulted("Parameter mismatch: Not enough parameters specified " +
                        $"(attempted call: {call.ToString()}, local method: {method.ToDescriptionString()})");
                }
            }

            // Invoke method
            try
            {
                var returnValue = method.Invoke(rpcTarget, actualParams.ToArray());

                if (returnValue is Task)
                {
                    await (Task)returnValue;

                    if (returnValue.GetType().GetGenericTypeDefinition() == typeof(Task<>) &&
                        returnValue.GetType().GetGenericArguments()[0].Name != "VoidTaskResult")
                        returnValue = ((dynamic)returnValue).Result;
                    else
                        returnValue = null;
                }

                return RpcReturn.Success(returnValue);
            }
            catch (Exception e)
            {
                return RpcReturn.Faulted($"Local execution failed (exception: {e.ToString()})");
            }
        }
    }
}
