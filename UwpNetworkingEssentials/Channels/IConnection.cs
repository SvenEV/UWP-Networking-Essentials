using Nito.AsyncEx;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials.Channels
{
    public interface IConnection
    {
        /// <summary>
        /// The connection identifier.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The event that is raised when an object is received.
        /// </summary>
        IObservable<IRequest> RequestReceived { get; }

        /// <summary>
        /// The event that is raised when the connection is closed.
        /// </summary>
        IObservable<DisconnectEventArgs> Disconnected { get; }

        /// <summary>
        /// Sends a message using default options.
        /// <seealso cref="RequestOptions.Default"/>
        /// </summary>
        /// <param name="message">Request object</param>
        /// <returns>Result containing the response object and status information</returns>
        Task<RequestResult> SendMessageAsync(object message);

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">Request object</param>
        /// <param name="options">Request options</param>
        /// <returns>Result containing the response object and status information</returns>
        Task<RequestResult> SendMessageAsync(object message, RequestOptions options);

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <remarks>
        /// Calls to <see cref="CloseAsync"/> are allowed at any time, but only the first call in the
        /// <see cref="ConnectionStatus.Connected"/> state has an effect.
        /// </remarks>
        Task CloseAsync();
    }

    public abstract class ConnectionBase : IConnection
    {
        protected readonly AsyncLock _mutex = new AsyncLock(); // mutex to protect _status
        protected readonly Subject<IRequest> _requestReceived = new Subject<IRequest>();
        protected readonly Subject<DisconnectEventArgs> _disconnected = new Subject<DisconnectEventArgs>();
        protected ConnectionStatus _status = ConnectionStatus.Connected;

        public string Id { get; }

        public IObservable<IRequest> RequestReceived => _requestReceived;

        public IObservable<DisconnectEventArgs> Disconnected => _disconnected;

        // will be called at most once
        protected abstract Task CloseCoreAsync();

        // will be called at most once, raises the Disconnected event, _status may be queried in here
        protected abstract void DisposeCore();

        protected abstract Task<RequestResult> SendMessageCoreAsync(object message, RequestOptions options);

        public ConnectionBase(string id)
        {
            Id = id;
        }

        public async Task CloseAsync()
        {
            using (await _mutex.LockAsync().ContinueOnOtherContext())
            {
                if (_status == ConnectionStatus.Connected)
                {
                    await CloseCoreAsync().ContinueOnOtherContext();
                    _status = ConnectionStatus.Disconnected;
                    Dispose();
                }
            }
        }

        public Task<RequestResult> SendMessageAsync(object message)
        {
            return SendMessageAsync(message, RequestOptions.Default);
        }

        public async Task<RequestResult> SendMessageAsync(object message, RequestOptions options)
        {
            using (await _mutex.LockAsync().ContinueOnOtherContext())
            {
                if (_status == ConnectionStatus.Disposed) // state 'Disconnected' cannot occur here
                    return new RequestResult(null, RequestStatus.Disconnected);

                try
                {
                    return await SendMessageCoreAsync(message, options).ContinueOnOtherContext();
                }
                catch (Exception e)
                {
                    // the derived class should take care of exceptions, but in case it doesn't we catch exceptions here
                    return new RequestResult(null, RequestStatus.Failure, e);
                }
            }
        }

        protected async Task DisposeAsync()
        {
            using (await _mutex.LockAsync().ContinueOnOtherContext())
                Dispose();
        }

        private void Dispose()
        {
            // At this point, _status may be
            // - 'Disconnected' if the local peer gracefully closed the connection via CloseAsync()
            // - 'Connected' if the remote peer closed the connection or an error occurred

            DisposeCore();
            _requestReceived.OnCompleted();
            _disconnected.OnCompleted();
            _status = ConnectionStatus.Disposed;
        }
    }
}
