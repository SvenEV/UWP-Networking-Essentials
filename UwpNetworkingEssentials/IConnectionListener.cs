using Nito.AsyncEx;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials
{
    public interface IConnectionListener
    {
        /// <summary>
        /// The event that is raised when a connection is incoming.
        /// </summary>
        IObservable<IConnection> ConnectionReceived { get; }

        // TODO: Implement event for failed connection attempts
        // IObservable<IConnectionAttempt> ConnectionAttemptFailed { get; }
        
        /// <summary>
        /// Begins listening for incoming connections.
        /// </summary>
        /// <remarks>
        /// Further calls to <see cref="StartAsync"/> while the listener is active are allowed and have no effect.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The object has been disposed</exception>
        Task StartAsync();

        /// <summary>
        /// Stops listening for incoming connections. After disposal the listener cannot be started again.
        /// </summary>
        Task DisposeAsync();
    }

    public interface IConnectionListener<TConnection> : IConnectionListener where TConnection : IConnection
    {
        new IObservable<TConnection> ConnectionReceived { get; }
    }

    /// <summary>
    /// The base class for <see cref="IConnectionListener"/> implementations.
    /// </summary>
    /// <typeparam name="TConnection"></typeparam>
    public abstract class ConnectionListenerBase<TConnection> : IConnectionListener<TConnection>
        where TConnection : IConnection
    {
        private readonly IObservable<IConnection> _connectionReceivedNonGeneric;

        protected readonly Subject<TConnection> _connectionReceived = new Subject<TConnection>();
        protected readonly AsyncLock _mutex = new AsyncLock(); // mutex protecting _status
        protected ConnectionListenerStatus _status = ConnectionListenerStatus.Inactive;

        public IObservable<TConnection> ConnectionReceived => _connectionReceived;

        IObservable<IConnection> IConnectionListener.ConnectionReceived => _connectionReceivedNonGeneric;

        public async Task DisposeAsync()
        {
            using (await _mutex.LockAsync())
            {
                if (_status != ConnectionListenerStatus.Disposed)
                {
                    await DisposeCoreAsync();
                    _connectionReceived.OnCompleted();
                    _status = ConnectionListenerStatus.Disposed;
                }
            }
        }

        public async Task StartAsync()
        {
            using (await _mutex.LockAsync())
            {
                if (_status == ConnectionListenerStatus.Disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                if (_status == ConnectionListenerStatus.Inactive)
                {
                    await StartCoreAsync();
                    _status = ConnectionListenerStatus.Active;
                }
            }
        }

        public ConnectionListenerBase()
        {
            _connectionReceivedNonGeneric = ConnectionReceived.Select(conn => (IConnection)conn);
        }

        // will be called at most once
        protected abstract Task DisposeCoreAsync();

        // will be called at most once
        protected abstract Task StartCoreAsync();
    }
}
