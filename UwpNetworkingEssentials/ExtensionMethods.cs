using Nito.AsyncEx;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace UwpNetworkingEssentials
{
    internal static class ExtensionMethods
    {
        public static string ToDescriptionString(this MethodInfo method) =>
            $"{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})";

        public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
        {
            if (await Task.WhenAny(task, Task.Delay(timeout)).ContinueOnOtherContext() == task)
                await task.ContinueOnOtherContext();
            else
                throw new TimeoutException();
        }

        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            if (await Task.WhenAny(task, Task.Delay(timeout)).ContinueOnOtherContext() == task)
                return await task.ContinueOnOtherContext();
            else
                throw new TimeoutException();
        }
    }
}

namespace System.Threading.Tasks
{ 
    public static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable ContinueOnOtherContext(this Task task) =>
            task.ConfigureAwait(false);

        public static ConfiguredTaskAwaitable<T> ContinueOnOtherContext<T>(this Task<T> task) =>
            task.ConfigureAwait(false);

        public static ConfiguredTaskAwaitable<T> ContinueOnOtherContext<T>(this AwaitableDisposable<T> task)
            where T : IDisposable => task.ConfigureAwait(false);
    }
}
