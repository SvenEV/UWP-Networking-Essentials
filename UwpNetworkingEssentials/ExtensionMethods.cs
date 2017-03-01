using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials
{
    internal static class ExtensionMethods
    {
        public static string ToDescriptionString(this MethodInfo method) =>
            $"{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})";

        public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
        {
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
                await task;
            else
                throw new TimeoutException();
        }

        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
                return await task;
            else
                throw new TimeoutException();
        }
    }
}
