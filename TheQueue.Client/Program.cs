using NetMQ;
using NetMQ.Sockets;

static class Program
{
	public static void Main()
	{
		PublishSubscriber();
		//RequestReply();
	}

	private static void PublishSubscriber()
	{
		using (var client = new SubscriberSocket())
		{
			client.Connect("tcp://localhost:5556");

			client.Subscribe("queue");
			while (true)
			{
				var msg = client.ReceiveFrameString();
				Console.WriteLine("{0}", msg);
			}
		}
	}

	private static void RequestReply()
	{
		using (var client = new RequestSocket())
		{
			client.Connect("tcp://localhost:5555");

			string? sendMessage = Console.ReadLine();
			if (string.IsNullOrEmpty(sendMessage))
				sendMessage = "test string";

			client.SendFrame(sendMessage);
			string str = client.ReceiveFrameString();

			Console.WriteLine("{0}", str);
		}
	}
}
