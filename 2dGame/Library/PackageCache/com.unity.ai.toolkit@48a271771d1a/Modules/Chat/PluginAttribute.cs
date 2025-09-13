using System;
using System.ComponentModel;

namespace Unity.AI.Toolkit.Chat
{
    /// <summary>
    ///     Marks a static method executing commands for AI Assistant.
    ///     Each method parameter must have a <see cref="ParameterAttribute"/> attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false), EditorBrowsable(EditorBrowsableState.Never)]
    public class PluginAttribute : Attribute
    {
        /// <summary>
        ///     A description of the effect applied by the method.
        /// </summary>
        public readonly string Description;

        /// <summary>
        ///     This is the text that will be associated with the action of calling this plugin. For example, Generate
        ///     Sprite. This text will be displayed where the user clicks to perform the plugin call. Probably a button.
        ///     If null or empty Ai Assistant will provide a reasonable fallback.
        /// </summary>
        public readonly string ActionText;

        /// <summary>
        ///     This is the tool that the plugin is associated with. For example "Agent", or "Texture". If null or empty
        ///     Ai Assistant will provide a reasonable fallback.
        /// </summary>
        public readonly string ToolName;

        /// <summary>
        ///     Text that will be used when displaying the method in a UI block. Example "Texture" to name to product,
        ///     or "Run this with Agent?" to instruct the user. This field is optional. If null, the system will
        ///     determine a reasonable fallback.
        /// </summary>
        public readonly string DisplayText;

        /// <summary>
        ///     Marks a static method executing commands for AI Assistant.
        /// </summary>
        /// <param name="description">
        ///     A description of the effect applied by the method.
        /// </param>
        /// <param name="actionText">
        ///     This is the text that will be associated with the action of calling this plugin. For example, Generate
        ///     Sprite. This text will be displayed where the user clicks to perform the plugin call. Probably a button.
        ///     If null or empty Ai Assistant will provide a reasonable fallback.
        /// </param>
        /// <param name="toolName">
        ///     This is the tool that the plugin is associated with. For example "Agent", or "Texture". If null or empty
        ///     Ai Assistant will provide a reasonable fallback.
        /// </param>
        /// <param name="displayText"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public PluginAttribute(string description, string actionText = null, string toolName = null, string displayText = null)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException(nameof(description),
                    $"Cannot be empty");

            Description = description;
            ActionText = actionText;
            ToolName = toolName;
            DisplayText = displayText;
        }
    }
}
