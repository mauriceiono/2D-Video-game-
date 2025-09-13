using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Unity.AI.Toolkit
{
    /// <summary>
    /// Manages asynchronous operations in the Unity Editor. This utility is specifically designed to be robust
    /// in complex editor states, such as when the editor is paused in play mode or out of focus.
    /// It achieves this by using `EditorApplication.update` as a "heartbeat" to pump the async machinery,
    /// ensuring that continuations can execute when the standard game loop is frozen.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class EditorTask
    {
        // This constant can be adjusted as needed.
        const int k_AbandonmentTimeoutMilliseconds = 5000;

        /// <summary>
        /// Extension method for Task. Awaits the task ensuring its direct continuation
        /// does not capture the Unity synchronization context, then ensures the
        /// final continuation (after this awaitable) runs on the main Unity thread.
        /// Returns a standard Task that completes on the main thread.
        /// </summary>
        public static async Task ConfigureAwaitMainThread(this Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (EditorThread.isMainThread && !isPlayingPaused && EditorAsyncKeepAliveScope.isFocused)
            {
                await task;
                return;
            }

            await task.ConfigureAwait(false);
            await EditorThread.EnsureMainThreadAsync();
        }

        /// <summary>
        /// Extension method for Task(TResult). Awaits the task ensuring its direct continuation
        /// does not capture the Unity synchronization context, then ensures the
        /// final continuation (after this awaitable) runs on the main Unity thread.
        /// Returns a standard Task(TResult) whose result is available on the main thread.
        /// </summary>
        public static async Task<TResult> ConfigureAwaitMainThread<TResult>(this Task<TResult> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (EditorThread.isMainThread && !isPlayingPaused && EditorAsyncKeepAliveScope.isFocused)
                return await task;

            var result = await task.ConfigureAwait(false);
            await EditorThread.EnsureMainThreadAsync();
            return result;
        }

        /// <summary>
        /// Editor is playing and paused
        /// </summary>
        public static bool isPlayingPaused
        {
            get
            {
                try { return EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isPaused; }
                catch { return false; }
            }
        }

        /// <summary>
        /// Yield and return to the main thread. Important in paused play mode.
        /// </summary>
        public static Task Yield()
        {
            // commented out because it doesn't seem reliable, especially on startup
            //if (EditorThread.isMainThread && !isPlayingPaused && EditorAsyncKeepAliveScope.isFocused)
            //    return YieldAsync();

            var batchMode = false;
            try { batchMode = Application.isBatchMode; }
            catch (UnityException) { /* ignored */ }

            return batchMode ? YieldAsync() : Delay(1);
        }

        static async Task YieldAsync() => await Task.Yield();

        /// <summary>
        /// Pauses for a specified duration. This delay is safe to use even when the editor is
        /// paused in play mode or out of focus.
        /// </summary>
        public static Task Delay(int millisecondsDelay) => Delay(millisecondsDelay, CancellationToken.None);

        /// <summary>
        /// Pauses for a specified duration. This delay is safe to use even when the editor is
        /// paused in play mode or out of focus.
        /// </summary>
        public static Task Delay(int millisecondsDelay, CancellationToken cancellationToken) => Delay(TimeSpan.FromMilliseconds(millisecondsDelay), cancellationToken);

        /// <summary>
        /// Creates a Task that completes after a specified time interval, driven by the editor's
        /// update loop. This method is robust and works correctly even when the editor is paused
        /// in play mode or out of focus.
        /// </summary>
        public static Task Delay(TimeSpan delay, CancellationToken cancellationToken)
        {
            var rawDelayTask = CreateEditorUpdateDelayTask(delay, cancellationToken);
            return rawDelayTask.ConfigureAwaitMainThread();

            static Task CreateEditorUpdateDelayTask(TimeSpan time, CancellationToken token)
            {
                if (time <= TimeSpan.Zero)
                {
                    return Task.CompletedTask;
                }

                if (token.IsCancellationRequested)
                {
                    return Task.FromCanceled(token);
                }

                var tcs = new TaskCompletionSource<bool>();
                double endTime;

                var yieldInstead = false;
                try { endTime = EditorApplication.timeSinceStartup + time.TotalSeconds; }
                catch (UnityException)
                {
                    // We can't calculate a delay, so we'll treat this as a request
                    // to yield and complete on the very next editor update frame.
                    // This can happen when the Editor is serializing game objects.
                    yieldInstead = true;
                    endTime = 0;
                }

                EditorApplication.CallbackFunction updateCallback = null;
                CancellationTokenRegistration registration = default;

                updateCallback = () =>
                {
                    if (yieldInstead || EditorApplication.timeSinceStartup >= endTime)
                    {
                        tcs.TrySetResult(true);
                        EditorApplication.update -= updateCallback;
                        try { registration.Dispose(); } catch { /* ignored */ }
                    }
                };

                registration = token.Register(() =>
                {
                    tcs.TrySetCanceled(token);
                    EditorApplication.update -= updateCallback;
                    try { registration.Dispose(); } catch { /* ignored */ }
                });

                EditorApplication.update += updateCallback;
                return tcs.Task;
            }
        }

        /// <summary>
        /// Runs an action on a background thread. See <see cref="Run{TResult}(Func{Task{TResult}}, CancellationToken)"/> for details.
        /// </summary>
        public static Task Run(Action action) => Run(action, CancellationToken.None);

        /// <summary>
        /// Runs an action on a background thread. See <see cref="Run{TResult}(Func{Task{TResult}}, CancellationToken)"/> for details.
        /// </summary>
        public static Task Run(Action action, CancellationToken cancellationToken)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return Run<bool>(() =>
            {
                action();
                return Task.FromResult(true);
            }, cancellationToken);
        }

        /// <summary>
        /// Runs an async function on a background thread. See <see cref="Run{TResult}(Func{Task{TResult}}, CancellationToken)"/> for details.
        /// </summary>
        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function) => Run(function, CancellationToken.None);

        /// <summary>
        /// Runs an async function on a background thread, ensuring it does not hang in a paused editor.
        /// Its paramount design goal is to *always* unblock the `await`-ing caller immediately upon cancellation.
        /// It achieves this by using `EditorApplication.update` to monitor the underlying task. When a cancellation
        /// is requested, the caller is unblocked, and a "best effort" signal is sent to the detached background work.
        /// </summary>
        /// <param name="function">The asynchronous function to execute.</param>
        /// <param name="cancellationToken">The token to observe for cancellation.</param>
        public static async Task<TResult> Run<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            // 1. Start the core task logic, which handles backgrounding and cancellation signals.
            var coreTask = RunCore(function, cancellationToken);

            // 2. Create a TaskCompletionSource to represent the completion of our monitoring.
            // This is the key to surviving a paused editor, as the `update` event continues to fire.
            var monitorTcs = new TaskCompletionSource<bool>();
            EditorApplication.CallbackFunction monitorCallback = null;

            monitorCallback = () =>
            {
                // 3. When the inner task completes, we stop monitoring and signal our monitor task.
                if (coreTask.IsCompleted)
                {
                    EditorApplication.update -= monitorCallback; // Clean up the monitor
                    monitorTcs.TrySetResult(true); // Signal that monitoring is finished
                }
            };

            EditorApplication.update += monitorCallback;

            // 4. Wait for our monitor to tell us the task is complete.
            await monitorTcs.Task;

            // 5. Now that we KNOW the task is complete, we can safely await it.
            // This will not hang. It will either return the result or throw the correct
            // exception (TaskCanceledException or the one from IsFaulted).
            return await coreTask;
        }

        /// <summary>
        /// This is the core implementation that runs a task with a robust cancellation and abandonment-detection policy.
        /// It is wrapped by the public `Run` method which adds the editor-pump monitoring layer.
        /// </summary>
        static Task<TResult> RunCore<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            var tcs = new TaskCompletionSource<TResult>();
            if (cancellationToken.IsCancellationRequested)
            {
                tcs.SetCanceled();
                return tcs.Task;
            }

            var abandonmentLogCts = new CancellationTokenSource();
            CancellationTokenRegistration callerTokenRegistration = default;

            _ = Task.Run(async () =>
            {
                try
                {
                    var result = await Task.Run(function, cancellationToken).ConfigureAwait(false);
                    await EditorThread.EnsureMainThreadAsync();
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException)
                    {
                        // This try-catch is a safeguard against race conditions where the TCS
                        // might already be cancelled by the time an exception is processed.
                        try { tcs.TrySetCanceled(cancellationToken); } catch { /* ignored */ }
                    }
                    else
                    {
                        // This try-catch is a safeguard against race conditions where the TCS
                        // might already be cancelled by the time an exception is processed.
                        try { tcs.TrySetException(ex); } catch { /* ignored */ }
                    }
                }
                finally
                {
                    try { abandonmentLogCts.Cancel(); } catch (ObjectDisposedException) { /* Ignored */ }
                    #pragma warning disable CS1690
                    try { callerTokenRegistration.Dispose(); } catch { /* ignored */ }
                    #pragma warning restore CS1690
                }
            });

            callerTokenRegistration = cancellationToken.Register(() =>
            {
                // Unblock the caller immediately.
                tcs.TrySetCanceled(cancellationToken);

                // Start a background timer to detect if the cancelled task was non-cooperative.
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // We must use our own robust Delay, as Task.Delay can hang in the editor.
                        await Delay(k_AbandonmentTimeoutMilliseconds, abandonmentLogCts.Token);
                    }
                    catch (OperationCanceledException) { /* This is the expected success path. */ }
                    catch (ObjectDisposedException) { /* This is the expected success path. */ }
                    //if (Unsupported.IsDeveloperMode())
                    //    Debug.Log("An EditorTask was cancelled, but the background work did not complete within 5 seconds. The task may be non-cooperative and has been abandoned.");
                });
            });

            tcs.Task.ContinueWith(_ => abandonmentLogCts.Dispose(), TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        /// <summary>
        /// Dispatches an action on the main thread. Uses the editor's update loop for scheduling,
        /// making it safe to `await` even in a paused editor.
        /// </summary>
        public static Task RunOnMainThread(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (EditorThread.isMainThread)
            {
                action();
                return Task.CompletedTask;
            }

            var tcs = new TaskCompletionSource<bool>();
            EditorApplication.CallbackFunction updateCallback = null;
            updateCallback = () =>
            {
                EditorApplication.update -= updateCallback;
                try
                {
                    action();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            };
            EditorApplication.update += updateCallback;
            return tcs.Task;
        }

        /// <summary>
        /// Dispatches an asynchronous action on the main thread. See <see cref="RunOnMainThread(Action)"/> for details.
        /// </summary>
        public static Task RunOnMainThread(Func<Task> asyncAction) => RunOnMainThread(asyncAction, CancellationToken.None);

        /// <summary>
        /// Dispatches an asynchronous action on the main thread. Uses the editor's update loop for scheduling,
        /// making it safe to `await` even in a paused editor.
        /// </summary>
        public static Task RunOnMainThread(Func<Task> asyncAction, CancellationToken cancellationToken)
        {
            if (asyncAction == null)
                throw new ArgumentNullException(nameof(asyncAction));

            if (EditorThread.isMainThread && cancellationToken == CancellationToken.None)
            {
                return asyncAction();
            }

            var tcs = new TaskCompletionSource<bool>();
            var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

            EditorApplication.CallbackFunction updateCallback = null;
            updateCallback = async () =>
            {
                EditorApplication.update -= updateCallback;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await asyncAction();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException) tcs.TrySetCanceled(cancellationToken);
                    else tcs.TrySetException(ex);
                }
                finally
                {
                    registration.Dispose();
                }
            };
            EditorApplication.update += updateCallback;
            return tcs.Task;
        }

        /// <summary>
        /// Dispatches an asynchronous action on the main thread that returns a result. See <see cref="RunOnMainThread(Action)"/> for details.
        /// </summary>
        public static Task<TResult> RunOnMainThread<TResult>(Func<Task<TResult>> asyncAction) => RunOnMainThread(asyncAction, CancellationToken.None);

        /// <summary>
        /// Dispatches an asynchronous action on the main thread that returns a result. Uses the editor's update loop for scheduling,
        /// making it safe to `await` even in a paused editor.
        /// </summary>
        public static Task<TResult> RunOnMainThread<TResult>(Func<Task<TResult>> asyncAction, CancellationToken cancellationToken)
        {
            if (asyncAction == null)
                throw new ArgumentNullException(nameof(asyncAction));

            if (EditorThread.isMainThread && cancellationToken == CancellationToken.None)
            {
                return asyncAction();
            }

            var tcs = new TaskCompletionSource<TResult>();
            var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

            EditorApplication.CallbackFunction updateCallback = null;
            updateCallback = async () =>
            {
                EditorApplication.update -= updateCallback;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var result = await asyncAction();
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException) tcs.TrySetCanceled(cancellationToken);
                    else tcs.TrySetException(ex);
                }
                finally
                {
                    registration.Dispose();
                }
            };
            EditorApplication.update += updateCallback;
            return tcs.Task;
        }

        /// <summary>
        /// Asynchronously waits for a condition. This method is safe to call
        /// even when the Unity Editor is not in focus. It uses an EditorApplication.update
        /// subscription to poll for the required state, which keeps the async context alive.
        /// </summary>
        /// <returns>A Task that completes when the condition is met or a timeout is reached.</returns>
        public static Task<bool> WaitForCondition(Func<bool> condition, TimeSpan timeoutDuration)
        {
            // If the API is already accessible, we can return immediately.
            if (condition())
            {
                return Task.FromResult(true);
            }

            // Use a TaskCompletionSource to convert the event-based check into an awaitable Task.
            var tcs = new TaskCompletionSource<bool>();
            var timeout = DateTime.Now + timeoutDuration;

            EditorApplication.CallbackFunction updateCallback = null;

            // This delegate will be called on every editor update frame.
            updateCallback = () =>
            {
                // Check for success condition
                if (condition())
                {
                    tcs.TrySetResult(true); // Complete the task successfully
                    EditorApplication.update -= updateCallback; // Unsubscribe to stop polling

                    return;
                }

                // Check for timeout condition
                if (DateTime.Now > timeout)
                {
                    tcs.TrySetResult(false); // Complete the task to unblock the caller
                    EditorApplication.update -= updateCallback; // Unsubscribe to stop polling
                }
            };

            // Start polling by subscribing to the editor's update loop.
            EditorApplication.update += updateCallback;

            return tcs.Task;
        }
    }
}
