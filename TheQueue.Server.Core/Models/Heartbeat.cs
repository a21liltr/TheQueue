namespace TheQueue.Server.Core.Models
{
    public class Heartbeat
    {
        public ConnectedClient Client { get; set; }
        public Heartbeat(ConnectedClient client)
        {
            Client = client;
        }
    }
}
