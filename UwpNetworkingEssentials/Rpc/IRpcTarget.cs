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
        /// <param name="connection"></param>
        void OnConnected(RpcConnection connection);

        /// <summary>
        /// For RPC clients this is called after the connection to an RPC
        /// server has been closed.
        /// For RPC servers this is called after the connection to an RPC
        /// client has been closed.
        /// </summary>
        /// <param name="connection"></param>
        void OnDisconnected(RpcConnection connection);
    }
}
