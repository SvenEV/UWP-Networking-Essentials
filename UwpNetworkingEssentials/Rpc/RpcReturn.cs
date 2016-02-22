namespace UwpNetworkingEssentials.Rpc
{
    /// <summary>
    /// Represents the result of a remote procedure call.
    /// </summary>
    internal class RpcReturn
    {
        public object Value { get; set; }

        public bool IsSuccessful { get; set; }

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
