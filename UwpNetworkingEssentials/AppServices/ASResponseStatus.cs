using Windows.ApplicationModel.AppService;

namespace UwpNetworkingEssentials.AppServices
{
    public class ASResponseStatus : IResponseStatus
    {
        public AppServiceResponseStatus Code { get; }

        public ASResponseStatus(AppServiceResponseStatus code)
        {
            Code = code;
        }
    }
}
