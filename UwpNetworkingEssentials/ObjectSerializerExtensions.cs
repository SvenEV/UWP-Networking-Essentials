using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace UwpNetworkingEssentials
{
    public static class ObjectSerializerExtensions
    {
        /// <summary>
        /// Writes an object to a stream.
        /// </summary>
        /// <param name="o">Object</param>
        /// <param name="writer">Stream writer</param>
        /// <returns></returns>
        public static async Task SerializeToStreamAsync(this IObjectSerializer serializer, object o, DataWriter writer)
        {
            var data = serializer.Serialize(o);

            var bytes = Encoding.UTF8.GetBytes(data);
            writer.WriteUInt32((uint)bytes.Length);
            writer.WriteBytes(bytes);

            await writer.StoreAsync();
        }

        /// <summary>
        /// Reads an object from a stream.
        /// Asynchronously blocks until an object is available.
        /// </summary>
        /// <param name="reader">Stream reader</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The object</returns>
        public static async Task<object> DeserializeFromStreamAsync(this IObjectSerializer serializer,
            DataReader reader, CancellationToken cancellationToken)
        {
            await reader.LoadAsync(sizeof(uint)).AsTask(cancellationToken);
            var totalLength = reader.ReadUInt32();
            await reader.LoadAsync(totalLength).AsTask(cancellationToken);

            var data = reader.ReadString(totalLength);
            var value = serializer.Deserialize(data);
            return value;
            
        }
    }
}
