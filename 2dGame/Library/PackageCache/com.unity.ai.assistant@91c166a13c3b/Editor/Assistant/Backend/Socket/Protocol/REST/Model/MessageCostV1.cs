using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using OpenAPIDateConverter = Unity.Ai.Assistant.Protocol.Client.OpenAPIDateConverter;

namespace Unity.Ai.Assistant.Protocol.Model
{
    /// <summary>
    /// Schema for a conversation.
    /// </summary>
    [DataContract(Name = "MessageCostV1")]
    internal partial class MessageCostV1
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCostV1" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected MessageCostV1() { }
        public MessageCostV1(MessagePoints messagePoints)
        {
            MessagePoints = messagePoints;
        }

        /// <summary>
        /// Gets or Sets MessagePoints
        /// </summary>
        [DataMember(Name = "message_points", IsRequired = true, EmitDefaultValue = true)]
        public MessagePoints MessagePoints { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class MessageCostV1 {\n");
            sb.Append("  MessagePoints: ").Append(MessagePoints).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
    }

}
