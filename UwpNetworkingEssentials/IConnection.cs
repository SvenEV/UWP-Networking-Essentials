using System;
using System.Reactive.Linq;
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
        /// The event that is raised when an object is
        /// received.
        /// </summary>
        IObservable<IRequest> RequestReceived { get; }

        /// <summary>
        /// The event that is raised when the connection
        /// is closed.
        /// </summary>
        IObservable<IDisconnectEventArgs> Disconnected { get; }

        /*
        /// <summary>
        /// Sends an object.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        [Obsolete("Replaced by SendMessageAsync(object)", true)]
        Task SendAsync(object o);

        /// <summary>
        /// Sends a request message, waits until a response message
        /// of the specified type is received, and returns that
        /// response message.
        /// </summary>
        /// <typeparam name="TResponse">Type of the response message</typeparam>
        /// <param name="requestObject">The request message</param>
        /// <returns>Response message</returns>
        [Obsolete("Replaced by SendMessageAsync(object)", true)]
        Task<TResponse> RequestAsync<TResponse>(object requestObject);
        */
        

        Task<IResponse> SendMessageAsync(object message);

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <returns></returns>
        Task DisposeAsync();
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

    public abstract class ConnectionBase<TRequest, TResponse, TDisconnectEventArgs> : IConnection<TRequest, TResponse, TDisconnectEventArgs>
        where TRequest : IRequest
        where TResponse : IResponse
        where TDisconnectEventArgs : IDisconnectEventArgs
    {
        private readonly IObservable<IRequest> _requestReceivedNonGeneric;
        private readonly IObservable<IDisconnectEventArgs> _disconnectedNonGeneric;

        public abstract string Id { get; }

        public abstract IObservable<TRequest> RequestReceived { get; }

        IObservable<IRequest> IConnection.RequestReceived => _requestReceivedNonGeneric;

        public abstract IObservable<TDisconnectEventArgs> Disconnected { get; }

        IObservable<IDisconnectEventArgs> IConnection.Disconnected => _disconnectedNonGeneric;

        public abstract Task DisposeAsync();

        public abstract Task<TResponse> SendMessageAsync(object message);

        async Task<IResponse> IConnection.SendMessageAsync(object message) => await SendMessageAsync(message);

        public ConnectionBase()
        {
            _requestReceivedNonGeneric = RequestReceived.Select(r => (IRequest)r);
            _disconnectedNonGeneric = Disconnected.Select(e => (IDisconnectEventArgs)e);
        }
    }
}
