namespace UwpNetworkingEssentials.Channels
{
    public enum DisconnectReason
    {
        /// <summary>
        /// The connection has been closed without a known reason.
        /// </summary>
        Unknown,

        /// <summary>
        /// The connection has been closed due to a network error.
        /// </summary>
        UnexpectedDisconnect,

        /// <summary>
        /// The local peer has explicitly closed the connection.
        /// </summary>
        LocalPeerDisconnected,

        /// <summary>
        /// The remote peer has explicitly closed the connection.
        /// </summary>
        RemotePeerDisconnected
    }
}
