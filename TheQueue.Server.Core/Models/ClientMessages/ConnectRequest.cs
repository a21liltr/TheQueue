namespace TheQueue.Server.Core.Models.ClientMessages
{
    public class ConnectRequest
    {
        public string Name { get; set; }
        public bool EnterQueue { get; set; }
        public bool? Supervisor { get; set; }
    }
}