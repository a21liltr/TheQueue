using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using TheQueue.Server.Core.Models;
using TheQueue.Server.Core.Models.BroadcastMessages;
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
		private List<ConnectedClient> _connectedClients; // how supervisor connect?
		private List<QueueTicket> _queue; // queue of names
		private List<Supervisor> _supervisors;

		public ServerService(ILogger<ServerService> logger,
			IConfiguration config)
		{
			_logger = logger;
			_config = config;
			var test = _config.GetValue<string>("test");
			_broadcastQueue = new();
			_connectedClients = new();
			_queue = new();
			_supervisors = new();
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
			using (ResponseSocket responder = new())
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
					HandleConnect(received);

					if (received.SuperVisor.HasValue && received.SuperVisor.Value)
					{
						// upate supervisor list
						if (received.SuperVisor.Value && !_supervisors.Any(x => x.Name == received.Name))
						{
							Supervisor supervisor = new()
							{
								Name = received.Name,
								Status = Enums.Status.Available
							};
							_supervisors.Add(supervisor);
							var broadcastMessage = "supervisors - " + JsonConvert.SerializeObject(_supervisors.ToArray());
							_broadcastQueue.Enqueue(broadcastMessage);
						}
					}
					else if (received.EnterQueue.HasValue)
					{
						var response = HandleEnterQueueMessage(received);
						var responseMessage = JsonConvert.SerializeObject(response);
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
							var error = new ErrorMessage()
							{
								Error = Enums.ErrorType.Critical,
								Msg = "Heartbeat could not be tied to a connected client."
							};
							responder.SendFrame(JsonConvert.SerializeObject(error));
							continue;
						}
						responder.SendFrame("{}");
					}
				}
			}
		}

		private void RunPubSubServer(string address)
		{
			using (PublisherSocket publisher = new())
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

		private void HandleConnect(ClientMessage message)
		{
			_logger.LogInformation("Handling Connection for ClientId {client}", message.ClientId);
			if (!_connectedClients.Any(x => x.ClientId == message.ClientId))
			{
				ConnectedClient client = new()
				{
					ClientId = message.ClientId,
					Name = message.Name ?? $"Client_{_queue.Count + 1}"
				};
				client.OnDisconnect += OnDisconnect;
				_connectedClients.Add(client);
				_logger.LogInformation("Connected ClientId {client}", message.ClientId);
			}
		}

		private QueueTicket HandleEnterQueueMessage(ClientMessage message)
		{
			if (message.EnterQueue.Value)
			{
				// add to queue
				if (!_queue.Any(x => x.Name == message.Name))
				{
					QueueTicket ticket = new()
					{
						Name = message.Name,
						Ticket = _queue.Count() + 1
					};
					_queue.Add(ticket);
					var broadcastMessage = "queue - " + JsonConvert.SerializeObject(_queue.ToArray());
					_broadcastQueue.Enqueue(broadcastMessage);
				}
			}
			var queueTicket = _queue.FirstOrDefault(x => x.Name == message.Name);

			if (queueTicket != null)
			{
				return queueTicket;
			}

			return new QueueTicket
			{
				Name = message.Name ?? message.ClientId,
				Ticket = -1
			};
		}

		private void OnDisconnect(string clientId, string name)
		{
			var disconnectedClient = _connectedClients.FirstOrDefault(x => x.ClientId == clientId);
			if (disconnectedClient is null)
			{
				_logger.LogWarning("ClientId {wrongId} could not be found", clientId);
			}

			if (disconnectedClient is not null)
			{
				_connectedClients.Remove(disconnectedClient);
				if (!_connectedClients.Any(x => x.Name == name))
				{
					_queue.Remove(_queue.First(x => x.Name == name));
					var broadcastMessage = "queue - " + JsonConvert.SerializeObject(_queue.ToArray());
					_broadcastQueue.Enqueue(broadcastMessage);
				}
			}
		}

		private void HandleMessageRequest(ClientMessage message)
		{
			UserMessages userMessage = new()
			{
				Supervisor = message.Name,
				Message = message.Message.Body
			};
			var supervisorMessage = JsonConvert.SerializeObject(userMessage);
			var broadcastMessage = $"{message.Message.Recipient} - {supervisorMessage}";
			_broadcastQueue.Enqueue(broadcastMessage);
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