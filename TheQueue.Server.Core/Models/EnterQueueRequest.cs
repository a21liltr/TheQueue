namespace TheQueue.Server.Core.Models
{
    public class EnterQueueRequest
    {
        public Guid ClientId { get; set; }
        public string Name { get; set; }
        public bool EnterQueue {  get; set; }

        public EnterQueueRequest(string name, bool enterQueue)
        {
            Name = name;
            EnterQueue = enterQueue;
        }
    }
}
