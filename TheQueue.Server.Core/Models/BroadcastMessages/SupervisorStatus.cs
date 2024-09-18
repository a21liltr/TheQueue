using TheQueue.Server.Core.Enums;

namespace TheQueue.Server.Core.Models.BroadcastMessages
{
	public class SupervisorStatus
	{
        public string Name { get; set; }
        public Status Status { get; set; }
        public QueueStatus? Client { get; set; }
    }
}
