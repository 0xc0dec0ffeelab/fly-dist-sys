using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fly_dist_sys._1_Echo
{
    public class EchoMessageBody : MessageBody
    {
        [JsonPropertyName("echo")]
        public required string Echo { get; set; }
    }
}
