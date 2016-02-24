using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials.Rpc
{
    public class RpcClient : IRpcClient
    {
        private readonly RpcConnection _connection;
        private readonly object _rpcTarget;

        /// <inheritdoc/>
        public RpcConnection Connection => _connection;

        /// <inheritdoc/>
        public dynamic Server => _connection.Proxy;

        private RpcClient(RpcConnection connection, object rpcTarget)
        {
            _connection = connection;
            _rpcTarget = rpcTarget;
        }

        /// <summary>
        /// Connects to an <see cref="RpcServer"/>.
        /// </summary>
        /// <param name="hostName">Remote host name</param>
        /// <param name="port">Remote port</param>
        /// <param name="rpcTarget">
        /// The object where remote procedure calls from the server
        /// are executed on. Set this to null if you do not want to allow
        /// RPC calls from the server to the client.
        /// </param>
        /// <param name="serializer">Serializer</param>
        /// <returns></returns>
        public static async Task<RpcClient> ConnectAsync(string hostName, string port, object rpcTarget, IObjectSerializer serializer)
        {
            try
            {
                var connection = await StreamSocketConnection.ConnectAsync(hostName, port, serializer);

                if (connection == null)
                    return null;

                var rpcConnection = new RpcConnection(connection);
                var client = new RpcClient(rpcConnection, rpcTarget);

                connection.ObjectReceived.OfType<RpcCall>().Subscribe(client.OnCall);
                connection.ObjectReceived.OfType<StreamSocketConnectionCloseMessage>().Subscribe(__ => client.OnDisconnected());

                (rpcTarget as IRpcTarget)?.OnConnected(rpcConnection);

                return new RpcClient(rpcConnection, rpcTarget);
            }
            catch (Exception exception)
            {
                var rpcException = new RpcConnectionAttemptFailedException(hostName, port, exception);
                (rpcTarget as IRpcTarget)?.OnConnectionAttemptFailed(rpcException);
                throw rpcException;
            }
        }

        private void OnCall(RpcCall call)
        {
            RpcHelper.HandleMethodCall(_connection, call, _rpcTarget);
        }

        private void OnDisconnected()
        {
            (_rpcTarget as IRpcTarget)?.OnDisconnected(_connection);
        }

        /// <inheritdoc/>
        public async Task DisposeAsync()
        {
            await _connection.DisposeAsync();
        }
    }
}