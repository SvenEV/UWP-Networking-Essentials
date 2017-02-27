using System.Threading.Tasks;

namespace UwpNetworkingEssentials
{
    public interface IRequest
    {
        object Message { get; }

        Task<IResponseStatus> SendResponseAsync(object message);
    }

    public interface IRequest<TResponseStatus> : IRequest where TResponseStatus : IResponseStatus
    {
        new Task<TResponseStatus> SendResponseAsync(object message);
    }

    public abstract class RequestBase<TResponseStatus> : IRequest<TResponseStatus> where TResponseStatus : IResponseStatus
    {
        public abstract object Message { get; }

        public abstract Task<TResponseStatus> SendResponseAsync(object message);

        async Task<IResponseStatus> IRequest.SendResponseAsync(object message) => await SendResponseAsync(message);
    }
}
