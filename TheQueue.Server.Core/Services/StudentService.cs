﻿using Newtonsoft.Json;
using TheQueue.Server.Core.Models;
using TheQueue.Server.Core.Models.ClientMessages;
using TheQueue.Server.Core.Models.ServerMessages;

namespace TheQueue.Server.Core.Services
{
    public class StudentService
    {
        public ConcurrentList<QueueTicket> _queue;

        public StudentService()
        {
            _queue = new();
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "queue.json")))
            {
                _queue = JsonConvert.DeserializeObject<ConcurrentList<QueueTicket>>(
                File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "queue.json"))) ?? new();
            }
        }

        public QueueTicket CreateStudentAndAddToQueueIfNotExists(ClientMessage message)
        {
            QueueTicket ticket = new()
            {
                Name = message.Name,
            };

            if (message.EnterQueue!.Value && !string.IsNullOrWhiteSpace(message.Name))
            {
                if (!_queue.Any(x => x.Name == message.Name))
                {
                    ticket.Ticket = _queue.LastOrDefault()?.Ticket + 1 ?? 1;
                    _queue.Add(ticket);
                }
                else
                {
                    ticket = _queue.First(x => x.Name == message.Name);
                }
            }

            return ticket;
        }
    }
}
