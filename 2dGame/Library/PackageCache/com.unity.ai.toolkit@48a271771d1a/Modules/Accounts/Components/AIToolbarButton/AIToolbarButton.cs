using System;
using System.Linq;
using Unity.AI.Toolkit.Accounts.Services;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AI.Toolkit.Accounts
{
    public static class AIToolbarButton
    {
        const string k_UssClassName = "ai-toolbar-button";

        const string k_NotificationVariantUssClassName = "ai-toolbar-button--with-points-notifications";

        const string k_StyleSheetPath =
            "Packages/com.unity.ai.toolkit/Modules/Accounts/Components/AIToolbarButton/AIToolbarButton.uss";

        const int k_NotificationDurationMs = 2000;

        static bool s_Initialized;
        static Button AIButton => AIDropdownController.aiButton;
        // Hard to pin down the exact element as it changes name id per Unity version. Structure seems somewhat stable though, so we are basing query on that.
        static TextElement STextElement => AIButton.Query<TextElement>().ToList().LastOrDefault();
        static string s_OriginalContent;
        static IVisualElementScheduledItem s_AIToolbarButtonSchedule;

        public static void ShowPointsCostNotification(int amount)
        {
            if (AIButton == null)
                return;

            s_AIToolbarButtonSchedule?.Pause();
            STextElement.text = $"-{amount}";
            s_AIToolbarButtonSchedule = AIButton.schedule.Execute(() =>
            {
                STextElement.text = s_OriginalContent;
                AIButton.RemoveFromClassList(k_NotificationVariantUssClassName);
            }).StartingIn(k_NotificationDurationMs);
            AIButton.AddToClassList(k_NotificationVariantUssClassName);
        }

        internal static void Init()
        {
            if (s_Initialized)
                return;

            s_Initialized = true;
            s_OriginalContent = STextElement.text;

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StyleSheetPath);
            if (!styleSheet)
                return;

            AIButton.styleSheets.Add(styleSheet);
            AIButton.AddToClassList(k_UssClassName);
        }
    }
}

