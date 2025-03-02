using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fly_dist_sys._3_Broadcast
{
    public class TopologyMessageBody : MessageBody
    {
        [JsonPropertyName("topology")]
        public required ConcurrentDictionary<string, List<string>> Topology { get; set; }
    }
}
