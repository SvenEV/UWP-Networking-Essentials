namespace UwpNetworkingEssentials.Channels
{
    public enum RequestStatus
    {
        /// <summary>
        /// The request message has been sent successfully and a response message has been received successfully if
        /// desired.
        /// </summary>
        Success,

        /// <summary>
        /// The request message could not be sent.
        /// </summary>
        Failure,

        /// <summary>
        /// The request message could not be sent because the connection was already closed.
        /// </summary>
        Disconnected,

        /// <summary>
        /// The request message has been sent successfully, but a timeout has been triggered while waiting for the
        /// response message.
        /// </summary>
        ResponseTimeout
    }
}
