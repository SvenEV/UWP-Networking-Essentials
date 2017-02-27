using Newtonsoft.Json;

namespace UwpNetworkingEssentials.StreamSockets
{
    public class StreamSocketResponse : IResponse
    {
        public object Message { get; }

        public IResponseStatus Status { get; }

        public StreamSocketResponse(object message)
        {
            Message = message;
            Status = new StreamSocketResponseStatus();
        }
    }

    internal class StreamSocketResponseMessage
    {
        public int RequestId { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public object Message { get; set; }
    }
}
