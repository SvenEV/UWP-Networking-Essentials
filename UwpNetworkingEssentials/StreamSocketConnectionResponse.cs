namespace UwpNetworkingEssentials
{
    internal class StreamSocketConnectionResponse
    {
        public bool IsSuccessful { get; }
        public string ConnectionId { get; }

        public StreamSocketConnectionResponse(string connectionId)
        {
            ConnectionId = connectionId;
            IsSuccessful = !string.IsNullOrEmpty(connectionId);
        }
    }
}
