namespace UwpNetworkingEssentials.Channels
{
    /// <summary>
    /// If the local endpoint purposely closes a connection via <see cref="IConnection.CloseAsync"/>, the status
    /// transitions from <see cref="Connected"/> to <see cref="Disconnected"/> and eventually to <see cref="Disposed"/>.
    /// If the remote endpoint purposely closes a connection via <see cref="IConnection.CloseAsync"/> or the connection
    /// is lost due to an error, the status transitions directly from <see cref="Connected"/> to <see cref="Disposed"/>.
    /// </summary>
    public enum ConnectionStatus
    {
        Connected,

        Disconnected,

        Disposed
    }
}
