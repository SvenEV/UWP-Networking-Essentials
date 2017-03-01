namespace UwpNetworkingEssentials.Channels
{
    public class DisconnectEventArgs
    {
        /// <summary>
        /// The connection that was closed.
        /// </summary>
        public IConnection Connection { get; }

        public DisconnectReason Reason { get; }

        /// <summary>
        /// Provides additional information about the disconnect.
        /// The value of this property is optional and varies depending on the type of channel.
        /// </summary>
        public object Details { get; }

        public DisconnectEventArgs(IConnection connection, DisconnectReason reason, object details = null)
        {
            Connection = connection;
            Reason = reason;
            Details = details;
        }
    }
}
