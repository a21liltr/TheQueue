using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using TheQueue.Server.Core.Models;
using System.Text.Json;
using TheQueue.Server.Core.Enums;
using System.ServiceModel.Channels;

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
                        _logger.LogInformation($"Received empty message");

                    // serialize to msg
                    ClientMessage deserialized = JsonSerializer.Deserialize<ClientMessage>(message);

                    // switch check msg type
                    MessageType messageType = deserialized.MessageType;

                    // TODO: Handle properly.
                    // Check if exists
                    // Check what client wants to do
                    switch (messageType)
                    {
                        case MessageType.Connect:
                            // TODO: Return list of queued clients ?
                            // Should be able to see list before queueing ?
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
                            AddClient(deserialized);
                            // AddToQueue();
                            break;
                        case MessageType.Dequeue:
                            // TODO: If doesn't need help (anymore) but want to be connected and see queue.
                            // DequeueSelf();
                            break;
                        case MessageType.DequeueStudent:
                            // TODO:
                            // Supervisor only!
                            // check if authorised
                            // DequeueStudent();
                            break;
                        default:
                            //_logger.LogWarning("Error: Could not find MessageType from incoming Message.");
                            _logger.LogInformation("Case for {messageType} not implementet yet", messageType);
                            break;
                    }

                    Console.WriteLine($"Received message with type {messageType}:\n" +
                        $"from \"{message}\"");

                    responder.SendFrame(GetQueue(message));
                }
            }
        }

        public string GetQueue(string message)
        {
            string queue = string.Empty;
            foreach (var connected in _clientQueue)
            {
                queue += $"\n{connected.Name}\n";
            }

            return $"Message \"{message}\" has been received." +
                $"\nNew student with name {message} added to queue." +
                $"\nStudents in queue:" +
                $"{queue}";
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