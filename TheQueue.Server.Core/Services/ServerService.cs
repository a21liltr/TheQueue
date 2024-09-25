using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using TheQueue.Server.Core.Models.ServerMessages;

namespace TheQueue.Server.Core.Services
{
	public class ServerService
	{
		private readonly ILogger<ServerService> _logger;
		private readonly IConfiguration _config;
		private List<QueueTicket> _queue;

		private bool serverIsRunning = true;

		private Queue<string> broadcastQueue; // type should be message

		public ServerService(ILogger<ServerService> logger,
			IConfiguration config)
		{
			_logger = logger;
			_config = config;
			_queue = new List<QueueTicket>();
			var test = _config.GetValue<string>("test");
		}

		public void RunServer()
		{
			var port = 5555;
            Task rrServer = Task.Run(() => { RunRequestReplyServer($"tcp://*:{port}"); });
			//Task psServer = Task.Run(() => { RunPubSubServer($"tcp://*:{port+1}"); });

			Task.WaitAll(rrServer);
		}

		public void ShutdownServer()
		{
			serverIsRunning = false;
		}

		private void RunRequestReplyServer(string address)
		{
			using (var responder = new ResponseSocket())
			{
				responder.Bind(address); //port?
				_logger.LogInformation("Server running");
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

					//QueueTicket received = DeserializeJSON(message);
					_logger.LogInformation($"Received message");

					// kolla valid medddelande
					// kolla vad meddelande
					// deserialze meddelande
					// gör saker

					QueueTicket test = new QueueTicket
					{
						Name = "test",
						Ticket = _queue.Select(t => t.Ticket).LastOrDefault() + 1
					};
					_queue.Add(test);

                    responder.SendFrame(JsonConvert.SerializeObject(test));
				}
			}
		}

		private void RunPubSubServer(string address)
		{
			using (var responder = new PublisherSocket())
			{
				responder.Bind(address); //port?
				while (serverIsRunning)
				{
					// peek at queue

					// if any -> dequeue and broadcast


					Thread.Sleep(1000);
				}
			}
		}

		private QueueTicket DeserializeJSON(string message)
		{
            return JsonConvert.DeserializeObject<QueueTicket>(message);
        }

		
	}
}