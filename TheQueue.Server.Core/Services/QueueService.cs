using NetMQ;
using Newtonsoft.Json;
using TheQueue.Server.Core.Models.BroadcastMessages;

namespace TheQueue.Server.Core.Services
{
    public class QueueService : IDisposable
    {
        public NetMQQueue<TopicMessage> broadcastQueue;
        private bool disposedValue;

        public QueueService()
        {
            broadcastQueue = new NetMQQueue<TopicMessage>();
        }

        public void SendBroadcast(string topic, object? message)
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
            broadcastQueue.Enqueue(broadcastMessage);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    broadcastQueue.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
