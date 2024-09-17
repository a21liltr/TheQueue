namespace TheQueue.Server.Core.Models
{
    public class QueueTicket
    {
        public ConnectedClient Client { get; set; }
        public int Ticket { get; set; }
        public QueueTicket(ConnectedClient client, int ticketNumber)
        {
            Client = client;
            Ticket = ticketNumber;
        }
    }
}
