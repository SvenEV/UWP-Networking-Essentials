using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UwpNetworkingEssentials
{
    /// <summary>
    /// A simple serializer that converts objects to/from JSON representation.
    /// </summary>
    public class DefaultJsonSerializer : IObjectSerializer
    {
        private static readonly JsonSerializerSettings _defaultJsonSettings = new JsonSerializerSettings();

        private readonly Assembly _assembly;
        private readonly Dictionary<string, Type> _types;
        private readonly JsonSerializerSettings _jsonSettings;

        /// <summary>
        /// Initializes a new default serializer.
        /// </summary>
        /// <param name="assembly">
        /// The assembly that is used for type lookup and instantiation
        /// </param>
        /// <param name="jsonSettings">
        /// JSON serializer settings that are considered during object serialization.
        /// </param>
        public DefaultJsonSerializer(Assembly assembly, JsonSerializerSettings jsonSettings)
        {
            _assembly = assembly;
            _jsonSettings = jsonSettings ?? _defaultJsonSettings;
            _types = new[]
                {
                    _assembly,
                    GetType().GetTypeInfo().Assembly,
                    typeof(int).GetTypeInfo().Assembly
                }
                .Distinct()
                .SelectMany(asm => asm.GetTypes())
                .ToDictionary(t => t.FullName);
        }

        /// <summary>
        /// Initializes a new default serializer with default JSON
        /// serialization settings.
        /// </summary>
        /// <param name="assembly">
        /// The assembly that is used for type lookup and instantiation
        /// </param>
        public DefaultJsonSerializer(Assembly assembly) : this(assembly, _defaultJsonSettings)
        {
        }

        public string Serialize(object o) => JsonConvert.SerializeObject(new JsonMessage(o, _jsonSettings));

        public object Deserialize(string json)
        {
            var message = JsonConvert.DeserializeObject<JsonMessage>(json);

            var type = LookupType(message.TypeName);
            var genericParams = message.GenericTypeParameters.Select(LookupType).ToArray();
            var finalType = genericParams.Length == 0 ? type : type.MakeGenericType(genericParams);

            return JsonConvert.DeserializeObject(message.Value, finalType, _jsonSettings);
        }

        private Type LookupType(string name)
        {
            return (_types.TryGetValue(name, out var type))
                ? type
                : throw new ArgumentException($"The type '{name}' could not be found");
        }

        class JsonMessage
        {
            public string TypeName { get; set; }
            public string[] GenericTypeParameters { get; set; }
            public string Value { get; set; }

            public JsonMessage()
            {
            }

            public JsonMessage(object o, JsonSerializerSettings jsonSettings)
            {
                Value = JsonConvert.SerializeObject(o, jsonSettings);

                var type = o.GetType();

                if (type.IsConstructedGenericType)
                {
                    TypeName = type.GetGenericTypeDefinition().FullName;
                    GenericTypeParameters = o.GetType().GenericTypeArguments
                        .Select(t => t.FullName).ToArray();
                }
                else
                {
                    TypeName = type.FullName;
                    GenericTypeParameters = Array.Empty<string>();
                }
            }
        }
    }
}
