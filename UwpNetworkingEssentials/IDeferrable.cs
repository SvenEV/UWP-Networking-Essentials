using System;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials
{
    public interface IDeferrable
    {
        IDisposable GetDeferral();

        Task WaitForDeferralsAsync();
    }
}
