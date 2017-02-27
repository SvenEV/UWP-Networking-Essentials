using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Windows.Storage.Streams;

namespace UwpNetworkingEssentials
{
    internal static class ExtensionMethods
    {
        public static string ToDescriptionString(this MethodInfo method)
            => $"{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})";
    }
}
