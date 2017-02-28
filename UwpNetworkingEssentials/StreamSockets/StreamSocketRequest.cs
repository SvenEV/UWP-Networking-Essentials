using Newtonsoft.Json;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials.StreamSockets
{
    public class StreamSocketRequest : RequestBase<StreamSocketResponseStatus>
    {
        private readonly StreamSocketConnection _connection;

        /// <summary>
        /// Request identifier. This value should be unique across all requests issued by the
        /// <see cref="StreamSocketConnection"/> that has sent this request.
        /// </summary>
        public int RequestId { get; }

        public override object Message { get; }

        public StreamSocketRequest(int id, object message, StreamSocketConnection connection)
        {
            RequestId = id;
            Message = message;
            _connection = connection;
        }

        protected override async Task<StreamSocketResponseStatus> SendResponseCoreAsync(object responseMessage)
        {
            await _connection.SendResponseAsync(RequestId, responseMessage);
            return new StreamSocketResponseStatus();
        }
    }

    internal class StreamSocketRequestMessage
    {
        public int RequestId { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public object Message { get; set; }
    }
}
