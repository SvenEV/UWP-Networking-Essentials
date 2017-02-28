namespace UwpNetworkingEssentials
{
    public interface IResponse
    {
        object Message { get; }

        IResponseStatus Status { get; }
    }

    public interface IResponse<TResponseStatus> : IResponse where TResponseStatus : IResponseStatus
    {
        new TResponseStatus Status { get; }
    }

    public abstract class ResponseBase<TResponseStatus> : IResponse<TResponseStatus>
        where TResponseStatus : IResponseStatus
    {
        public abstract object Message { get; }

        public abstract TResponseStatus Status { get; }

        IResponseStatus IResponse.Status => Status;
    }
}
