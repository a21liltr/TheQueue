namespace TheQueue.Server.Core.Models.ClientMessages
{
    public class MessageRequest : RequestBase
    {
        public string Name { get; set; }
        public Message Message { get; set; }
    }

    public class Message
    {
        public string Recipient { get; set; }
        public string Body { get; set; }

    }
}
