using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Windows.Storage.Streams;

namespace UwpNetworkingEssentials
{
    internal static class ExtensionMethods
    {
        public static void WriteStringWithLength(this DataWriter writer, string s)
        {
            if (writer.UnicodeEncoding != Windows.Storage.Streams.UnicodeEncoding.Utf8)
                throw new ArgumentException("Encoding must be UTF8", nameof(writer));

            var bytes = Encoding.UTF8.GetBytes(s);
            writer.WriteUInt32((uint)bytes.Length);
            writer.WriteBytes(bytes);
        }

        public static string ToDescriptionString(this MethodInfo method)
            => $"{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})";
    }
}
