using System;
using Unity.AI.Toolkit.Accounts.Services;
using Unity.AI.Toolkit.Accounts.Services.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AI.Toolkit.Accounts.Components
{
    [UxmlElement]
    public partial class SessionStatusBanner : VisualElement
    {
        AIDisabledBanner m_AiDisabledBanner;
        SignInBanner m_SignIn;
        SignInDelayedBanner m_SignInDelayedBanner;
        NoNetworkBanner m_NoNetwork;
        AIDisabledPackageBanner m_AIDisabledPackageBanner;
        AIDisabledLegalBanner m_AIDisabledLegalBanner;
        ConnectToCloudBanner m_ConnectToCloudBanner;
        ConnectToCloudDelayedBanner m_ConnectToCloudDelayedBanner;
        AccountLoadDelayedBanner m_AccountLoadDelayedBanner;
        RegionBanner m_RegionBanner;
        PackagesUnsupportedBanner m_UnsupportedBanner;

        protected VisualElement m_Current;

        public SessionStatusBanner()
        {
            AddToClassList("session-status-banner");
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                Account.session.OnChange += Refresh;
                Refresh();
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                Account.session.OnChange -= Refresh;
            });
        }

        // ReSharper disable once VirtualMemberNeverOverridden.Global
        protected virtual VisualElement CurrentView()
        {
            VisualElement current = null;
            if (!Account.network.IsAvailable)
                current = m_NoNetwork ??= new();
            else if (!Account.settings.RegionAvailable)
                current = m_RegionBanner ??= new ();
            else if (!Account.settings.PackagesSupported && this is AssistantSessionStatusBanner)
                current = m_UnsupportedBanner ??= new AssistantUnsupportedBanner();
            else if (!Account.settings.PackagesSupported && this is GeneratorsSessionStatusBanner)
                current = m_UnsupportedBanner ??= new GeneratorsUnsupportedBanner();
            else if (!Account.settings.PackagesSupported)
                current = m_UnsupportedBanner ??= new ();
            else if (Account.signIn.Value == SignInStatus.NotReady)
                current = m_SignInDelayedBanner ??= new ();
            else if (Account.signIn.IsSignedOut)
                current = m_SignIn ??= new();
            else if (Account.cloudConnected.Value == ProjectStatus.NotConnected)
                current = m_ConnectToCloudBanner ??= new();
            else if (Account.cloudConnected.Value == ProjectStatus.NotReady)
                current = m_ConnectToCloudDelayedBanner ??= new ();
            else if (Account.settings.Value == null)
                current = m_AccountLoadDelayedBanner ??= new ();
            else if (!Account.settings.AiAssistantEnabled && !Account.settings.AiGeneratorsEnabled)
                current = m_AiDisabledBanner ??= new();
            else if (!Account.legalAgreement.IsAgreed)
                current = m_AIDisabledLegalBanner ??= new();
            else if (!Account.settings.AiAssistantEnabled && this is AssistantSessionStatusBanner)
                current = m_AIDisabledPackageBanner ??= new();
            else if (!Account.settings.AiGeneratorsEnabled && this is GeneratorsSessionStatusBanner)
                current = m_AIDisabledPackageBanner ??= new();
            return current;
        }

        // ReSharper disable once VirtualMemberNeverOverridden.Global
        protected virtual void Refresh()
        {
            var current = CurrentView();
            if (m_Current != current)
            {
                Clear();
                m_Current = current;
                if (m_Current != null)
                    Add(m_Current);
            }
        }
    }
}
