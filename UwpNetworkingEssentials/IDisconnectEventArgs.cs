namespace UwpNetworkingEssentials
{
    public interface IDisconnectEventArgs
    {
        /// <summary>
        /// The connection that has been closed.
        /// </summary>
        IConnection Connection { get; }

        ConnectionCloseReason Reason { get; }
    }

    public interface IDisconnectEventArgs<TConnection> : IDisconnectEventArgs where TConnection : IConnection
    {
        new TConnection Connection { get; }
    }

    public class DisconnectEventArgsBase<TConnection> : IDisconnectEventArgs<TConnection> where TConnection : IConnection
    {
        public TConnection Connection { get; }

        IConnection IDisconnectEventArgs.Connection => Connection;

        public ConnectionCloseReason Reason { get; }

        public DisconnectEventArgsBase(TConnection connection, ConnectionCloseReason reason)
        {
            Connection = connection;
            Reason = reason;
        }
    }
}
