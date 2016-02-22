using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            var jsonLength = totalLength;
            await reader.LoadAsync(totalLength);

            var typeNameLength = reader.ReadUInt32();
            var typeName = reader.ReadString(typeNameLength);
            var type = LookupType(typeName);

            jsonLength -= 2 * sizeof(uint) + typeNameLength;

            // Read generic type parameters
            var genericArgsCount = reader.ReadUInt32();
            var typeArgs = new Type[genericArgsCount];

            for (var i = 0; i < genericArgsCount; i++)
            {
                var typeParamNameLength = reader.ReadUInt32();
                var typeParamName = reader.ReadString(typeParamNameLength);
                typeArgs[i] = LookupType(typeParamName);
                jsonLength -= sizeof(uint) + typeParamNameLength;
            }

            // Construct final type
            var finalType = (genericArgsCount == 0) ? type :
                type.MakeGenericType(typeArgs);

            // Read JSON and deserialize
            var json = reader.ReadString(jsonLength);
            var o = JsonConvert.DeserializeObject(json, finalType);
            return o;
        }

        public async Task SerializeAsync(object o, DataWriter writer)
        {
            var json = JsonConvert.SerializeObject(o);

            var typeName = o.GetType().FullName;
            var typeArgs = o.GetType().GenericTypeArguments.Select(t => t.FullName).ToArray();

            var totalLength =
                sizeof(uint) + typeName.Length + // Type name and its length
                sizeof(uint) + typeArgs.Length * sizeof(uint) + typeArgs.Sum(t => t.Length) + // Generic args
                json.Length;

            writer.WriteUInt32((uint)totalLength);
            writer.WriteUInt32((uint)typeName.Length);
            writer.WriteString(typeName);
            writer.WriteUInt32((uint)typeArgs.Length);

            foreach (var t in typeArgs)
            {
                writer.WriteUInt32((uint)t.Length);
                writer.WriteString(t);
            }

            writer.WriteString(json);

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
    }
}
