using UwpNetworkingEssentials.Channels;
using Windows.Networking.Sockets;

namespace UwpNetworkingEssentials.Rpc
{
    /// <summary>
    /// Provides additional connection events.
    /// An RPC target object can optionally implement this interface to
    /// receive these events.
    /// </summary>
    public interface IRpcTarget
    {
        /// <summary>
        /// For RPC clients this is called after the connection to an RPC
        /// server has been established.
        /// For RPC servers this is called after a connection to a new
        /// RPC client has been established.
        /// </summary>
        /// <param name="connection">The connection that has been established</param>
        void OnConnected(RpcConnection connection);

        /// <summary>
        /// For RPC clients and servers this is called after the connection
        /// attempt of a client has failed.
        /// </summary>
        /// <param name="exception">Exception</param>
        void OnConnectionAttemptFailed(RpcConnectionAttemptFailedException exception);

        /// <summary>
        /// For RPC clients this is called after the connection to an RPC
        /// server has been closed.
        /// For RPC servers this is called after the connection to an RPC
        /// client has been closed.
        /// </summary>
        /// <param name="connection">The connection that has been closed</param>
        /// <param name="args">Provides further information about the disconnect</param>
        void OnDisconnected(RpcConnection connection, DisconnectEventArgs args);
    }
}
