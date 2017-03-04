using Microsoft.VisualStudio.TestTools.UnitTesting;
using UwpNetworkingEssentials.Rpc;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Channels.DebugChannel;

namespace UwpNetworkingEssentials.Tests
{
    /// <summary>
    /// Tests the typed RPC API.
    /// Depending on the interface, RPC calls are synchronous or asynchronous.
    /// An RPC call to multiple endpoints must return null (or the default value corresponding to the declared return
    /// type), because we can't just return a collection of all results since this does not conform to the interface
    /// (this is in contrast to dynamic RPC).
    /// </summary>
    [TestClass]
    [TestCategory("Typed RPC")]
    public class RpcTypedTest
    {
        private RpcServer<IClientMethods> _server;
        private RpcConnection<IServerMethods> _client1;
        private RpcConnection<IServerMethods> _client2;

        [TestInitialize]
        public async Task Initialize()
        {
            var listener = new DebugConnectionListener();
            await listener.StartAsync();

            _server = new RpcServer<IClientMethods>(listener, new ServerCallTarget { Name = "Server" });

            _client1 = new RpcConnection<IServerMethods>(DebugConnection.Connect(listener),
                new ClientCallTarget { Name = "Client1" });

            _client2 = new RpcConnection<IServerMethods>(DebugConnection.Connect(listener),
                new ClientCallTarget { Name = "Client2" });
        }

        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual(4, _client1.Proxy.StringLength("Test"));
        }

        [TestMethod]
        public async Task TestMethod2()
        {
            Assert.AreEqual(3, await _client2.Proxy.StringLengthAsync("Foo"));
        }

        [TestMethod]
        public void TestMethod3()
        {
            // Dynamic result: object[] { 5, 5 }
            // Typed result: default(int) = 0
            Assert.AreEqual(0, _server.AllClients.StringLength("Hello"));
        }

        [TestMethod]
        public async Task TestMethod4()
        {
            // Dynamic result: object[] { 5, 5 }
            // Typed result: default(int) = 0
            Assert.AreEqual(0, await _server.AllClients.StringLengthAsync("Hello"));
        }

        [TestMethod]
        public void TestMethod5()
        {
            // Dynamic result: object[] { Customer, Customer }
            // Typed result: null
            Assert.IsNull(_server.AllClients.FindCustomer(new Point { X = 5, Y = 6 }));
        }

        [TestMethod]
        public void TestMethod6()
        {
            // Dynamic result: object[] { Customer, Customer }
            // Typed result: null
            Assert.IsNull(_server.ClientsExcept("foo").FindCustomer(new Point { X = 5, Y = 6 }));
        }

        [TestMethod]
        public async Task TestMethod7()
        {
            // Dynamic result: object[] { Customer, Customer }
            // Typed result: null
            Assert.IsNull(await _server.AllClients.FindCustomerAsync(new Point { X = 5, Y = 6 }));
        }

        [TestMethod]
        public async Task TestMethod8()
        {
            // Dynamic result: object[] { Customer, Customer }
            // Typed result: null
            Assert.IsNull(await _server.AllClients.FindCustomerAsync(new Point { X = 5 }, new Point { Y = 2 }));
        }
    }
}
