using Microsoft.VisualStudio.TestTools.UnitTesting;
using UwpNetworkingEssentials.Rpc;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Channels.DebugChannel;
using System.Linq;

namespace UwpNetworkingEssentials.Tests
{
    /// <summary>
    /// Tests the dynamic RPC API.
    /// Note that, as opposed to typed RPC, all RPC calls are asynchronous in dynamic RPC.
    /// </summary>
    [TestClass]
    [TestCategory("Dynamic RPC")]
    public class RpcDynamicTest
    {
        private RpcServer _server;
        private RpcConnection _client1;
        private RpcConnection _client2;

        [TestInitialize]
        public async Task Initialize()
        {
            var listener = new DebugConnectionListener();
            await listener.StartAsync();

            _server = new RpcServer(listener, new ServerCallTarget { Name = "Server" });

            _client1 = new RpcConnection(DebugConnection.Connect(listener),
                new ClientCallTarget { Name = "Client1" });

            _client2 = new RpcConnection(DebugConnection.Connect(listener),
                new ClientCallTarget { Name = "Client2" });
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            Assert.AreEqual(4, await _client1.Proxy.StringLength("Test"));
        }

        [TestMethod]
        public async Task TestMethod2()
        {
            Assert.AreEqual(3, await _client2.Proxy.StringLengthAsync("Foo"));
        }

        [TestMethod]
        public async Task TestMethod3()
        {
            // Dynamic result: object[] { 5, 5 }
            // Typed result: default(int) = 0
            object[] result = await _server.AllClients.StringLength("Hello");
            Assert.IsTrue(result.SequenceEqual(new object[] { 5, 5 }));
        }

        [TestMethod]
        public async Task TestMethod4()
        {
            // Dynamic result: object[] { 5, 5 }
            // Typed result: default(int) = 0

            // note: 'var' does not work here because... dynamic magic!?
            object[] result = await _server.AllClients.StringLengthAsync("Hello");
            Assert.IsTrue(result.SequenceEqual(new object[] { 5, 5 }));
        }

        [TestMethod]
        public async Task TestMethod5()
        {
            // Dynamic result: object[] { Customer, Customer }
            // Typed result: null
            var result = await _server.AllClients.FindCustomer(new Point { X = 5, Y = 6 });
            Assert.AreEqual(typeof(object[]), result?.GetType());
            Assert.AreEqual(2, ((object[])result).OfType<Customer>().Count());
        }

        [TestMethod]
        public async Task TestMethod6()
        {
            // Dynamic result: object[] { Customer, Customer }
            // Typed result: null
            var result = await _server.ClientsExcept("foo").FindCustomer(new Point { X = 5, Y = 6 });
            Assert.AreEqual(typeof(object[]), result?.GetType());
            Assert.AreEqual(2, ((object[])result).OfType<Customer>().Count());
        }

        [TestMethod]
        public async Task TestMethod7()
        {
            // Dynamic result: object[] { Customer, Customer }
            // Typed result: null
            var result = await _server.AllClients.FindCustomerAsync(new Point { X = 5, Y = 6 });
            Assert.AreEqual(typeof(object[]), result?.GetType());
            Assert.AreEqual(2, ((object[])result).OfType<Customer>().Count());
        }

        [TestMethod]
        public async Task TestMethod8()
        {
            // Dynamic result: object[] { Customer, Customer }
            // Typed result: null
            var result = await _server.AllClients.FindCustomerAsync(new Point { X = 5 }, new Point { Y = 2 });
            Assert.AreEqual(typeof(object[]), result?.GetType());
            Assert.AreEqual(2, ((object[])result).OfType<Customer>().Count());
        }
    }
}
