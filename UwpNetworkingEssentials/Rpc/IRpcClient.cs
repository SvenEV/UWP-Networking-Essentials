namespace UwpNetworkingEssentials.Rpc
{
    public interface IRpcClient
    {
        /// <summary>
        /// Gets the underlying connection to the RPC server.
        /// </summary>
        RpcConnection Connection { get; }

        /// <summary>
        /// Gets a proxy that can be used to invoke
        /// methods on the server.
        /// </summary>
        dynamic Server { get; }
    }
}
