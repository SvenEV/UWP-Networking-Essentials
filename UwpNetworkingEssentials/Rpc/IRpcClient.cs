using System.Threading.Tasks;

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

        /// <summary>
        /// Closes the connection to the server.
        /// </summary>
        /// <returns></returns>
        Task DisposeAsync();
    }
}
