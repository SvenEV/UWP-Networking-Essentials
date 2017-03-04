using Newtonsoft.Json;
using System.Linq;

namespace UwpNetworkingEssentials.Rpc
{
    /// <summary>
    /// Represents a remote procedure call.
    /// </summary>
    internal class RpcCall
    {
        [JsonProperty]
        public string MethodName { get; set; }

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public object[] Arguments { get; set; }

        public RpcCall()
        {
        }

        public RpcCall(string methodName, object[] arguments)
        {
            MethodName = methodName;
            Arguments = arguments;
        }

        public override string ToString()
            => $"{MethodName}({string.Join(", ", Arguments.Select(p => p == null ? "null" : p.GetType().Name))})";
    }
}
