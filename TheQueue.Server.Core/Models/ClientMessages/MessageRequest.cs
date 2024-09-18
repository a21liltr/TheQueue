namespace TheQueue.Server.Core.Models.ClientMessages
{
    public class MessageRequest : RequestBase
    {
        public string Name { get; set; }
        public Recipient Message { get; set; }
    }

    public class Recipient
    {
        public string Name { get; set; }
        public string Body { get; set; }

    }
}
