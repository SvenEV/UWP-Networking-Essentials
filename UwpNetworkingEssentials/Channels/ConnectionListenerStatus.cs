namespace UwpNetworkingEssentials.Channels
{
    public enum ConnectionListenerStatus
    {
        /// <summary>
        /// The connection listener is not listening for incoming connections.
        /// This is the initial state of a connection listener.
        /// </summary>
        Inactive,

        /// <summary>
        /// The connection listener is started and listening for incoming connections.
        /// </summary>
        Active,

        /// <summary>
        /// The connection listener is stopped and not listening for incoming connections.
        /// A disposed connection listener cannot be started again.
        /// </summary>
        Disposed
    }
}
