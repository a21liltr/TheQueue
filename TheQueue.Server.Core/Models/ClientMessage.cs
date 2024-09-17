using TheQueue.Server.Core.Enums;

namespace TheQueue.Server.Core.Models
{
    public class ClientMessage
    {
        public MessageType MessageType { get; set; }
        public ConnectedClient Client { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }

        public ClientMessage(
            ConnectedClient client,
            MessageType messageType)
        {
            Client = client;
            Name = client.Name;
            MessageType = messageType;
        }

        public ClientMessage(
            ConnectedClient client,
            MessageType messageType,
            string message)
        {
            Client = client;
            Name = client.Name;
            MessageType = messageType;
            Message = message;
        }
    }
}
