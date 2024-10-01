namespace TheQueue.Server.Core.Models.ClientMessages
{
    public class Heartbeat : RequestBase
    {
        public string ClientId { get; set; }
    }
}
