using Nito.AsyncEx;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;

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

        public static bool IsTaskTypeWithResult(this Type type)
        {
            return typeof(Task).IsAssignableFrom(type) &&
                type.GetTypeInfo().IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Task<>) &&
                type.GetGenericArguments()[0].Name != "VoidTaskResult";
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

        public static ConfiguredTaskAwaitable<T> ContinueOnOtherContext<T>(this IAsyncOperation<T> operation) =>
            operation.AsTask().ConfigureAwait(false);

        public static ConfiguredTaskAwaitable ContinueOnOtherContext(this IAsyncAction action) =>
            action.AsTask().ConfigureAwait(false);
    }
}
