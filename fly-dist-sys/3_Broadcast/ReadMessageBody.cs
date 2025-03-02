using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fly_dist_sys._3_Broadcast
{
    public class ReadMessageBody : MessageBody
    {
        [JsonPropertyName("messages")]
        public required List<int> Messages { get; set; }
    }
}
