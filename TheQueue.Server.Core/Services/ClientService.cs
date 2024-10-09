﻿using TheQueue.Server.Core.Models.ClientMessages;
using TheQueue.Server.Core.Models;
using Microsoft.Extensions.Logging;

namespace TheQueue.Server.Core.Services
{
    public class ClientService
    {
        private StudentService _studentService;
        private SupervisorService _supervisorService;
        private QueueService _queueService;
        private readonly ILogger<ClientService> _logger;

        public ConcurrentList<ConnectedClient> _connectedClients;

        public ClientService(StudentService studentService, SupervisorService supervisorService, QueueService queueService, ILogger<ClientService> logger)
        {
            _studentService = studentService;
            _supervisorService = supervisorService;
            _queueService = queueService;
            _logger = logger;
            _connectedClients = new();
        }

        public void HandleConnect(ClientMessage message)
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
                if (_studentService._queue.Any(x => x.Name == name))
                {
                    _studentService._queue.Remove(_studentService._queue.First(x => x.Name == name));
                    _queueService.SendBroadcast("queue", _studentService._queue);
                }

                if (_supervisorService._supervisors.Any(x => x.Name == name))
                {
                    _supervisorService._supervisors.Remove(_supervisorService._supervisors.First(x => x.Name == name));
                    _queueService.SendBroadcast("supervisors", _supervisorService._supervisors);
                }
            }
        }

        public bool HandleHeartbeat(ClientMessage message)
        {
            var client = _connectedClients.FirstOrDefault(x => x.ClientId == message.ClientId);
            if (client is null)
            {
                return false;
            }
            client.OnHeartbeat();
            return true;
        }
    }
}