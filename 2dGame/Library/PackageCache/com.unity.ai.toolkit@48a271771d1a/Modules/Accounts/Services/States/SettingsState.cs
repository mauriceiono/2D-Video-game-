using System;
using System.Threading.Tasks;
using Unity.AI.Toolkit.Accounts.Services.Core;
using Unity.AI.Toolkit.Accounts.Services.Data;
using Unity.AI.Toolkit.Connect;
using UnityEditor;
using UnityEngine;

namespace Unity.AI.Toolkit.Accounts.Services.States
{
    public class SettingsState
    {
        internal readonly Signal<SettingsRecord> settings;
        internal readonly Signal<bool> regionAvailability;
        internal readonly Signal<bool> packagesSupported;

        public event Action OnChange;
        public SettingsRecord Value { get => settings.Value; internal set => settings.Value = value; }
        public bool RegionAvailable { get => regionAvailability.Value; set => regionAvailability.Value = value; }
        public bool PackagesSupported { get => packagesSupported.Value; set => packagesSupported.Value = value; }
        public void Refresh() => settings.Refresh();
        public Task RefreshSettings() => RefreshInternal();
        public bool AiAssistantEnabled => Value?.IsAiAssistantEnabled ?? false;
        public bool AiGeneratorsEnabled => Value?.IsAiGeneratorsEnabled ?? false;
        public bool IsDataSharingEnabled => Value?.IsDataSharingEnabled ?? false;
        public bool IsTermsOfServiceAccepted => Value?.IsTermsOfServiceAccepted ?? false;

        public SettingsState()
        {
            settings = new(AccountPersistence.SettingsProxy, () => _ = RefreshInternal(), () => OnChange?.Invoke());
            regionAvailability = new Signal<bool>(AccountPersistence.RegionAvailabilityProxy, () => _ = RefreshInternal(), () => OnChange?.Invoke());
            packagesSupported = new Signal<bool>(AccountPersistence.PackagesSupportedProxy, () => _ = RefreshInternal(), () => OnChange?.Invoke());

            Refresh();
            AIDropdownBridge.ConnectProjectStateChanged(Refresh);
            AIDropdownBridge.ConnectStateChanged(Refresh);
            AIDropdownBridge.UserStateChanged(Refresh);
            if (!Application.isBatchMode)
                EditorApplication.focusChanged += OnEditorFocusChanged;
        }

        void OnEditorFocusChanged(bool focused)
        {
            if (focused)
                Refresh();
        }

        async Task RefreshInternal()
        {
            RegionAvailable = true; // Assume region is available by default, change to false if there is an error in AccountApi.GetSettings()
            PackagesSupported = true; // Same for packages, assume they are supported by default

            var result = await AccountApi.GetSettings();
            if (result == null)
                Value = null;
            else
                Value = new(result);
        }
    }
}
