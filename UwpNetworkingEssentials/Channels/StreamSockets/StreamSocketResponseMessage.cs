using Newtonsoft.Json;

namespace UwpNetworkingEssentials.Channels.StreamSockets
{
    internal class StreamSocketResponseMessage
    {
        public int RequestId { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public object Message { get; set; }
    }
}
