using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Channels;

namespace UwpNetworkingEssentials.Rpc
{
    public class TypedRpcProxy : DispatchProxy
    {
        private IReadOnlyList<IConnection> _connections = null;

        public static T Create<T>(IConnection connection)
        {
            var proxy = Create<T, TypedRpcProxy>();
            ((dynamic)proxy)._connections = new[] { connection };
            return proxy;
        }

        public static T Create<T>(IEnumerable<IConnection> connections)
        {
            var proxy = Create<T, TypedRpcProxy>();
            ((dynamic)proxy)._connections = connections.ToList();
            return proxy;
        }
        
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var call = new RpcCall(targetMethod.Name, args);
            var returnType = targetMethod.ReturnType;

            // decision tree by expected return type:
            // ________________________________________________________________________________
            // |                       TResult : Task (async execution)?                      |
            // |___________________YES_________________|__________________NO__________________|
            // |          with result (Task<T>)?       |           just 1 connection?         |
            // |_________YES________|_________NO_______|________YES________|________NO________|
            // | just 1 connection? |just 1 connection?|
            // |____YES___|____NO___|___YES___|___NO___|
            
            if (typeof(Task).IsAssignableFrom(returnType))
            {
                // asynchronous execution

                if (returnType.IsTaskTypeWithResult())
                {
                    // Task<TResult>
                    var resultType = returnType.GenericTypeArguments[0];
                    dynamic tcs = Activator.CreateInstance(typeof(TaskCompletionSource<>).MakeGenericType(resultType));
                    
                    if (_connections.Count == 1)
                    {
                        RpcHelper.CallMethodAsync(_connections[0], call).ContinueWith(task =>
                        {
                            // cast to dynamic is required for correct execution by the DLR, do not remove
                            tcs.SetResult((dynamic)task.Result);
                        });
                    }
                    else
                    {
                        var tasks = _connections
                            .Select(conn => RpcHelper.CallMethodAsync(conn, call))
                            .Cast<Task>();

                        // braces are required for correct execution by the DLR, do not remove
                        Task.WhenAll(tasks).ContinueWith(task => { tcs.SetResult(Default(resultType)); });
                    }

                    return tcs.Task;
                }
                else
                {
                    // Task (without result)
                    if (_connections.Count == 1)
                    {
                        // CallMethodAsync returns Task<TResult> which derives from Task, so we can return that directly
                        return RpcHelper.CallMethodAsync(_connections[0], call);
                    }
                    else
                    {
                        // Task.WhenAll returns Task<TResult[]> which derives from Task, so we can return that directly
                        var tasks = _connections.Select(conn => RpcHelper.CallMethodAsync(conn, call));
                        return Task.WhenAll(tasks);
                    }
                }
            }
            else
            {
                // synchronous (blocking) execution

                if (_connections.Count == 1)
                {
                    // do RPC call, block until finished, return result
                    return RpcHelper.CallMethodAsync(_connections[0], call)
                        .ContinueOnOtherContext()
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    // do RPC calls, block until all are finished, return default(TResult)
                    var tasks = _connections.Select(conn => RpcHelper.CallMethodAsync(conn, call)).ToArray();
                    Task.WaitAll(tasks);
                    return Default(returnType);
                }
            }
        }

        private static dynamic Default(Type type)
        {
            return (type.GetTypeInfo().IsValueType && type != typeof(void))
                ? Activator.CreateInstance(type)
                : null;
        }
    }
}
