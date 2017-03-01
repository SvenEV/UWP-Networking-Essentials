using System;

namespace UwpNetworkingEssentials.Channels
{
    public class RequestOptions
    {
        public static readonly RequestOptions Default = new RequestOptions();

        /// <summary>
        /// The maximum amount of time a call to <see cref="IConnection.SendMessageAsync(object)"/> waits for a response
        /// before the call returns with the status <see cref="RequestStatus.ResponseTimeout"/>.
        /// </summary>
        /// <remarks>
        /// The default timespan is 30 seconds. Not all channels support this option and some channels might timeout
        /// earlier or later than specified by <see cref="ResponseTimeout"/>.
        /// </remarks>
        public TimeSpan ResponseTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Determines whether a response is awaited.
        /// </summary>
        /// <remarks>
        /// If <see cref="IsResponseRequired"/> is set to false, the returned response object is always null.
        /// The default value is true.
        /// </remarks>
        public bool IsResponseRequired { get; set; } = true;
    }
}
