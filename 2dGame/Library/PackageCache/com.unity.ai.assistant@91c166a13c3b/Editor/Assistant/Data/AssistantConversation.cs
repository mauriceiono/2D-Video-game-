using System;
using System.Collections.Generic;

namespace Unity.AI.Assistant.Editor.Data
{
    [Serializable]
    class AssistantConversation
    {
        public string Title;
        public AssistantConversationId Id;
        public readonly List<AssistantMessage> Messages = new();
    }
}
