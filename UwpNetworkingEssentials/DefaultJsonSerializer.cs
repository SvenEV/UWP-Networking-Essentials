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

        private readonly Dictionary<string, Type> _types;
        private readonly JsonSerializerSettings _jsonSettings;

        /// <summary>
        /// Initializes a new default serializer.
        /// </summary>
        /// <param name="assemblies">The assemblies that are used for type lookup and instantiation</param>
        /// <param name="jsonSettings">JSON serializer settings that are considered during object serialization</param>
        public DefaultJsonSerializer(IEnumerable<Assembly> assemblies, JsonSerializerSettings jsonSettings)
        {
            _jsonSettings = jsonSettings ?? _defaultJsonSettings;
            _types = assemblies
                .Concat(new[] { GetType().GetTypeInfo().Assembly, typeof(int).GetTypeInfo().Assembly })
                .Distinct()
                .SelectMany(asm => asm.GetTypes())
                .ToDictionary(t => t.FullName);
        }

        /// <summary>
        /// Initializes a new default serializer with default JSON serialization settings.
        /// </summary>
        /// <param name="assemblies">The assemblies that are used for type lookup and instantiation</param>
        public DefaultJsonSerializer(IEnumerable<Assembly> assemblies) : this(assemblies, _defaultJsonSettings)
        {
        }

        /// <summary>
        /// Initializes a new default serializer with default JSON serialization settings.
        /// </summary>
        /// <param name="assemblies">The assemblies that are used for type lookup and instantiation</param>
        public DefaultJsonSerializer(params Assembly[] assemblies) : this(assemblies, _defaultJsonSettings)
        {
        }

        public string Serialize(object o) => JsonConvert.SerializeObject(new JsonMessage(o, _jsonSettings));

        public object Deserialize(string json)
        {
            var message = JsonConvert.DeserializeObject<JsonMessage>(json);

            if (message.IsEmpty)
                return null;

            var type = LookupType(message.TypeName);
            var genericParams = message.GenericTypeArguments.Select(LookupType).ToArray();
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
            public string[] GenericTypeArguments { get; set; }
            public string Value { get; set; }

            public bool IsEmpty => TypeName == null || Value == null;

            public JsonMessage()
            {
            }

            public JsonMessage(object o, JsonSerializerSettings jsonSettings)
            {
                if (o == null)
                    return;

                Value = JsonConvert.SerializeObject(o, jsonSettings);

                var type = o.GetType();

                if (type.IsConstructedGenericType)
                {
                    TypeName = type.GetGenericTypeDefinition().FullName;
                    GenericTypeArguments = o.GetType().GenericTypeArguments
                        .Select(t => t.FullName).ToArray();
                }
                else
                {
                    TypeName = type.FullName;
                    GenericTypeArguments = Array.Empty<string>();
                }
            }
        }
    }
}
