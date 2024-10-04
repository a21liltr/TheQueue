using Newtonsoft.Json;

namespace TheQueue.Server.Core.Models.ServerMessages
{
    public class QueueTicket
    {
        public string Name { get; set; }
        public int Ticket { get; set; }
    }
}
