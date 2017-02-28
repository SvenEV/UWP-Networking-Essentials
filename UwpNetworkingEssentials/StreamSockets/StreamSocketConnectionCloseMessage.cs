namespace UwpNetworkingEssentials.StreamSockets
{
    /// <summary>
    /// A message that indicates that an <see cref="IConnection"/>
    /// has been closed gracefully be the remote endpoint.
    /// </summary>
    internal class StreamSocketConnectionCloseMessage
    {
        public static readonly StreamSocketConnectionCloseMessage Instance = new StreamSocketConnectionCloseMessage();

        private StreamSocketConnectionCloseMessage()
        {
        }
    }
}
