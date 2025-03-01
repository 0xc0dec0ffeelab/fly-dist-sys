using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fly_dist_sys._2_Unique_ID_Generation
{
    public class MessageBody_2 : MessageBody
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }
    }
}
