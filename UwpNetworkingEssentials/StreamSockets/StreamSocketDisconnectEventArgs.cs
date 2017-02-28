namespace UwpNetworkingEssentials.StreamSockets
{
    public class StreamSocketDisconnectEventArgs : DisconnectEventArgsBase<StreamSocketConnection>
    {
        public StreamSocketDisconnectEventArgs(StreamSocketConnection connection, DisconnectReason reason)
            : base(connection, reason)
        {
        }
    }
}
