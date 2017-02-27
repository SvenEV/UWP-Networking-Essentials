using System;
using System.Reactive.Linq;
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
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        /// Stops listening for incoming connections.
        /// After disposal the listener cannot be started again.     TODO: Does this make sense?
        /// </summary>
        /// <returns></returns>
        Task DisposeAsync();
    }

    public interface IConnectionListener<TConnection> : IConnectionListener where TConnection : IConnection
    {
        new IObservable<TConnection> ConnectionReceived { get; }
    }

    public abstract class ConnectionListenerBase<TConnection> : IConnectionListener<TConnection> where TConnection : IConnection
    {
        private readonly IObservable<IConnection> _connectionReceivedNonGeneric;

        public abstract IObservable<TConnection> ConnectionReceived { get; }

        IObservable<IConnection> IConnectionListener.ConnectionReceived => _connectionReceivedNonGeneric;

        public abstract Task DisposeAsync();

        public abstract Task StartAsync();

        public ConnectionListenerBase()
        {
            _connectionReceivedNonGeneric = ConnectionReceived.Select(conn => (IConnection)conn);
        }
    }
}
