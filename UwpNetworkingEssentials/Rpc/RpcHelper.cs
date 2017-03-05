using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Channels;

namespace UwpNetworkingEssentials.Rpc
{
    internal static class RpcHelper
    {
        /// <summary>
        /// Sends an <see cref="RpcCall"/> and returns the result from the <see cref="RpcReturn"/> response.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="call"></param>
        /// <returns></returns>
        public static async Task<object> CallMethodAsync(IConnection connection, RpcCall call)
        {
            var response = await connection.SendMessageAsync(call).ContinueOnOtherContext();

            if (response.Status != RequestStatus.Success)
            {
                throw new InvalidOperationException(
                    $"The remote procedure call to '{call.MethodName}' failed ({response.Status})");
            }

            if (response.Response is RpcReturn returnMessage)
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

        /// <summary>
        /// Takes an <see cref="RpcCall"/> from a request, invokes the desired method, and responds to the request
        /// with an <see cref="RpcReturn"/> containing the result of the method invocation.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="rpcCallRequest"></param>
        /// <param name="rpcTarget"></param>
        public static async Task HandleMethodCall(RpcConnectionBase connection, IRequest rpcCallRequest)
        {
            using (var deferral = rpcCallRequest.GetDeferral())
            {
                try
                {
                    if (connection.CallTarget == null)
                    {
                        await rpcCallRequest
                            .SendResponseAsync(
                                RpcReturn.Faulted("No remote procedure calls are allowed on this connection"))
                            .ContinueOnOtherContext();
                    }
                    else
                    {
                        var call = (RpcCall)rpcCallRequest.Message;
                        var result = await InvokeMethodAsync(connection, call).ContinueOnOtherContext();
                        await rpcCallRequest.SendResponseAsync(result).ContinueOnOtherContext();
                    }
                }
                catch
                {
                    await rpcCallRequest.SendResponseAsync(RpcReturn.Faulted("Unknown error")).ContinueOnOtherContext();
                }
            }
        }

        public static async Task<RpcReturn> InvokeMethodAsync(RpcConnectionBase connection, RpcCall call)
        {
            // Current limitations
            // - no support for overloaded methods

            if (string.IsNullOrEmpty(call.MethodName))
                return RpcReturn.Faulted("No method name specified");

            var method = connection.CallTarget.GetType().GetMethod(call.MethodName);

            if (method == null)
            {
                return RpcReturn.Faulted(
                     $"No method with name '{call.MethodName}' could be found (attempted call: {call.ToString()})");
            }

            // Parameter check
            var formalParams = method.GetParameters();
            var actualParams = call.Arguments.ToList();

            if (call.Arguments.Length > formalParams.Length)
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

                if (actualParam == Type.Missing && !formalParam.IsOptional)
                {
                    return RpcReturn.Faulted("Parameter mismatch: Not enough parameters specified " +
                        $"(attempted call: {call.ToString()}, local method: {method.ToDescriptionString()})");
                }
            }

            // Invoke method
            try
            {
                RpcCallContext.Register(call, connection);
                var returnValue = method.Invoke(connection.CallTarget, actualParams.ToArray());

                if (returnValue is Task)
                {
                    await ((Task)returnValue).ContinueOnOtherContext();

                    if (returnValue.GetType().IsTaskTypeWithResult())
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
