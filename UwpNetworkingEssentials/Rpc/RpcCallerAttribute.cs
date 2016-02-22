using System;

namespace UwpNetworkingEssentials.Rpc
{
    /// <summary>
    /// Parameters marked with this attribute will be assigned the
    /// calling client during an RPC call.
    /// The annotated parameter should be of type RpcConnection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class RpcCallerAttribute : Attribute
    {
    }
}
