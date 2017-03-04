using System.Threading.Tasks;

namespace UwpNetworkingEssentials.ChatSample.ViewModels
{
    public interface IServerInterface
    {
        Task BroadcastMessageAsync(string message);
    }
    
    public interface IClientInterface
    {
        void AddMessage(string message);
    }
}
