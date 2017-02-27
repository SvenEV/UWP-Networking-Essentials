namespace UwpNetworkingEssentials
{
    internal class ConnectionResponseMessage
    {
        public bool IsSuccessful { get; }
        public string ConnectionId { get; }

        public ConnectionResponseMessage(string connectionId)
        {
            ConnectionId = connectionId;
            IsSuccessful = !string.IsNullOrEmpty(connectionId);
        }
    }
}
