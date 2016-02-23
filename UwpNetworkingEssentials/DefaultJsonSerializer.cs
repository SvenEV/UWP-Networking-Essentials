using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace UwpNetworkingEssentials
{
    /// <summary>
    /// A simple serializer that converts objects to/from JSON representation.
    /// </summary>
    /// <remarks>
    /// Packet structure:
    /// - Total length       : uint   (total length of the packet without the "total length" uint)
    /// - Type name length   : uint
    /// - Type name          : string
    /// - Generic args count : uint
    /// - For each generic parameter
    ///   - Type name length : uint
    ///   - Type name        : string
    /// - Data               : JSON
    /// </remarks>
    public class DefaultJsonSerializer : IObjectSerializer
    {
        private readonly Assembly _assembly;
        private readonly Dictionary<string, Type> _types;

        /// <summary>
        /// Initializes a new default serializer.
        /// </summary>
        /// <param name="assembly">
        /// The assembly that is used for type lookup and instantiation
        /// </param>
        public DefaultJsonSerializer(Assembly assembly)
        {
            _assembly = assembly;
            _types = new[] { _assembly, GetType().GetTypeInfo().Assembly }
                .Distinct()
                .SelectMany(asm => asm.GetTypes())
                .ToDictionary(t => t.FullName);
        }

        public async Task<object> DeserializeAsync(DataReader reader)
        {
            await reader.LoadAsync(sizeof(uint));
            var totalLength = reader.ReadUInt32();
            await reader.LoadAsync(totalLength);

            var json = reader.ReadString(totalLength);
            var message = JsonConvert.DeserializeObject<Message>(json);

            var type = LookupType(message.TypeName);
            var genericParams = message.GenericTypeParameters.Select(LookupType).ToArray();
            var finalType = genericParams.Length == 0 ? type : type.MakeGenericType(genericParams);

            var value = JsonConvert.DeserializeObject(message.Value, finalType);
            return value;
        }

        public async Task SerializeAsync(object o, DataWriter writer)
        {
            var json = JsonConvert.SerializeObject(new Message(o));

            var bytes = Encoding.UTF8.GetBytes(json);
            writer.WriteUInt32((uint)bytes.Length);
            writer.WriteBytes(bytes);
            
            await writer.StoreAsync();
        }

        private Type LookupType(string name)
        {
            Type type;

            if (_types.TryGetValue(name, out type))
                return type;
            else
                throw new ArgumentException($"The type '{name}' could not be found");
        }

        class Message
        {
            public string TypeName { get; set; }
            public string[] GenericTypeParameters { get; set; }
            public string Value { get; set; }

            public Message()
            {
            }

            public Message(object value)
            {
                Value = JsonConvert.SerializeObject(value);
                TypeName = value.GetType().FullName;
                GenericTypeParameters = value.GetType().GenericTypeArguments
                    .Select(t => t.FullName).ToArray();
            }
        }
    }
}
