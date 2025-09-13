using System;
using System.Threading.Tasks;
using UnityEditor.Search;
using UnityEngine;

namespace Unity.AI.Assistant.Editor.Utils
{
    static class TaskUtils
    {
        // Log exceptions for any 'fire and forget' functions (ie. not using await)
        internal static Task WithExceptionLogging(this Task task, Action<Exception> exceptionHandler = null)
        {
            if (task != null && (!task.IsCompleted || task.IsFaulted))
            {
                var stackTrace = System.Environment.StackTrace;
                _ = LogExceptionTask(task, exceptionHandler, stackTrace);
            }

            return task;
        }
        async static Task LogExceptionTask(Task task, Action<Exception> exceptionHandler, string sourceStack)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                var combinedException = new AggregateException(ex.Message, ex, new Exception($"Originating thread callstack: {sourceStack}"));
                InternalLog.LogException(combinedException);
                exceptionHandler?.Invoke(combinedException);
            }
        }

        internal static void DispatchToMainThread(Action action)
        {
            Dispatcher.Enqueue(() =>
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    InternalLog.LogException(e);
                }
            });
        }
    }
}
