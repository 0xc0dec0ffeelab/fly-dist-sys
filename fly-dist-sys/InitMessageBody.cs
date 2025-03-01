using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fly_dist_sys
{
    public class InitMessageBody : MessageBody
    {
        [JsonPropertyName("node_id")]
        public string? Id { get; set; }
        [JsonPropertyName("node_ids")]
        public List<string>? Ids { get; set; }
    }
}
