namespace TheQueue.Server.Core.Models
{
    public class MessageRequest
    {
        public Guid ClientId { get; set; }
        public string Name { get; set; }
        public Recipient Recipient { get; set; }

        public MessageRequest(string name, Recipient recipient)
        {
            ClientId = Guid.NewGuid();
            Name = name;
            Recipient = recipient;
        }
    }

    public class Recipient
    {
        public string Name { get; set; }
        public string Body { get; set; }

        public Recipient(string name, string body)
        {
            Name = name;
            Body = body;
        }
    }
}
