namespace UwpNetworkingEssentials.Rpc
{
    /// <summary>
    /// Represents a remote procedure call.
    /// </summary>
    internal class RpcCall
    {
        public string MethodName { get; set; }

        public object[] Parameters { get; set; }

        public RpcCall()
        {
        }

        public RpcCall(string methodName, object[] parameters)
        {
            MethodName = methodName;
            Parameters = parameters;
        }
    }
}
