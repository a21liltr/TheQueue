using TheQueue.Server.Core.Enums;
using TheQueue.Server.Core.Models.ServerMessages;

namespace TheQueue.Server.Core.Models
{
    public class Supervisor
    {
        public string Name { get; set; }
        public Status Status { get; set; }
        public QueueTicket? Client { get; set; }
    }
}
