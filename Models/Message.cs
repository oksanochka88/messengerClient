using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mACRON.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        [JsonProperty("user_id")]
        public string UserId { get; set; }
        public string Content { get; set; }
        [JsonProperty("created_at")]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime CreatedAt { get; set; }
    }


    public class MessagesResponse
    {
        public List<Message> Messages { get; set; }
    }
}
