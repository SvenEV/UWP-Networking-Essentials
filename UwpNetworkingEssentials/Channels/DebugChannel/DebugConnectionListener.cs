using System;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials.Channels.DebugChannel
{
    public class DebugConnectionListener : ConnectionListenerBase<DebugConnection>
    {
        protected override Task StartCoreAsync() => Task.CompletedTask;

        protected override Task DisposeCoreAsync() => Task.CompletedTask;

        internal DebugConnection OnConnectionReceived()
        {
            var connectionId = "DEBUG_" + Guid.NewGuid();
            var serverToClientConnection = new DebugConnection(connectionId);
            _connectionReceived.OnNext(serverToClientConnection);
            return serverToClientConnection;
        }
    }
}
