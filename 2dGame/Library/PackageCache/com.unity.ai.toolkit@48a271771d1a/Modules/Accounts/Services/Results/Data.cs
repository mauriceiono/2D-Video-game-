using System;
using AiEditorToolsSdk.Components.Organization.Responses;

namespace Unity.AI.Toolkit.Accounts.Services.Data
{
    [Serializable]
    public record SettingsRecord
    {
        public string OrgId;
        public bool IsAiAssistantEnabled;
        public bool IsAiGeneratorsEnabled;
        public bool IsDataSharingEnabled;
        public bool IsTermsOfServiceAccepted;

        public SettingsRecord(SettingsResult result)
        {
            OrgId = result?.OrgId;
            IsAiAssistantEnabled = result is { IsAiAssistantEnabled: true };
            IsAiGeneratorsEnabled = result is { IsAiGeneratorsEnabled: true };
            IsDataSharingEnabled = result is { IsDataSharingEnabled: true };
            IsTermsOfServiceAccepted = result is { IsTermsOfServiceAccepted: true };
        }
    }

    [Serializable]
    public record PointsBalanceRecord
    {
        public string OrgId;
        public long PointsAllocated;
        public long PointsAvailable;

        public PointsBalanceRecord(PointsBalanceResult result)
        {
            OrgId = result?.OrgId;
            PointsAllocated = result?.PointsAllocated ?? 0;
            PointsAvailable = result?.PointsAvailable ?? 0;
        }
    }

    [Serializable]
    public enum SignInStatus
    {
        NotReady,
        SignedIn,
        SignedOut,
    }

    [Serializable]
    public enum ProjectStatus
    {
        NotReady,
        Connected,
        NotConnected,
        OfflineConnected,
    }
}
