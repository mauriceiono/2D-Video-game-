using System.Collections.Generic;
using Unity.AI.Assistant.Editor.Data;

namespace Unity.AI.Assistant.UI.Editor.Scripts.Data
{
    internal class ConversationModel
    {
        public AssistantConversationId Id;
        public string Title;
        public readonly List<MessageModel> Messages = new();

        public long LastMessageTimestamp;
        public bool IsFavorite;
        public bool IsLoaded;

        public double StartTime;
    }
}
