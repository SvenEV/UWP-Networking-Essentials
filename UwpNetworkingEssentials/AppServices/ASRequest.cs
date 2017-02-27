using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;

namespace UwpNetworkingEssentials.AppServices
{
    public class ASRequest : RequestBase<ASResponseStatus>
    {
        private readonly ASConnection _connection;
        private readonly AppServiceRequest _internalRequest;
        private readonly AppServiceDeferral _deferral;

        public override object Message { get; }

        public ASRequest(AppServiceRequest internalRequest, AppServiceDeferral deferral, ASConnection connection)
        {
            _connection = connection;
            _internalRequest = internalRequest;
            _deferral = deferral;
            Message = _connection._serializer.DeserializeFromValueSet(internalRequest.Message);
        }

        public override async Task<ASResponseStatus> SendResponseAsync(object message)
        {
            var result = await _internalRequest.SendResponseAsync(message, _connection._serializer);
            _deferral.Complete();
            return result;
        }
    }
}
