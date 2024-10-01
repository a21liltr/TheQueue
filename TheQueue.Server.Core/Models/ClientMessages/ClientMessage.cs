using TheQueue.Server.Core.Enums;
using TheQueue.Server.Core.Models.ServerMessages;

namespace TheQueue.Server.Core.Models.ClientMessages
{
	public class ClientMessage
	{
		public string ClientId { get; set; }
		public string? Name { get; set; }
		public bool? EnterQueue { get; set; }
		public bool? SuperVisor { get; set; }
		public Message? Message { get; set; }
        public QueueTicket? QueueTicket { get; set; }
        public Status? Status { get; set; }
    }
}
