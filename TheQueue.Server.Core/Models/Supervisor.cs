using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TheQueue.Server.Core.Enums;
using TheQueue.Server.Core.Models.ServerMessages;

namespace TheQueue.Server.Core.Models
{
    public class Supervisor
    {
        public string Name { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }
        public QueueTicket? Client { get; set; }
    }
}