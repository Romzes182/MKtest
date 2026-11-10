global using MKtest.Compatibility;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Compatibility
{
    internal static class FrameworkCompatibility
    {
        public static int Clamp(int value, int min, int max)
        {
            if (min > max) throw new ArgumentException("Minimum exceeds maximum.");
            return Math.Min(Math.Max(value, min), max);
        }

        public static bool Contains(this string source, string value, StringComparison comparison) =>
            source.IndexOf(value, comparison) >= 0;

        // Cancel only the wait, as Task.WaitAsync does; the producer owns its task.
        public static async Task WaitAsync(this Task task, CancellationToken token)
        {
            if (task.IsCompleted || !token.CanBeCanceled)
            {
                await task.ConfigureAwait(false);
                return;
            }
            token.ThrowIfCancellationRequested();
            var canceled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (token.Register(() => canceled.TrySetCanceled(token)))
            {
                var completed = await Task.WhenAny(task, canceled.Task).ConfigureAwait(false);
                await completed.ConfigureAwait(false);
            }
        }

        public static async Task<T> WaitAsync<T>(this Task<T> task, CancellationToken token)
        {
            await ((Task)task).WaitAsync(token).ConfigureAwait(false);
            return await task.ConfigureAwait(false);
        }
    }
}
