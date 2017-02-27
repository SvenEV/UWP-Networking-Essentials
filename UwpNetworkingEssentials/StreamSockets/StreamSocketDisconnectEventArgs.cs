namespace UwpNetworkingEssentials.StreamSockets
{
    public class StreamSocketDisconnectEventArgs : DisconnectEventArgsBase<StreamSocketConnection>
    {
        public StreamSocketDisconnectEventArgs(StreamSocketConnection connection, ConnectionCloseReason reason) : base(connection, reason)
        {
        }
    }
}
