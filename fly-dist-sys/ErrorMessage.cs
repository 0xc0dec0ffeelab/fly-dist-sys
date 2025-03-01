using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fly_dist_sys
{
    public class ErrorMessage
    {
        /// <summary>
        /// type = "error"
        /// </summary>
        [JsonPropertyName("type")]
        public required string Type { get; set; }
        [JsonPropertyName("in_reply_to")]
        public int? InReplyTo { get; set; }
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
