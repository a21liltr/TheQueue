namespace TheQueue.Server.Core.Models.ClientMessages
{
    public class EnterQueueRequest : RequestBase
    {
        public string Name { get; set; }
        public bool EnterQueue { get; set; }
    }
}
