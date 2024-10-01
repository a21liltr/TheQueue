using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using TheQueue.Server.Core.Enums;
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

		private int _port;
		private Queue<string> _broadcastQueue;
		private List<ConnectedClient> _connectedClients;
		private List<QueueTicket> _queue;
		private List<Supervisor> _supervisors;

		public ServerService(ILogger<ServerService> logger,
			IConfiguration config)
		{
			_logger = logger;
			_config = config;
			_port = _config.GetValue<int>("Port");
			_broadcastQueue = new();
			_connectedClients = new();

            string queueJson = "queue.json";
            string queuePath = Path.Combine(Environment.CurrentDirectory, queueJson);
            var readQueueJson = File.ReadAllText(queuePath);
			if (!string.IsNullOrWhiteSpace(readQueueJson))
				_queue = JsonConvert.DeserializeObject<List<QueueTicket>>(readQueueJson);
			else
				_queue = new();

			string supervisorJson = "supervisors.json";
            string supervisorPath = Path.Combine(Environment.CurrentDirectory, supervisorJson);
            var readSupervisorsJson = File.ReadAllText(supervisorPath);
			if (!string.IsNullOrWhiteSpace(readSupervisorsJson))
				_supervisors = JsonConvert.DeserializeObject<List<Supervisor>>(readSupervisorsJson);
			else
				_supervisors = new();
		}

		public void RunServer()
		{
			Task rrServer = Task.Run(() => { RunRequestReplyServer($"tcp://localhost:{_port}"); });
			Task psServer = Task.Run(() => { RunPubSubServer($"tcp://localhost:{_port + 1}"); });
			Task.WaitAll(rrServer, psServer);
		}

		public void ShutdownServer()
		{
			_serverIsRunning = false;
			File.WriteAllText("queue.json", string.Empty);
			File.WriteAllText("supervisors.json", string.Empty);
		}

		private void RunRequestReplyServer(string address)
		{
			using (ResponseSocket responder = new())
			{
				responder.Bind(address);
				_logger.LogInformation("Server running");
				while (_serverIsRunning)
				{
					try
					{

						string message = responder.ReceiveFrameString();
						_logger.LogInformation("Received message, {message}", message);
						if (string.IsNullOrEmpty(message))
						{
							_logger.LogWarning("Received bad message");
							responder.SendFrame(CreateErrorMessage("Received bad message", ErrorType.Warning));
							continue;
						}

						var received = JsonConvert.DeserializeObject<ClientMessage>(message);
						if (received is null)
						{
							_logger.LogWarning("Received bad message");
							responder.SendFrame(CreateErrorMessage("Received bad message", ErrorType.Warning));
							continue;
						}
						_logger.LogDebug("Deserialized message to ClientMessage object");

						// validation on properties
						if (string.IsNullOrEmpty(received.ClientId))
						{
							_logger.LogError("Missing ClientId, {message}", message);
							responder.SendFrame(CreateErrorMessage("Missing ClientId", ErrorType.Critical));
						}

						HandleConnect(received);

						if (received.SuperVisor.HasValue && received.SuperVisor.Value)
						{
							var supervisorConnect = HandleSupervisorConnect(received);
							if (!string.IsNullOrEmpty(supervisorConnect))
								responder.SendFrame(supervisorConnect);
						}
						else if (received.EnterQueue.HasValue)
						{
							var response = HandleEnterQueueMessage(received);
							var responseMessage = JsonConvert.SerializeObject(response);
							responder.SendFrame(responseMessage);
						}
						else if (received.Message is not null)
						{
							if (string.IsNullOrWhiteSpace(received.Name))
							{
								_logger.LogError("Received message without contents to forward {message}", message);
								responder.SendFrame(CreateErrorMessage("Received message without contents to forward", ErrorType.Critical));
							}
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
						SendBroadcast("queue", _queue.ToArray());
						SendBroadcast("supervisors", _supervisors.ToArray());
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "An error occured: {errorMessage}", ex.Message);
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
					try
					{
						// peek at queue
						// if any -> dequeue and broadcast
						if (_broadcastQueue.Count is not 0)
						{
							var message = _broadcastQueue.Dequeue();

							publisher.SendFrame(message);
							_logger.LogInformation("Published {pubMsg}", message);
						}
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "An error occured: {errorMessage}", ex.Message);
					}
					finally
					{
						Thread.Sleep(1000);
					}
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

		private string HandleSupervisorConnect(ClientMessage message)
		{
			if (string.IsNullOrWhiteSpace(message.Name))
				return CreateErrorMessage("Supervisor Name not found", ErrorType.Warning);

			if (!_supervisors.Any(x => x.Name == message.Name))
			{
				Supervisor supervisor = new()
				{
					Name = message.Name,
					Status = Status.Available
				};
				_supervisors.Add(supervisor);
				//var broadcastMessage = "supervisors - " + JsonConvert.SerializeObject(_supervisors.ToArray());
				//_broadcastQueue.Enqueue(broadcastMessage);
			}

			if (message.Status.HasValue)
			{
				var supervisor = _supervisors.FirstOrDefault(x => x.Name == message.Name);
				if (supervisor is null)
				{
					return CreateErrorMessage("Supervisor not found", ErrorType.Warning);
				}

				switch (message.Status.Value)
				{
					case Status.Available:
					case Status.Pending:
						supervisor.Status = message.Status.Value;
						supervisor.Client = null;
						break;
					case Status.Occupied:
						supervisor.Status = Status.Occupied;
						supervisor.Client = message.QueueTicket;
						break;
					default:
						return CreateErrorMessage("Incorrect status", ErrorType.Warning);
				}
			}
			return string.Empty;
		}

		private QueueTicket HandleEnterQueueMessage(ClientMessage message)
		{
			if (message.EnterQueue!.Value && !string.IsNullOrWhiteSpace(message.Name))
			{
				if (!_queue.Any(x => x.Name == message.Name))
				{
					QueueTicket ticket = new()
					{
						Name = message.Name,
						Ticket = _queue.Count() + 1
					};
					_queue.Add(ticket);
					//var broadcastMessage = "queue - " + JsonConvert.SerializeObject(_queue.ToArray());
					//_broadcastQueue.Enqueue(broadcastMessage);
				}
			}
			var queueTicket = _queue.FirstOrDefault(x => x.Name == message.Name);

			if (queueTicket != null)
				return queueTicket;

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
				_logger.LogWarning("ClientId {wrongId} could not be found", clientId);

			if (disconnectedClient is not null)
			{
				_connectedClients.Remove(disconnectedClient);
				if (!_connectedClients.Any(x => x.Name == name))
				{
					_queue.Remove(_queue.First(x => x.Name == name));
					//var broadcastMessage = "queue - " + JsonConvert.SerializeObject(_queue.ToArray());
					//_broadcastQueue.Enqueue(broadcastMessage);
					SendBroadcast("queue", _queue.ToArray());
				}
			}
		}

		private void HandleMessageRequest(ClientMessage message)
		{
			if (string.IsNullOrWhiteSpace(message.Message.Body))
			{
				_logger.LogError("Received message with not body content to forward {message}", message);
				return;
			}

			UserMessages userMessage = new()
			{
				Supervisor = message.Name!,
				Message = message.Message.Body
			};
			//var supervisorMessage = JsonConvert.SerializeObject(userMessage);
			//var broadcastMessage = $"{message.Message.Recipient} - {supervisorMessage}";
			//_broadcastQueue.Enqueue(broadcastMessage);
			string topic = message.Message.Recipient;
			SendBroadcast(topic, userMessage);
		}

		private void SendBroadcast(string topic, object? message)
		{
			var serialized = JsonConvert.SerializeObject(message, Formatting.Indented);
			if (topic is "queue" || topic is "supervisors")
			{
				File.WriteAllText($"\\\\{topic}.json", serialized);
			}
			var broadcastMessage = $"{topic} - " + serialized;
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

		private string CreateErrorMessage(string message, ErrorType errorType)
		{
			ErrorMessage errorMessage = new()
			{
				Error = errorType,
				Msg = message
			};
			_logger.LogError("{errorType} An error occured: {errorMessage}", errorType, message);
			return JsonConvert.SerializeObject(errorMessage);
		}
	}
}