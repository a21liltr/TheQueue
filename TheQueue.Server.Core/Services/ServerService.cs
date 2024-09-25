using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System.Text;
using TheQueue.Server.Core.Models.BroadcastMessages;
using TheQueue.Server.Core.Models.ClientMessages;
using TheQueue.Server.Core.Models.ServerMessages;

namespace TheQueue.Server.Core.Services
{
	public class ServerService
	{
		private readonly ILogger<ServerService> _logger;
		private readonly IConfiguration _config;
		private List<QueueTicket> _queue;

		private bool _serverIsRunning = true;

		private Queue<string> _broadcastQueue; // type should be message

		public ServerService(ILogger<ServerService> logger,
			IConfiguration config)
		{
			_logger = logger;
			_config = config;
			_queue = new List<QueueTicket>();
			var test = _config.GetValue<string>("test");
			_broadcastQueue = new Queue<string>();
		}

		public void RunServer()
		{
			var port = 5555;
			//Task rrServer = Task.Run(() => { RunRequestReplyServer($"tcp://*:{port}"); });
			Task rrServer = Task.Run(() => { test(); });
			Task psServer = Task.Run(() => { RunPubSubServer($"tcp://localhost:{port + 1}"); });

			Task.WaitAll(rrServer, psServer);
		}

		public void ShutdownServer()
		{
			_serverIsRunning = false;
		}

		private void RunRequestReplyServer(string address)
		{
			using (var responder = new ResponseSocket())
			{
				responder.Bind(address); //port?
				_logger.LogInformation("Server running");
				while (_serverIsRunning)
				{
					string message = responder.ReceiveFrameString();
					_logger.LogInformation("Received message, {message}", message);

					if (string.IsNullOrEmpty(message))
					{
						_logger.LogWarning($"Received bad message");
						_ = responder.TrySignalError();
						continue;
					}

					// TODO: Handle properly.

					var received = (EnterQueueRequest)DeserializeJSON(message);
					_logger.LogInformation($"Received message");

					// kolla valid medddelande
					// kolla vad meddelande
					// deserialze meddelande
					// gör saker

					if (received.EnterQueue)
					{
						QueueTicket test = new QueueTicket
						{
							Name = "test",
							Ticket = _queue.Select(t => t.Ticket).LastOrDefault() + 1
						};
						_queue.Add(test);

						responder.SendFrame(JsonConvert.SerializeObject(test));
					}




					Thread.Sleep(1000);

				}
			}
		}

		private void RunPubSubServer(string address)
		{
			using (var publisher = new PublisherSocket())
			{
				publisher.Bind(address); //port?
				while (_serverIsRunning)
				{
					// peek at queue
					// if any -> dequeue and broadcast
					if (_broadcastQueue.Count is not 0)
					{
						var message = _broadcastQueue.Dequeue();
						
						publisher.SendFrame(message);
						_logger.LogInformation("Published {msg}", message);
					}

					Thread.Sleep(1000);
				}
			}
		}

		private void test()
		{
			while (true)
			{
				_broadcastQueue.Enqueue("queue - test message");
				_broadcastQueue.Enqueue("bla - bla message");
				_logger.LogInformation("Enqueue");

				Thread.Sleep(1000);
			}

		}

		private object DeserializeJSON(string json)
		{
			return JsonConvert.DeserializeObject(json);
		}
	}
}