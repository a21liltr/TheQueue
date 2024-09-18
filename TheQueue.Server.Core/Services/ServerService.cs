using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace TheQueue.Server.Core.Services
{
	public class ServerService
	{
		private readonly ILogger<ServerService> _logger;
		private readonly IConfiguration _config;
		private List<string> _clientQueue;

		private bool serverIsRunning = true;

		private Queue<string> broadcastQueue; // type should be message

		public ServerService(ILogger<ServerService> logger,
			IConfiguration config)
		{
			_logger = logger;
			_config = config;
			_clientQueue = new List<string>();
			var test = _config.GetValue<string>("test");
		}

		public void RunServer(string address)
		{
			Task rrServer = Task.Run(() => { RunRequestReplyServer(address); });
			Task psServer = Task.Run(() => { RunPubSubServer(address); });

			Task.WaitAll(rrServer, psServer);
		}

		public void ShutdownServer()
		{
			serverIsRunning = false;
		}

		private void RunRequestReplyServer(string address)
		{
			using (var responder = new ResponseSocket())
			{
				responder.Bind("tcp://*:5555"); //port?

				while (serverIsRunning)
				{
					string message = responder.ReceiveFrameString();
					_logger.LogInformation("Received message, {message}", message);

					if (message is null)
					{
						_logger.LogInformation($"Received empty message");
						continue;
					}

					// TODO: Handle properly.


					_logger.LogInformation($"Received message");

					responder.SendFrame($"Server has received message:\n{message}");
				}
			}
		}

		private void RunPubSubServer(string address)
		{
			using (var responder = new PublisherSocket())
			{
				responder.Bind("tcp://*:5555"); //port?
				while (serverIsRunning)
				{
					// peek at queue

					// if any -> dequeue and broadcast


					Thread.Sleep(1000);
				}
			}
		}
	}
}