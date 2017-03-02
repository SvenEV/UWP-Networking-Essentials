using System;
using System.Collections.Generic;
using System.Threading;

namespace UwpNetworkingEssentials.Rpc
{
    /// <summary>
    /// Allows RPC method implementations to obtain useful information about the currently executed RPC method call,
    /// such as the connection from which the RPC call was issued (the "caller" endpoint).
    /// </summary>
    public sealed class RpcCallContext
    {
        private static readonly AsyncLocal<RpcCallContext> _context = new AsyncLocal<RpcCallContext>();

        /// <summary>
        /// Obtains an <see cref="RpcCallContext"/> for the current RPC call.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The current execution is not taking place in the context of an RPC call.
        /// </exception>
        public static RpcCallContext Current => _context.Value ?? throw new InvalidOperationException(
            "The current execution is not taking place in the context of an RPC call");

        public static bool TryGetCurrent(out RpcCallContext context)
        {
            context = _context.Value;
            return context != null;
        }

        internal static RpcCallContext Register(RpcCall call, RpcConnection connection)
        {
            var context = new RpcCallContext(call, connection);
            _context.Value = context;
            return context;
        }


        // instance members

        private readonly RpcCall _call;
        private readonly RpcConnection _connection;

        public string MethodName => _call.MethodName;

        public IReadOnlyList<object> Arguments => _call.Parameters;

        public RpcConnection Connection => _connection;

        private RpcCallContext(RpcCall call, RpcConnection connection)
        {
            _call = call;
            _connection = connection;
        }
    }
}
