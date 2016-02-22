using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace UwpNetworkingEssentials.Rpc
{
    public class RpcServer : IRpcServer
    {
        private readonly IObjectSerializer _serializer;
        private readonly StreamSocketListener _listener;
        private readonly Dictionary<string, RpcConnection> _connections = new Dictionary<string, RpcConnection>();
        private readonly object _rpcTarget;

        public IReadOnlyDictionary<string, RpcConnection> Connections => _connections;

        public dynamic AllClients => new RpcMultiProxy(_connections.Values.Select(c => c._proxy));

        public string Port => _listener.Information.LocalPort;

        private RpcServer(StreamSocketListener listener, object rpcTarget, IObjectSerializer serializer)
        {
            _serializer = serializer;
            _listener = listener;
            _rpcTarget = rpcTarget;
        }

        /// <summary>
        /// Starts an <see cref="RpcServer"/>.
        /// </summary>
        /// <param name="port">Port to listen on</param>
        /// <param name="rpcTarget">
        /// The object where remote procedure calls from clients are
        /// executed on. Set this to null if you do not want to allow
        /// RPC calls from clients to the server.
        /// </param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static async Task<RpcServer> StartAsync(string port, object rpcTarget, IObjectSerializer serializer)
        {
            var listener = new StreamSocketListener();
            var server = new RpcServer(listener, rpcTarget, serializer);
            listener.ConnectionReceived += server.OnConnectionReceived;
            await listener.BindServiceNameAsync(port);
            return server;
        }

        public dynamic Client(string connectionId)
        {
            return _connections[connectionId].Proxy;
        }

        public dynamic ClientsExcept(string connectionId)
        {
            return new RpcMultiProxy(_connections.Values.Where(c => c.Id != connectionId).Select(c => c._proxy));
        }

        private async void OnConnectionReceived(StreamSocketListener _, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                var connection = await StreamSocketConnection.AcceptConnectionAsync(args.Socket, _serializer);

                if (connection != null)
                {
                    var rpcConnection = new RpcConnection(connection);
                    connection.ObjectReceived.OfType<RpcCall>().Subscribe(call => OnCall(rpcConnection, call));
                    connection.ObjectReceived.OfType<StreamSocketConnectionCloseMessage>().Subscribe(__ => OnClientDisconnected(rpcConnection));

                    _connections.Add(connection.Id, rpcConnection);

                    (_rpcTarget as IRpcTarget)?.OnConnected(rpcConnection);
                }
            }
            catch
            {
                // What now?
                Debugger.Break();
            }
        }

        private void OnCall(RpcConnection connection, RpcCall call)
        {
            RpcHelper.HandleMethodCall(connection, call, _rpcTarget);
        }

        private void OnClientDisconnected(RpcConnection connection)
        {
            (_rpcTarget as IRpcTarget)?.OnDisconnected(connection);
        }
    }
}
