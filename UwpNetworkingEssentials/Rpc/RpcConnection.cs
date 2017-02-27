using System;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials.Rpc
{
    public class RpcConnection
    {
        internal readonly RpcProxy _proxy;

        public IConnection UnderlyingConnection { get; }

        public string Id => UnderlyingConnection.Id;
        
        public dynamic Proxy => _proxy;

        internal RpcConnection(IConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            UnderlyingConnection = connection;
            _proxy = new RpcProxy(connection);
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <returns></returns>
        public async Task DisposeAsync()
        {
            await UnderlyingConnection.DisposeAsync();
        }
    }
}
