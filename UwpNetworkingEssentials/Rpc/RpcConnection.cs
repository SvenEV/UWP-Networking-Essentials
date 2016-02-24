using System.Threading.Tasks;

namespace UwpNetworkingEssentials.Rpc
{
    public class RpcConnection
    {
        internal readonly RpcProxy _proxy;

        internal StreamSocketConnection SocketConnection { get; }

        public string Id => SocketConnection.Id;

        public string LocalAddress => SocketConnection.Information.LocalAddress.ToString();
        public string LocalPort => SocketConnection.Information.LocalPort;
        public string RemoteAddress => SocketConnection.Information.RemoteAddress.ToString();
        public string RemotePort => SocketConnection.Information.RemotePort;

        public dynamic Proxy => _proxy;

        internal RpcConnection(StreamSocketConnection connection)
        {
            SocketConnection = connection;
            _proxy = new RpcProxy(connection);
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <returns></returns>
        public async Task DisposeAsync()
        {
            await SocketConnection.DisposeAsync();
        }
    }
}
