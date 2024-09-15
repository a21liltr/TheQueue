using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using TheQueue.Server.Core.Models;
using System.Text.Json;
using TheQueue.Server.Core.Enums;

namespace TheQueue.Server.Core.Services
{
    public class ServerService
    {
        private readonly ILogger<ServerService> _logger;
        private readonly IConfiguration _config;
        private List<ClientMessage> _clientQueue;

        public ServerService(ILogger<ServerService> logger,
            IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _clientQueue = new List<ClientMessage>();
            var test = _config.GetValue<string>("test");
        }

        public void RunServer()
        {
            using (var responder = new ResponseSocket())
            {
                responder.Bind("tcp://*:5556");

                while (true)
                {
                    // Handle message
                    string message = responder.ReceiveFrameString();
                    _logger.LogInformation("Received message, {message}", message);

                    if (message is null)
                    {
                        _logger.LogInformation($"Received empty message");
                    }

                    // serialize to msg
                    ClientMessage deserialized = JsonSerializer.Deserialize<ClientMessage>(message);

                    // switch check msg type
                    MessageType messageType = deserialized.MessageType;
                    switch(messageType)
                    {
                        case MessageType.Connect:
                            AddClient(deserialized);
                            break;
                        case MessageType.Disconnect:
                            RemoveClient(deserialized.Name);
                            break;
                        case MessageType.Heartbeat:
                            // TODO:
                            // ResetHeartbeatTimer();
                            break;
                        case MessageType.Queue:
                            // TODO:
                            // AddToQueue();
                            break;
                        case MessageType.Dequeue:
                            // TODO:
                            // DequeueSelf();
                            break;
                        case MessageType.DequeueStudent:
                            // TODO:
                            // Supervisor only!
                            // check if authorised
                            // DequeueStudent();
                            break;
                        default:
                            _logger.LogWarning("Error: Could not find MessageType from incoming Message.");
                            break;
                    }


                    Console.WriteLine($"Received message:\n" +
                        $"\"{message}\"");

                    AddClient(new ClientMessage(message));

                    string queue = string.Empty;
                    foreach (var connected in _clientQueue)
                    {
                        queue += $"{connected.Name}\n";
                    }

                    responder.SendFrame($"Message \"{message}\" has been received." +
                        $"\nNew student with name {message} added to queue." +
                        $"\nStudents in queue:" +
                        $"{queue}");
                }
            }
        }

        public List<ClientMessage> GetAll()
        {
            return _clientQueue;
        }

        public List<ClientMessage> AddClient(ClientMessage student)
        {
            _clientQueue.Add(student);
            return _clientQueue;
        }
        public List<ClientMessage> RemoveClient(string name)
        {
            ClientMessage student = GetConnectedClient(name);
            if (name is null || student.Name == string.Empty)
                return _clientQueue;

            _clientQueue.Remove(student);

            return _clientQueue;
        }

        public ClientMessage GetConnectedClient(string name)
        {
            if (!_clientQueue.Exists(x => x.Name == name))
                return new ClientMessage(name);

            return _clientQueue.Find(s => s.Name == name)!;
        }

        private void ResetHeartbeatTimer(ConnectedClient client)
        {
            client.HeartbeatTimer = 0;
        }
    }
}