using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace UwpNetworkingEssentials
{
    public interface IObjectSerializer
    {
        /// <summary>
        /// Writes an object to a stream.
        /// </summary>
        /// <param name="o">Object</param>
        /// <param name="writer">Stream writer</param>
        /// <returns></returns>
        Task SerializeAsync(object o, DataWriter writer);

        /// <summary>
        /// Reads an object from a stream.
        /// Asynchronously blocks until an object is available.
        /// </summary>
        /// <param name="reader">Stream reader</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The object</returns>
        Task<object> DeserializeAsync(DataReader reader, CancellationToken cancellationToken);
    }
}
