namespace UwpNetworkingEssentials.Channels
{
    /// <summary>
    /// Provides information about the result of a <see cref="IRequest.SendResponseAsync(object)"/> call.
    /// </summary>
    public class RespondResult
    {
        public ResponseStatus Status { get; }
        
        /// <summary>
        /// Additional information.
        /// The value of this property is optional and varies depending on the type of channel.
        /// </summary>
        public object StatusDetails { get; }

        public RespondResult(ResponseStatus status, object statusDetails = null)
        {
            Status = status;
            StatusDetails = statusDetails;
        }
    }
}
