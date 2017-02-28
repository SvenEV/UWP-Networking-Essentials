using Nito.AsyncEx;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials
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
        IObservable<IDisconnectEventArgs> Disconnected { get; }

        Task<IResponse> SendMessageAsync(object message);

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <remarks>
        /// Calls to <see cref="CloseAsync"/> are allowed at any time, but only the first call in the
        /// <see cref="ConnectionStatus.Connected"/> state has an effect.
        /// </remarks>
        Task CloseAsync();
    }

    public interface IConnection<TRequest, TResponse, TDisconnectEventArgs> : IConnection
        where TRequest : IRequest
        where TResponse : IResponse
        where TDisconnectEventArgs : IDisconnectEventArgs
    {
        new IObservable<TRequest> RequestReceived { get; }

        new IObservable<TDisconnectEventArgs> Disconnected { get; }

        new Task<TResponse> SendMessageAsync(object message);
    }

    public abstract class ConnectionBase<TRequest, TResponse, TDisconnectEventArgs>
        : IConnection<TRequest, TResponse, TDisconnectEventArgs>
        where TRequest : IRequest
        where TResponse : IResponse
        where TDisconnectEventArgs : IDisconnectEventArgs
    {
        protected readonly AsyncLock _mutex = new AsyncLock(); // mutex to protect _status
        protected readonly Subject<TRequest> _requestReceived = new Subject<TRequest>();
        protected readonly Subject<TDisconnectEventArgs> _disconnected = new Subject<TDisconnectEventArgs>();
        protected ConnectionStatus _status = ConnectionStatus.Connected;
        private readonly IObservable<IRequest> _requestReceivedNonGeneric;
        private readonly IObservable<IDisconnectEventArgs> _disconnectedNonGeneric;

        public string Id { get; }

        public IObservable<TRequest> RequestReceived => _requestReceived;

        IObservable<IRequest> IConnection.RequestReceived => _requestReceivedNonGeneric;

        public IObservable<TDisconnectEventArgs> Disconnected => _disconnected;

        IObservable<IDisconnectEventArgs> IConnection.Disconnected => _disconnectedNonGeneric;

        public async Task CloseAsync()
        {
            using (await _mutex.LockAsync())
            {
                if (_status == ConnectionStatus.Connected)
                {
                    await CloseCoreAsync();
                    _status = ConnectionStatus.Disconnected;
                    Dispose();
                }
            }
        }

        protected async Task DisposeAsync()
        {
            using (await _mutex.LockAsync())
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

        async Task<IResponse> IConnection.SendMessageAsync(object message) => await SendMessageAsync(message);

        public async Task<TResponse> SendMessageAsync(object message)
        {
            using (await _mutex.LockAsync())
            {
                if (_status == ConnectionStatus.Disposed) // state 'Disconnecting' cannot occur here
                    throw new ObjectDisposedException(GetType().FullName);

                return await SendMessageCoreAsync(message);
            }
        }

        public ConnectionBase(string id)
        {
            Id = id;
            _requestReceivedNonGeneric = RequestReceived.Select(r => (IRequest)r);
            _disconnectedNonGeneric = Disconnected.Select(e => (IDisconnectEventArgs)e);
        }

        // will be called at most once
        protected abstract Task CloseCoreAsync();

        // will be called at most once, raises the Disconnected event, _status may be queried in here
        protected abstract void DisposeCore();

        protected abstract Task<TResponse> SendMessageCoreAsync(object message);
    }
}
