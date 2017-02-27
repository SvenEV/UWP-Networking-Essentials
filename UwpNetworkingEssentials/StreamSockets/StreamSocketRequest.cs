using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials.StreamSockets
{
    public class StreamSocketRequest : RequestBase<StreamSocketResponseStatus>
    {
        private readonly StreamSocketConnection _connection;
        private bool _hasResponded;

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

        public override async Task<StreamSocketResponseStatus> SendResponseAsync(object responseMessage)
        {
            if (_hasResponded)
                throw new InvalidOperationException("This request has already been responded to");

            await _connection.SendResponseAsync(RequestId, responseMessage);
            _hasResponded = true;

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
