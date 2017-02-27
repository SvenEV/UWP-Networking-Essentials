namespace UwpNetworkingEssentials
{
    /// <summary>
    /// A message that indicates that a <see cref="StreamSocketConnection"/>
    /// has been closed due to any of the following reasons:
    /// The local peer decided to close the connection,
    /// the remote peer decided to close the connection or
    /// the remote peer disconnected unexpectedly.
    /// </summary>
    internal class ConnectionCloseMessage
    {
        public ConnectionCloseReason Reason { get; }

        public ConnectionCloseMessage(ConnectionCloseReason reason)
        {
            Reason = reason;
        }
    }
}
