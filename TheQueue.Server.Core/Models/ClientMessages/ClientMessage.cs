namespace TheQueue.Server.Core.Models.ClientMessages
{
	public class ClientMessage
	{
		public string ClientId { get; set; }
		public string? Name { get; set; }
		public bool? EnterQueue { get; set; }
		public bool? SuperVisor { get; set; }
		public Recipient? Message { get; set; }
	}
}
