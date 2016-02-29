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
        public object[] Parameters { get; set; }

        public RpcCall()
        {
        }

        public RpcCall(string methodName, object[] parameters)
        {
            MethodName = methodName;
            Parameters = parameters;
        }

        public override string ToString()
            => $"{MethodName}({string.Join(", ", Parameters.Select(p => p == null ? "null" : p.GetType().Name))})";
    }
}
