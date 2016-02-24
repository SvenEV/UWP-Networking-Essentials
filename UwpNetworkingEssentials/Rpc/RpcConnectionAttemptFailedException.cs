using System;

namespace UwpNetworkingEssentials.Rpc
{
    public class RpcConnectionAttemptFailedException : Exception
    {
        public string RemoteHostName { get; }

        public string RemotePort { get; }

        public RpcConnectionAttemptFailedException(string remoteHostName, string remotePort, Exception innerException)
            : base($"Failed to connect to RPC server at '{remoteHostName}' (port {remotePort})", innerException)
        {
            RemoteHostName = remoteHostName;
            RemotePort = remotePort;
        }
    }
}
