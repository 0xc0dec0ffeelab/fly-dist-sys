using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fly_dist_sys
{
    /// <summary>
    /// https://github.com/jepsen-io/maelstrom/blob/main/doc/protocol.md#messages
    /// </summary>
    public class Message
    {
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Id { get; set; }

        [JsonPropertyName("src")]
        public string? Src { get; set; }

        [JsonPropertyName("dest")]
        public string? Dest { get; set; }

        [JsonPropertyName("body")]
        public JsonElement Body { get; set; }
    }
}
