using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials.Channels
{
    public interface IRequest : IDeferrable
    {
        /// <summary>
        /// The request message.
        /// </summary>
        object Message { get; }

        /// <summary>
        /// Indicates whether a response has already been sent to this request.
        /// </summary>
        bool HasResponded { get; }

        /// <summary>
        /// Sends a response object to this request.
        /// </summary>
        /// <param name="message">The response message</param>
        /// <returns>Response status</returns>
        /// <exception cref="ResponseAlreadySentException">
        /// A response has already been sent (see <see cref="HasResponded"/>)
        /// </exception>
        /// <remarks>
        /// Calling <see cref="SendResponseAsync(object)"/> is optional. An endpoint does not need to return a response
        /// to a request. If no response is sent after all deferrals are completed, an empty response message is
        /// automatically sent.
        /// </remarks>
        Task<RespondResult> SendResponseAsync(object message);
    }

    public abstract class RequestBase : IRequest
    {
        private readonly DeferralManager _deferralManager = new DeferralManager();
        private readonly object _mutex = new object(); // mutex to protect _hasResponded
        private bool _hasResponded = false;

        public abstract object Message { get; }

        public bool HasResponded { get { lock (_mutex) return _hasResponded; } }

        public async Task<RespondResult> SendResponseAsync(object message)
        {
            lock (_mutex)
            {
                if (_hasResponded)
                    return new RespondResult(ResponseStatus.RequestAlreadyRespondedTo);

                _hasResponded = true;
            }

            try
            {
                // use a deferral here for convenience - this way the API user only needs to acquire a deferral if the call
                // to SendResponseAsync(...) is deferred beyond the RequestReceived event handler
                using (var deferral = GetDeferral())
                    return await SendResponseCoreAsync(message);
            }
            catch (Exception e)
            {
                // the derived class should take care of exceptions, but in case it doesn't we catch exceptions here
                return new RespondResult(ResponseStatus.Failure, e);
            }
        }

        public IDisposable GetDeferral() => _deferralManager.GetDeferral();

        public Task WaitForDeferralsAsync() => _deferralManager.SignalAndWaitAsync();

        protected abstract Task<RespondResult> SendResponseCoreAsync(object message);
    }
}
