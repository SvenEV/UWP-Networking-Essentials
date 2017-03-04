using System.Collections.Generic;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials.Tests
{

    public interface IMethods
    {
        void DoStuff();
        Task DoStuffAsync();
        int StringLength(string s);
        Task<int> StringLengthAsync(string s);
        Customer FindCustomer(Point p);
        Task<Customer> FindCustomerAsync(Point p, Point q = default(Point));
    }

    public interface IServerMethods : IMethods { }

    public interface IClientMethods : IMethods { }

    public class CallTargetBase : IMethods
    {
        public string Name { get; set; }

        public List<string> List { get; } = new List<string>();

        public void DoStuff() => List.Add(nameof(DoStuff));

        public async Task DoStuffAsync()
        {
            await Task.CompletedTask;
            List.Add(nameof(DoStuffAsync));
        }

        public Customer FindCustomer(Point p) => new Customer { Name = Name, Age = 20 };

        public async Task<Customer> FindCustomerAsync(Point p, Point q = default(Point))
        {
            await Task.CompletedTask;
            return new Customer { Name = Name, Age = 30 };
        }

        public int StringLength(string s) => s.Length;

        public async Task<int> StringLengthAsync(string s)
        {
            await Task.CompletedTask;
            return s.Length;
        }
    }

    public class ServerCallTarget : CallTargetBase, IServerMethods
    {
    }

    public class ClientCallTarget : CallTargetBase, IClientMethods
    {
    }

    public class Customer
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
