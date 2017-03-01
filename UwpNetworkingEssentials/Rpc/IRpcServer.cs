using System.Collections.Generic;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Channels;

namespace UwpNetworkingEssentials.Rpc
{
    public interface IRpcServer
    {
        /// <summary>
        /// Gets the underlying connections to the RPC clients.
        /// The dictionary keys correspond to connection IDs.
        /// </summary>
        IReadOnlyDictionary<string, RpcConnection> Connections { get; }

        /// <summary>
        /// Gets the connection listener that listens for
        /// incoming connections.
        /// </summary>
        IConnectionListener Listener { get; }

        /// <summary>
        /// Gets a proxy that can be used to invoke
        /// methods on all connected clients.
        /// </summary>
        dynamic AllClients { get; }

        /// <summary>
        /// Gets a proxy that can be used to invoke
        /// methods on the specified client.
        /// </summary>
        /// <param name="connectionId">Client connection ID</param>
        /// <returns>Dynamic proxy</returns>
        dynamic Client(string connectionId);

        /// <summary>
        /// Gets a proxy that can be used to invoke
        /// methods on all clients except the client
        /// with the specified connection ID.
        /// </summary>
        /// <param name="connectionId">Connection ID to be excluded</param>
        /// <returns>Dynamic proxy</returns>
        dynamic ClientsExcept(string connectionId);

        /// <summary>
        /// Disconnects all clients and stops the server.
        /// </summary>
        /// <returns></returns>
        Task DisposeAsync();
    }
}
