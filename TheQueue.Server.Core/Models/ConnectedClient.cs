namespace TheQueue.Server.Core.Models
{
    public class ConnectedClient
    {
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;

        public ConnectedClient(string name)
        {
            ClientId = Guid.NewGuid();
            ClientName = name;
        }
    }
}
