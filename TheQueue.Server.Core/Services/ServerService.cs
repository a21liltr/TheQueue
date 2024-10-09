using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System.Collections.Concurrent;
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

        private readonly int _port;
        private int _ticketCounter = 1;

        // TODO: use instead of strings
        //private static readonly string _topicQueue = "queue";
        //private static readonly string _topicSupervisors = "supervisors";

        private Queue<TopicMessage> _broadcastQueue;
        private List<ConnectedClient> _connectedClients;
        private List<QueueTicket> _queue;
        private List<Supervisor> _supervisors;
        private ConcurrentBag<QueueTicket> _ticketQueue;

        public ServerService(ILogger<ServerService> logger,
            IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _port = _config.GetValue<int>("Port");
            _broadcastQueue = new();

            _connectedClients = new();
            _ticketQueue = new();
            _ticketQueue.OrderBy(x => x.Ticket);
            string queuePath = Path.Combine(Environment.CurrentDirectory, "queue.json");
            var readQueueJson = File.ReadAllText(queuePath);
            if (!string.IsNullOrWhiteSpace(readQueueJson))
                _queue = JsonConvert.DeserializeObject<List<QueueTicket>>(readQueueJson);
            else
                _queue = new();

            string supervisorPath = Path.Combine(Environment.CurrentDirectory, "supervisors.json");
            var readSupervisorsJson = File.ReadAllText(supervisorPath);
            if (!string.IsNullOrWhiteSpace(readSupervisorsJson))
                _supervisors = JsonConvert.DeserializeObject<List<Supervisor>>(readSupervisorsJson);
            else
                _supervisors = new();
        }

        public void RunServer()
        {
            Task rrServer = Task.Run(() => { RunRequestReplyServer($"tcp://localhost:{_port + 1}"); });
            Task psServer = Task.Run(() => { RunPubSubServer($"tcp://localhost:{_port}"); });
            Task.WaitAll(rrServer, psServer);
        }

        public void ShutdownServer()
        {
            _serverIsRunning = false;
            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "queue.json"), "[]");
            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "supervisors.json"), "[]");
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
                            responder.SendFrame(CreateErrorMessage("Received bad message", ErrorType.Warning));
                            continue;
                        }

                        var received = JsonConvert.DeserializeObject<ClientMessage>(message);
                        if (received is null)
                        {
                            responder.SendFrame(CreateErrorMessage("Received bad message", ErrorType.Warning));
                            continue;
                        }
                        _logger.LogDebug("Deserialized message to ClientMessage object");

                        // validation on properties
                        if (string.IsNullOrWhiteSpace(received.ClientId))
                        {
                            responder.SendFrame(CreateErrorMessage("Missing information", ErrorType.Critical));
                        }

                        HandleConnect(received);

                        if (received.Supervisor.HasValue && received.Supervisor.Value)
                        {
                            try
                            {
                                CreateSupervisorIfNotExists(received);
                                if (received.EnterQueue.HasValue && received.EnterQueue.Value)
                                {
                                    responder.SendFrame(HandleSupervisorEnterQueue(received)); // returns queueticket or null
                                }
                                else
                                {
                                    SetSupervisorStatus(received.Name, Status.Pending);
                                    responder.SendFrame("{}");
                                }
                            }
                            catch (Exception ex)
                            {
                                responder.SendFrame(ex.Message); // returns error
                                continue;
                            }
                        }
                        else if (received.EnterQueue.HasValue)
                        {
                            var response = CreateStudentAndAddToQueueIfNotExists(received);
                            var responseMessage = JsonConvert.SerializeObject(response);
                            responder.SendFrame(responseMessage);
                        }
                        else if (received.Message is not null)
                        {
                            if (string.IsNullOrWhiteSpace(received.Name))
                            {
                                responder.SendFrame(CreateErrorMessage("Received bad message", ErrorType.Critical));
                                continue;
                            }
                            responder.SendFrame(HandleMessageRequest(received));
                        }
                        else
                        {
                            if (!HandleHeartbeat(received))
                            {
                                responder.SendFrame(
                                CreateErrorMessage("Heartbeat could not be tied to a connected client", ErrorType.Critical));
                            }
                            else
                            {
                                responder.SendFrame("{}");
                            }
                            continue;
                        }
                        SendBroadcast("queue", _queue);
                        SendBroadcast("supervisors", _supervisors);
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
                publisher.Bind(address);
                while (_serverIsRunning)
                {
                    try
                    {
                        if (_broadcastQueue.Count is not 0)
                        {
                            var message = _broadcastQueue.Dequeue();

                            publisher.SendMoreFrame(message.Topic);
                            publisher.SendFrame(message.Message);
                            _logger.LogInformation("Published {topic} : {message}", message.Topic, message.Message);
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
            if (!_connectedClients.Any(x => x.ClientId == message.ClientId))
            {
                _logger.LogInformation("Handling Connection for ClientId {client}", message.ClientId);
                ConnectedClient client = new()
                {
                    ClientId = message.ClientId,
                    Name = message.Name
                };
                client.OnDisconnect += OnDisconnect;
                _connectedClients.Add(client);
                _logger.LogInformation("Connected ClientId {client}", message.ClientId);
            }
        }

        private void CreateSupervisorIfNotExists(ClientMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.Name))
                throw new Exception(CreateErrorMessage("Supervisor Name not found", ErrorType.Warning));

            if (!_supervisors.Any(x => x.Name == message.Name))
            {
                Supervisor supervisor = new()
                {
                    Name = message.Name,
                    Status = Status.Pending
                };
                _supervisors.Add(supervisor);
            }
        }

        public void SetSupervisorStatus(string supervisorName, Status status)
        {
            Supervisor supervisor = _supervisors.FirstOrDefault(x => x.Name == supervisorName)
                ?? throw new Exception(CreateErrorMessage($"No Supervisor with name {supervisorName}", ErrorType.Warning));

            if (status is not Status.Occupied)
            {
                supervisor.Client = null;
            }
            supervisor.Status = status;
        }

        private string HandleSupervisorEnterQueue(ClientMessage message)
        {

            _logger.LogInformation("Getting student for Supervisor {supervisor}", message.Name);

            QueueTicket? queueTicket = _queue.FirstOrDefault();
            if (queueTicket is null)
            {
                _logger.LogInformation("No students available for supervision");
                SetSupervisorStatus(message.Name, Status.Available);
                return "{}";
            }
            SetSupervisorStatus(message.Name, Status.Occupied);
            return JsonConvert.SerializeObject(queueTicket);
        }

        private QueueTicket CreateStudentAndAddToQueueIfNotExists(ClientMessage message)
        {
            QueueTicket ticket = new()
            {
                Name = message.Name,
            };
            if (message.EnterQueue!.Value && !string.IsNullOrWhiteSpace(message.Name))
            {
                if (!_queue.Any(x => x.Name == message.Name))
                {
                    ticket.Ticket = _ticketCounter++;
                    _queue.Add(ticket);
                }
                else
                {
                    ticket = _queue.First(x => x.Name == message.Name);
                }
            }
            else
            {
                if (_queue.Any(x => x.Name == message.Name))
                {
                    _queue.Remove(_queue.First(x => x.Name == message.Name));
                }
            }
            return ticket;
        }

        private void OnDisconnect(string clientId, string name)
        {
            _logger.LogInformation("Client {clientId} : {clientName} Disconnect from expired heartbeat", clientId, name);
            var disconnectedClient = _connectedClients.FirstOrDefault(x => x.ClientId == clientId);
            if (disconnectedClient is null)
            {
                _logger.LogWarning("ClientId {wrongId} could not be found", clientId);
                return;
            }

            _connectedClients.Remove(disconnectedClient);
            disconnectedClient.Dispose();
            if (!_connectedClients.Any(x => x.Name == name))
            {
                if (_queue.Any(x => x.Name == name))
                {
                    _queue.Remove(_queue.First(x => x.Name == name));
                    SendBroadcast("queue", _queue);
                }

                if (_supervisors.Any(x => x.Name == name))
                {
                    _supervisors.Remove(_supervisors.First(x => x.Name == name));
                    SendBroadcast("supervisors", _supervisors);
                }
            }
        }

        private string HandleMessageRequest(ClientMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.Message.Body))
                return CreateErrorMessage("Received message with not body content to forward", ErrorType.Warning);

            Supervisor? supervisor = _supervisors.FirstOrDefault(x => x.Name == message.Name);
            if (supervisor is null)
                return CreateErrorMessage("No supervisor found", ErrorType.Critical);

            QueueTicket? student = _queue.FirstOrDefault(x => x.Name == message.Message.Recipient);
            if (student is null)
                return CreateErrorMessage("Recipient not found", ErrorType.Critical);

            supervisor.Client = student;
            UserMessages userMessage = new()
            {
                Supervisor = message.Name,
                Message = message.Message.Body
            };

            _queue.Remove(student);

            string topic = message.Message.Recipient;
            SendBroadcast(topic, userMessage);
            return "{}";
        }

        private void SendBroadcast(string topic, object? message)
        {
            var serialized = JsonConvert.SerializeObject(message, Formatting.Indented);
            if (topic is "queue" || topic is "supervisors")
            {
                File.WriteAllText(Path.Combine(Environment.CurrentDirectory, $"{topic}.json"), serialized);
            }

            TopicMessage broadcastMessage = new()
            {
                Topic = topic,
                Message = serialized
            };
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