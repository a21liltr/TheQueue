namespace TheQueue.Server.Core.Models
{
    public class QueueTicket
    {
        public string Name { get; set; }
        public int Ticket { get; set; }

        public QueueTicket(string name, int ticketNumber)
        {
            Name = name;
            Ticket = ticketNumber;
        }
    }
}
