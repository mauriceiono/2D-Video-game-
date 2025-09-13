using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AiEditorToolsSdk;
using AiEditorToolsSdk.Components.Common.Enums;
using AiEditorToolsSdk.Components.Common.Responses.Wrappers;
using AiEditorToolsSdk.Components.Organization;
using AiEditorToolsSdk.Components.Organization.Responses;
using AiEditorToolsSdk.Domain.Abstractions.Services;
using Unity.AI.Toolkit.Accounts.Services.States;
using Unity.AI.Toolkit.Connect;
using UnityEditor;
using UnityEngine;

namespace Unity.AI.Toolkit.Accounts.Services.Core
{
    static class AccountApi
    {
        [InitializeOnLoadMethod]
        static void InitializeEnvironmentKeys() => Environment.RegisterEnvironmentKey(k_AccountEnvironmentKey, "Account Environment", _ => {
            Account.settings.Refresh();
            Account.pointsBalance.Refresh();
        });

        const string k_AccountEnvironmentKey = "AI_Toolkit_Account_Environment";

        public static string selectedEnvironment => Environment.GetSelectedEnvironment(k_AccountEnvironmentKey);

        static string s_LastLoggedError = string.Empty;
        static string s_LastLoggedException = string.Empty;

        static readonly string k_SessionTraceId = Guid.NewGuid().ToString();

        // These timeouts are NOT for network requests. They are external deadlines imposed on our async operations
        // to detect and escape from hangs in the Unity Editor's async scheduler.
        static readonly int[] k_TimeoutDurations = { 2, 4, 8, 16, 32 };

        // Cache for in-progress tasks
        static readonly Dictionary<Type, Task> k_TaskCache = new();

        class TraceIdProvider : ITraceIdProvider
        {
            readonly string m_SessionId;

            public TraceIdProvider(string sessionId) => m_SessionId = sessionId;

            public Task<string> GetTraceId() => Task.FromResult(m_SessionId);
        }

        /// <summary>
        /// Performs an API request with a multi-layered retry strategy.
        /// NOTE: The for-loop in this method is NOT a standard network retry loop.
        /// The inner SDK handles transient network errors (e.g., 503s) automatically.
        /// This outer loop is a safeguard against a known Unity Editor issue where async/await tasks
        /// can hang indefinitely. We impose our own escalating timeouts to detect such hangs and
        /// retry the entire operation from scratch.
        /// </summary>
        static async Task<TResponse> Request<TResponse>(Func<IOrganizationComponent, Task<OperationResult<TResponse>>> callback) where TResponse : class
        {
            try
            {
                await ApiAccessibleState.WaitForCloudProjectSettings();

                using var editorFocus = new EditorAsyncKeepAliveScope("Verifying account settings.");

                var builder = Builder.Build(UnityConnectProvider.organizationKey, UnityConnectProvider.userId, UnityConnectProvider.projectId, HttpClientManager.instance, selectedEnvironment, new Logger(), new Auth(), new TraceIdProvider(k_SessionTraceId));
                var component = builder.OrganizationComponent();

                OperationResult<TResponse> result = null;
                // This loop attempts the operation with increasingly longer deadlines.
                var retryAttempt = 0;
                for (; retryAttempt < k_TimeoutDurations.Length; retryAttempt++)
                {
                    try
                    {
                        // Create a CancellationToken that acts as our "hang detector" for this attempt.
                        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(k_TimeoutDurations[retryAttempt]));
                        var timeoutToken = tokenSource.Token;

                        // We run the entire SDK callback within our robust EditorTask, but with the external timeoutToken.
                        result = await EditorTask.Run(() => callback(component), timeoutToken);

                        if (result.Result.IsSuccessful)
                        {
                            // Success: The operation completed within our deadline.
                            return result.Result.Value;
                        }

                        // The API call completed without hanging but returned a definitive failure (e.g., 401 Unauthorized, invalid plan).
                        // These are not transient errors, so there is no point in retrying. We break the loop to process the final result.
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        // This catch block is the core of our editor-hang safeguard.
                        // An OperationCanceledException here means our self-imposed timeout was triggered.
                        // We interpret this not as a network timeout, but as a potential hang in the editor's async scheduler.
                        // We will let the loop continue to try again with a longer deadline.
                    }
                }

                // Handle the final result after all attempts have been exhausted or a definitive failure was encountered.
                if (result != null)
                {
                    if (result.Result.Error.AiResponseError == AiResultErrorEnum.UnavailableForLegalReasons)
                        Account.settings.RegionAvailable = false;

                    if (result.Result.Error.AiResponseError == AiResultErrorEnum.ApiNoLongerSupported)
                        Account.settings.PackagesSupported = false;

                    var errorMessage = result.Result.Error.AiResponseError == AiResultErrorEnum.RateLimitExceeded // typically means wrong url (staging vs prod)
                        ? $"Account information returned {result.Result.Error.AiResponseError} on Url '{selectedEnvironment}'. Is the Url correct?\nTrace Id {result.SdkTraceId} => {result.W3CTraceId}"
                        : $"Error after {retryAttempt + 1} attempt(s): {result.Result.Error.AiResponseError} - {result.Result.Error.Errors.FirstOrDefault()} -- Result type: {typeof(TResponse).Name} -- Url: {selectedEnvironment} -- Trace Id {result.SdkTraceId} => {result.W3CTraceId}";
                    if (!string.IsNullOrEmpty(UnityConnectProvider.organizationKey) && errorMessage != s_LastLoggedError)
                    {
                        Debug.Log(errorMessage);
                        s_LastLoggedError = errorMessage;
                    }
                }
            }
            catch (Exception exception)
            {
                // This outer catch handles any unexpected exceptions not caught by the inner retry loop.
                var exceptionMessage = exception.ToString();
                if (!string.IsNullOrEmpty(UnityConnectProvider.organizationKey) && exceptionMessage != s_LastLoggedException)
                {
                    Debug.Log($"Exception after retry attempts: {exceptionMessage}");
                    s_LastLoggedException = exceptionMessage;
                }
            }

            return null;
        }

        static Task<T> GetOrCreateCachedTask<T>(Func<Task<T>> taskFactory) where T : class
        {
            var type = typeof(T);

            if (k_TaskCache.TryGetValue(type, out var existingTask) && !existingTask.IsCompleted)
            {
                return (Task<T>)existingTask;
            }

            var newTask = taskFactory();
            k_TaskCache[type] = newTask;

            return newTask;
        }

        internal static Func<Task<SettingsResult>> GetSettingsDelegate = () => Request(component => component.GetSettings());
        internal static Func<Task<PointsBalanceResult>> GetPointsDelegate = () => Request(component => component.GetPointsBalance());

        internal static Task<SettingsResult> GetSettings() =>
            GetOrCreateCachedTask(GetSettingsDelegate);

        internal static Task<PointsBalanceResult> GetPointsBalance() =>
            GetOrCreateCachedTask(GetPointsDelegate);

        internal static Task<SettingsResult> SetTermsOfServiceAcceptance(bool value) =>
            Request(component => component.SetTermsOfServiceAcceptance(value));
    }

    static class HttpClientManager
    {
        static HttpClient s_Instance;

        public static HttpClient instance
        {
            get { return s_Instance ??= new HttpClient(); }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Initialize()
        {
            s_Instance = null;
            Application.quitting += Dispose;
        }

        static void Dispose()
        {
            Application.quitting -= Dispose;

            if (s_Instance != null)
            {
                s_Instance.Dispose();
                s_Instance = null;
            }
        }
    }
}
