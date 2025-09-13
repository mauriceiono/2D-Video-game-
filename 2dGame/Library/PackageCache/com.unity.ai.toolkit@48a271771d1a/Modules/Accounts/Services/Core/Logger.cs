using System;
using UnityEngine;

namespace Unity.AI.Toolkit.Accounts.Services.Core
{
    class Logger : AiEditorToolsSdk.Domain.Abstractions.Services.ILogger
    {
        public void LogDebug(string message)
        {
            try
            {
                EditorTask.RunOnMainThread(() =>
                {
                    Debug.Log(message);
                });
            }
            catch
            {
                // Silent catch with no logging
            }
        }

        public void LogDebug(Exception exception, string message)
        {
            try
            {
                EditorTask.RunOnMainThread(() =>
                {
                    Debug.Log(message);
                    Debug.LogException(exception);
                });
            }
            catch
            {
                // Silent catch with no logging
            }
        }

        public void LogDebug(Exception exception)
        {
            try
            {
                EditorTask.RunOnMainThread(() =>
                {
                    Debug.LogException(exception);
                });
            }
            catch
            {
                // Silent catch with no logging
            }
        }

        public void LogPublicInformation(string message)
        {
            try
            {
                EditorTask.RunOnMainThread(() =>
                {
                    Debug.Log(message);
                });
            }
            catch
            {
                // Silent catch with no logging
            }
        }

        public void LogPublicError(string message)
        {
            try
            {
                EditorTask.RunOnMainThread(() =>
                {
                    Debug.LogError(message);
                });
            }
            catch
            {
                // Silent catch with no logging
            }
        }
    }
}
