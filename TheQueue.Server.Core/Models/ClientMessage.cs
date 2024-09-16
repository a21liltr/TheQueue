using TheQueue.Server.Core.Enums;

namespace TheQueue.Server.Core.Models
{
    public class ClientMessage
    {
        public MessageType MessageType { get; set; }
        public string Name { get; set; }

        public ClientMessage(string name)
        {
            Name = name;
            MessageType = MessageType.Queue;
        }
    }
}
