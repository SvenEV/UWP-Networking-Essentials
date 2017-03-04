using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;

namespace UwpNetworkingEssentials.Channels.AppServices
{
    public class ASRequest : RequestBase
    {
        private readonly ASConnection _connection;
        private readonly AppServiceRequest _internalRequest;
        private readonly AppServiceDeferral _internalRequestDeferral;

        public override object Message { get; }

        public ASRequest(AppServiceRequest internalRequest, AppServiceDeferral internalRequestDeferral,
            ASConnection connection)
        {
            _connection = connection;
            _internalRequest = internalRequest;
            _internalRequestDeferral = internalRequestDeferral;
            Message = _connection._serializer.DeserializeFromValueSet(internalRequest.Message);
        }

        protected override async Task<RespondResult> SendResponseCoreAsync(object message)
        {
            var result = await _internalRequest
                .SendResponseAsync(message, _connection._serializer)
                .ContinueOnOtherContext();

            _internalRequestDeferral.Complete();
            return result;
        }
    }
}
