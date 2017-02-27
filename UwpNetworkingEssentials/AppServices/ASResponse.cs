using Windows.ApplicationModel.AppService;

namespace UwpNetworkingEssentials.AppServices
{
    public class ASResponse : ResponseBase<ASResponseStatus>
    {
        private readonly AppServiceResponse _internalResponse;

        public override ASResponseStatus Status { get; }

        public override object Message { get; }

        public ASResponse(AppServiceResponse internalResponse, IObjectSerializer serializer)
        {
            _internalResponse = internalResponse;
            Status = new ASResponseStatus(internalResponse.Status);

            if (internalResponse.Status == AppServiceResponseStatus.Success)
            {
                Message = serializer.DeserializeFromValueSet(internalResponse.Message);
            }
        }
    }
}
