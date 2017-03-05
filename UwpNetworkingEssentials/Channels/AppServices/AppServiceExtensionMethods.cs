using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace UwpNetworkingEssentials.Channels.AppServices
{
    public static class AppServiceExtensionMethods
    {
        internal static async Task<RequestResult> SendMessageAsync(this AppServiceConnection connection, object message,
            RequestOptions options, IObjectSerializer serializer)
        {
            var valueSet = serializer.SerializeToValueSet(message);
            var internalResponse = await connection.SendMessageAsync(valueSet).ContinueOnOtherContext();
            var success = internalResponse.Status == AppServiceResponseStatus.Success;

            var responseMessage = (success && options.IsResponseRequired)
                ? serializer.DeserializeFromValueSet(internalResponse.Message)
                : null;

            return new RequestResult(
                responseMessage,
                success ? RequestStatus.Success : RequestStatus.Failure,
                internalResponse.Status);
        }

        internal static async Task<RespondResult> SendResponseAsync(this AppServiceRequest request, object message,
            IObjectSerializer serializer)
        {
            var valueSet = serializer.SerializeToValueSet(message);
            var internalStatus = await request.SendResponseAsync(valueSet).ContinueOnOtherContext();
            var success = internalStatus == AppServiceResponseStatus.Success;
            return new RespondResult(success ? ResponseStatus.Success : ResponseStatus.Failure, internalStatus);
        }

        public static ValueSet SerializeToValueSet(this IObjectSerializer serializer, object o)
        {
            var data = serializer.Serialize(o);
            return new ValueSet
            {
                { "SerializedData", data }
            };
        }

        public static object DeserializeFromValueSet(this IObjectSerializer serializer, ValueSet valueSet)
        {
            if (valueSet.TryGetValue("SerializedData", out var dataObject) && dataObject is string data)
            {
                return serializer.Deserialize(data);
            }

            return null;
        }
    }
}
