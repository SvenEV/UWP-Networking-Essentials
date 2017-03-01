namespace UwpNetworkingEssentials.Channels
{
    public enum ResponseStatus
    {
        /// <summary>
        /// The response message has been sent successfully.
        /// </summary>
        Success,

        /// <summary>
        /// The response message could not be sent.
        /// </summary>
        Failure,

        /// <summary>
        /// The response message could not be sent because the connection was already closed.
        /// </summary>
        Disconnected,

        /// <summary>
        /// The response message could not be sent because the request has already been responded to.
        /// This state is only applicable to <see cref="SendStatus"/> values returned from
        /// <see cref="IRequest.SendResponseAsync(object)"/>.
        /// </summary>
        RequestAlreadyRespondedTo
    }
}
