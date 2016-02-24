using Newtonsoft.Json;

namespace UwpNetworkingEssentials.Rpc
{
    /// <summary>
    /// Represents the result of a remote procedure call.
    /// </summary>
    internal class RpcReturn
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public object Value { get; set; }

        [JsonProperty]
        public bool IsSuccessful { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Error { get; set; }

        public RpcReturn()
        {
        }

        private RpcReturn(object value, bool isSuccessful, string errorMessage)
        {
            Value = value;
            IsSuccessful = isSuccessful;
            Error = errorMessage;
        }

        public static RpcReturn Success(object returnValue = null)
        {
            return new RpcReturn(returnValue, true, "");
        }

        public static RpcReturn Faulted(string errorMessage)
        {
            return new RpcReturn(null, false, errorMessage);
        }
    }
}
