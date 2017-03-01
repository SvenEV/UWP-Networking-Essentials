using System.Threading.Tasks;

namespace UwpNetworkingEssentials.Channels.DebugChannel
{
    public class DebugRequest : RequestBase
    {
        private readonly DebugConnection _connection;

        public override object Message { get; }

        internal RequestResult Response { get; private set; }

        public DebugRequest(object message, DebugConnection connection)
        {
            Message = message;
            _connection = connection;
        }

        protected override Task<RespondResult> SendResponseCoreAsync(object message)
        {
            var status = new RespondResult(ResponseStatus.Success);
            Response = new RequestResult(message, RequestStatus.Success);
            return Task.FromResult(status);
        }
    }
}
