namespace UwpNetworkingEssentials.Channels
{
    /// <summary>
    /// Provides information about the result of a <see cref="IConnection.SendMessageAsync(object)"/> call, including
    /// the response object.
    /// </summary>
    public class RequestResult
    {
        public object Response { get; }

        public RequestStatus Status { get; }

        public object StatusDetails { get; }

        public RequestResult(object response, RequestStatus status, object statusDetails = null)
        {
            Response = response;
            Status = status;
            StatusDetails = statusDetails;
        }
    }
}
