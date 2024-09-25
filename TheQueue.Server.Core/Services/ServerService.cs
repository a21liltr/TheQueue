using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using TheQueue.Server.Core.Models;
using TheQueue.Server.Core.Models.ClientMessages;
using TheQueue.Server.Core.Models.ServerMessages;

namespace TheQueue.Server.Core.Services
{
	public class ServerService
	{
		private readonly ILogger<ServerService> _logger;
		private readonly IConfiguration _config;

		private bool _serverIsRunning = true;

		private Queue<string> _broadcastQueue; // type should be message
		private List<QueuedClient> _connectedClients; // how supervisor connect?
		private List<string> _queue; // queue of names

		public ServerService(ILogger<ServerService> logger,
			IConfiguration config)
		{
			_logger = logger;
			_config = config;
			var test = _config.GetValue<string>("test");
			_broadcastQueue = new Queue<string>();
			_connectedClients = new List<QueuedClient>();
			_queue = new List<string>();
		}

		public void RunServer()
		{
			var port = 5555;
			//Task rrServer = Task.Run(() => { RunRequestReplyServer($"tcp://localhost:{port}"); });
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
						_ = responder.TrySignalError(); // change to error message with sendframes
						continue;
					}

					// TODO: Handle properly.

					var received = JsonConvert.DeserializeObject<ClientMessage>(message);
					if (received == null)
					{
						_logger.LogWarning($"Received bad message");
						_ = responder.TrySignalError();
						continue;
					}

					_logger.LogDebug($"Deserialized message to ClientMessage object");

					// validation on properties

					if (received.EnterQueue.HasValue)
					{
						var response = HandleEnterQueueMessage(received);
						string responseMessage = JsonConvert.SerializeObject(response);
						responder.SendFrame(responseMessage);
					}
					else if (received.Message is not null)
					{
						HandleMessageRequest(received);
					}
					else
					{
						if (!HandleHeartbeat(received))
						{
							_logger.LogWarning($"Received bad heartbeat");
							_ = responder.TrySignalError();
							continue;
						}
						responder.SendFrame("{}");
					}
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

		private QueueTicket HandleEnterQueueMessage(ClientMessage message)
		{
			if (!_connectedClients.Any(x => x.ClientId == message.ClientId))
			{
				var client = new QueuedClient();
				client.ClientId = message.ClientId;
				client.Name = message.Name ?? ""; // handle properly
				client.OnDisconnect += OnDisconnect;

				_connectedClients.Add(client);
			}

			if (message.EnterQueue.Value)
			{
				// add to queue
				if (!_queue.Any(x => x == message.Name))
				{
					_queue.Add(message.Name);
				}
				// broadcast updated queue

			}
			return new QueueTicket
			{
				Name = message.Name,
				Ticket = _queue.IndexOf(message.Name) + 1
			};
		}

		private void OnDisconnect(string clientId)
		{
			var disconnectedClient = _connectedClients.FirstOrDefault(x => x.ClientId == clientId);
			_connectedClients.Remove(disconnectedClient);

			// if need broadcast - do
		}

		private void HandleMessageRequest(ClientMessage message)
		{

		}

		private bool HandleHeartbeat(ClientMessage message)
		{
			var client = _connectedClients.FirstOrDefault(x => x.ClientId == message.ClientId);
			if (client is null)
			{
				return false;
			}
			client.OnHeartbeat();
			return true;
		}

		// create method createerrormessage
		// parameter errortype and errormessage
		// log error
		// create error message object
		// return error message

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
	}
}