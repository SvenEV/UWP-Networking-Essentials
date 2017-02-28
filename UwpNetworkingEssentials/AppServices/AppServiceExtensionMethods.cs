using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace UwpNetworkingEssentials.AppServices
{
    public static class AppServiceExtensionMethods
    {
        internal static async Task<ASResponse> SendMessageAsync(this AppServiceConnection connection, object message,
            IObjectSerializer serializer)
        {
            var valueSet = serializer.SerializeToValueSet(message);
            var internalResponse = await connection.SendMessageAsync(valueSet);
            return new ASResponse(internalResponse, serializer);
        }

        internal static async Task<ASResponseStatus> SendResponseAsync(this AppServiceRequest request, object message,
            IObjectSerializer serializer)
        {
            var valueSet = serializer.SerializeToValueSet(message);
            var status = await request.SendResponseAsync(valueSet);
            return new ASResponseStatus(status);
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
